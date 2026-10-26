using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Windowing;

/// <summary>Repairs X11 popup watches after mapping, WM grabs and owner geometry changes.</summary>
internal sealed class IslandsX11PopupDismissal
{
    private static readonly ConditionalWeakTable<Window, IslandsX11PopupDismissal> Attached = new();

    internal static readonly MewProperty<bool> EnabledProperty = MewProperty<bool>.Register<Window>(
        "IslandsX11PopupDismissalEnabled", false, MewPropertyOptions.None,
        static (window, _, enabled) =>
        {
            if (enabled && OperatingSystem.IsLinux())
            {
                Attached.GetValue(window, static owner => new IslandsX11PopupDismissal(owner));
            }
        });

    private readonly Window _window;
    private Point? _position;
    private bool _watchRearmed;

    private IslandsX11PopupDismissal(Window window)
    {
        _window = window;
        window.NativeMessage += OnNativeMessage;
        window.Loaded += () => _position = window.Position;
    }

    private void OnNativeMessage(NativeMessageEventArgs args)
    {
        try
        {
            ProcessNativeMessage(args);
        }
        catch (Exception exception)
        {
            // An optional framework workaround must not abort the native event loop. Disable
            // this window's hook after a contract failure instead of throwing on every event.
            _window.NativeMessage -= OnNativeMessage;
            Trace.TraceError("Islands X11 popup workaround disabled: {0}", exception);
        }
    }

    private void ProcessNativeMessage(NativeMessageEventArgs args)
    {
        if (!X11Message.TryRead(args, out int type, out nint pointer)) return;

        if (type == 12 && !_watchRearmed && HoldsPopupWatch(_window)) // First Expose
        {
            // XGrabPointer can fail while a just-shown surface is not viewable yet. MewUI ignores
            // the return code and its host does not dispatch MapNotify. The first Expose confirms
            // visibility; retry the existing watch once, without reacting to later repaints.
            _watchRearmed = true;
            RecapturePopupSurface(_window);
        }
        else if (type == 8 && pointer != 0 && HoldsPopupWatch(_window)) // LeaveNotify
        {
            var crossing = Marshal.PtrToStructure<XCrossingEvent>(pointer);
            if (crossing.Mode == 2) // NotifyUngrab: the platform has taken our pointer watch away.
            {
                IslandsWindowPopupDismissal.DismissPopup(_window);
            }
        }
        else if (type == 10 && pointer != 0) // FocusOut
        {
            // WM move/menu loops grab the keyboard without deactivating the managed owner.
            // Normal focus changes already go through MewUI's deactivation policy.
            if (IsWindowManagerFocusGrab(pointer))
            {
                DismissOwnedPopups();
            }
        }
        else if (type == 22 && pointer != 0) // ConfigureNotify
        {
            var configure = Marshal.PtrToStructure<XConfigureEvent>(pointer);
            if (configure.Window != _window.Handle) return;

            // X11's x/y can be parent-relative or synthetic screen coordinates. Query the public
            // position instead of comparing those two coordinate spaces. No per-frame polling.
            var position = _window.Position;
            bool moved = _position is { } previous && previous != position;
            _position = position;
            bool resized = Math.Abs(configure.Width - _window.ClientSize.Width * _window.DpiScale) > 1
                || Math.Abs(configure.Height - _window.ClientSize.Height * _window.DpiScale) > 1;
            if (moved || resized) DismissOwnedPopups();
        }

        // Never consume native events: moving, resizing, activation and input still reach MewUI.
    }

    private void DismissOwnedPopups()
    {
        if (Application.IsRunning)
        {
            DismissOwnedPopups(_window, Application.Current.AllWindows);
        }
    }

    internal static void DismissOwnedPopups(Window owner, IReadOnlyList<Window> windows)
    {
        // A dismissal mutates AllWindows and can close an entire submenu chain.
        foreach (var popup in windows.Where(candidate => IslandsWindowPopupDismissal.IsOwnedPopup(owner, candidate)).Reverse().ToArray())
        {
            // Keep the framework's StaysOpen policy and owner notifications.
            IslandsWindowPopupDismissal.DismissPopup(popup);
        }
    }

    internal static bool IsWindowManagerFocusGrab(nint pointer)
    {
        var focus = Marshal.PtrToStructure<XFocusEvent>(pointer);
        return focus.Mode == 1 && focus.Detail is not (2 or 5);
    }

    internal static bool HoldsPopupWatch(Window window) =>
        ReferenceEquals(CaptureHolder(null!), window) && ArmedWatches(null!)?.Contains(window) == true;

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_osCaptureHolder")]
    internal static extern ref Window? CaptureHolder(Window type);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_armedWatches")]
    internal static extern ref List<Window>? ArmedWatches(Window type);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "RecapturePopupSurface")]
    private static extern void RecapturePopupSurface(Window window);

    // X11 uses native C long/pointer widths. These prefixes work on both 32-bit and 64-bit Linux.
    [StructLayout(LayoutKind.Sequential)]
    private struct XFocusEvent
    {
        public int Type;
        public nuint Serial;
        public int SendEvent;
        public nint Display, Window;
        public int Mode, Detail;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XCrossingEvent
    {
        public int Type;
        public nuint Serial;
        public int SendEvent;
        public nint Display, Window, Root, Subwindow;
        public nuint Time;
        public int X, Y, RootX, RootY, Mode, Detail;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XConfigureEvent
    {
        public int Type;
        public nuint Serial;
        public int SendEvent;
        public nint Display, Event, Window;
        public int X, Y, Width, Height;
    }

    /// <summary>Reads the optional platform assembly's public event contract without a runtime dependency.</summary>
    internal static class X11Message
    {
        private static PropertyInfo? _type;
        private static PropertyInfo? _pointer;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties,
            "Aprillz.MewUI.X11NativeMessageEventArgs", "Aprillz.MewUI.Platform.X11")]
        [UnconditionalSuppressMessage("Trimming", "IL2075",
            Justification = "The exact X11 event type's public properties are preserved above; all other types are rejected.")]
        internal static bool TryRead(NativeMessageEventArgs args, out int type, out nint pointer)
        {
            type = 0;
            pointer = 0;
            if (args.GetType().FullName != "Aprillz.MewUI.X11NativeMessageEventArgs") return false;

            _type ??= args.GetType().GetProperty("EventType");
            _pointer ??= args.GetType().GetProperty("EventPointer");
            if (_type?.GetValue(args) is not int eventType) return false;
            type = eventType;
            // Most native events need no payload, avoiding a second reflected read for motion/rendering.
            if (type is 8 or 10 or 22 && _pointer?.GetValue(args) is nint eventPointer)
            {
                pointer = eventPointer;
            }
            return true;
        }
    }
}

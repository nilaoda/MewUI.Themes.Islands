using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Windowing;

/// <summary>
/// One Win32 subclass for initial placement, popup dismissal and Windows 11 Snap Layouts over custom chrome.
/// </summary>
/// <remarks>
/// <para>
/// Windows shows the layout flyout for a window whose maximize button it can find, and it finds that button
/// by asking the window where it is: the window answers <c>WM_NCHITTEST</c> with <c>HTMAXBUTTON</c> and the
/// shell does the rest. A window that drew its own title bar therefore has to answer for itself, which is
/// what this does. The check is the build, not the theme: 21H2 (22000) is where the flyout arrives.
/// </para>
/// <para>
/// Answering has a price, and it is the whole reason this is not two lines. Once a point belongs to the
/// non-client area the client stops receiving the pointer there, so the button's own hover and press would
/// never arrive and it would sit inert while the system took its clicks. The non-client messages carry the
/// same information, so they are read here and handed to the button — which is the bargain any framework
/// that offers this declaratively has to make internally, and one this theme makes for itself because the
/// framework it sits on has no property for it.
/// </para>
/// <para>
/// The click is deliberately left to the system. <c>DefWindowProc</c> already knows what a
/// <c>HTMAXBUTTON</c> press means — maximize, restore, and the drag-to-snap gesture that starts from it —
/// and nothing is suppressed here, so the window keeps all of it. What this adds is the hit test and the
/// two states the button needs to keep looking alive.
/// </para>
/// </remarks>
internal sealed class IslandsWindowNativeHook
{
    private const uint WmNcHitTest = 0x0084;
    private const uint WmNcMouseMove = 0x00A0;
    private const uint WmNcLButtonDown = 0x00A1;
    private const uint WmNcLButtonUp = 0x00A2;
    private const uint WmNcMouseLeave = 0x02A2;
    private const uint WmNcDestroy = 0x0082;
    private const uint WmWindowPosChanging = 0x0046;

    /// <summary>The answer that hands a point to the system's maximize button.</summary>
    private const int HtMaxButton = 9;

    private const uint TmeLeave = 0x0002;
    private const uint TmeNonClient = 0x0010;

    /// <summary>Identifies this hook among any others the window carries; the value only has to be ours.</summary>
    private static readonly nuint SubclassId = 0x49736C31;

    /// <summary>The hook's entry point, in the shape comctl32 calls a subclass procedure with.</summary>
    private static readonly nint Proc;

    private readonly Window _window;
    private readonly Action<nint>? _positionChanging;
    private readonly Func<UIElement?, bool>? _isTitleBarDragTarget;
    private UIElement? _button;
    private Action<bool>? _setHot;
    private Action<bool>? _setPressed;

    private GCHandle _self;
    private bool _tracking;

    static IslandsWindowNativeHook()
    {
        unsafe
        {
            Proc = (nint)(delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nuint, nint, nint>)&WindowProc;
        }
    }

    internal IslandsWindowNativeHook(Window window, Action<nint>? positionChanging = null,
        Func<UIElement?, bool>? isTitleBarDragTarget = null)
    {
        _window = window;
        _positionChanging = positionChanging;
        _isTitleBarDragTarget = isTitleBarDragTarget;
    }

    /// <summary>Rebinds the existing hook when the title bar is rebuilt.</summary>
    internal void UpdateTarget(UIElement button, Action<bool> setHot, Action<bool> setPressed)
    {
        _button = button;
        _setHot = setHot;
        _setPressed = setPressed;
    }

    /// <summary>Whether this system has Snap Layouts at all: Windows 11, build 22000 or later.</summary>
    internal static bool IsSupported =>
        OperatingSystem.IsWindows() && Environment.OSVersion.Version.Build >= 22000;

    /// <summary>Installs one hook per window, once its native handle exists.</summary>
    internal bool TryInstall()
    {
        if (!OperatingSystem.IsWindows() || _self.IsAllocated)
        {
            return true;
        }

        if (_window.Handle == 0)
        {
            return false;
        }

        return Install();
    }

    private bool Install()
    {
        _self = GCHandle.Alloc(this);

        if (SetWindowSubclass(_window.Handle, Proc, SubclassId, GCHandle.ToIntPtr(_self)))
        {
            return true;
        }

        // A hook that did not go in leaves the window exactly as it was — the strip's own button still takes
        // the click — so there is nothing to undo but the handle, and nothing to report. Retrying would not
        // help, which is why this still counts as done.
        _self.Free();
        return true;
    }

    /// <summary>
    /// The window's messages, on their way past.
    /// </summary>
    /// <remarks>
    /// Reached through the handle the hook was installed with rather than a lookup keyed by window: the
    /// message carries it, so there is nothing to synchronise and nothing to leak if a window is destroyed
    /// without being asked.
    /// </remarks>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static nint WindowProc(nint hwnd, uint msg, nint wParam, nint lParam, nuint id, nint refData)
    {
        // A managed exception must never escape a native window procedure: Windows terminates the process.
        try
        {
            if (refData != 0 && GCHandle.FromIntPtr(refData).Target is IslandsWindowNativeHook snap)
            {
                if (msg == WmNcDestroy)
                {
                    snap.Release(hwnd);
                }
                else if (msg == WmWindowPosChanging)
                {
                    snap._positionChanging?.Invoke(lParam);
                }
                else if (snap._isTitleBarDragTarget is { } isDragTarget
                    && IslandsWindowPopupDismissal.TryDismiss(snap._window, msg, wParam, lParam, isDragTarget))
                {
                    return 0;
                }
                else if (IsSupported && snap.Answer(msg, wParam, lParam, out nint result))
                {
                    return result;
                }
            }
        }
        catch (Exception exception)
        {
            Trace.TraceError("Islands window message 0x{0:X}: {1}", msg, exception);
        }

        return DefSubclassProc(hwnd, msg, wParam, lParam);
    }

    /// <summary>
    /// Answers a message, and says whether the answer stands in place of the default one.
    /// </summary>
    internal bool Answer(uint msg, nint wParam, nint lParam, out nint result)
    {
        result = 0;

        switch (msg)
        {
            case WmNcHitTest:
                if (!OverButton(lParam))
                {
                    return false;
                }

                // Handled, so the framework's own answer never runs: it would call this stretch of the strip
                // a caption and offer the system nothing to put a flyout on.
                result = HtMaxButton;
                return true;

            case WmNcMouseMove:
                bool over = wParam == HtMaxButton;
                _setHot?.Invoke(over);
                if (over)
                {
                    TrackLeave();
                }
                else
                {
                    // Dragged off the button: the system's own buttons stop looking pressed there too.
                    _setPressed?.Invoke(false);
                }

                return false;

            case WmNcLButtonDown:
                if (wParam == HtMaxButton)
                {
                    _setPressed?.Invoke(true);
                }

                return false;

            case WmNcLButtonUp:
                _setPressed?.Invoke(false);
                return false;

            case WmNcMouseLeave:
                _tracking = false;
                _setHot?.Invoke(false);
                _setPressed?.Invoke(false);
                return false;
        }

        return false;
    }

    /// <summary>
    /// Whether a screen point from a message falls on the window button.
    /// </summary>
    /// <remarks>
    /// The button's own rectangle is asked for in screen pixels rather than rebuilt from the client point:
    /// the framework's conversion already accounts for the window's inset, its scale and any host in
    /// between, and a second copy of that arithmetic would be a second thing to keep right. It is worked out
    /// per message rather than cached because the strip is laid out again whenever the window is resized or
    /// the interface language changes, and a stale rectangle would answer for a button that had moved.
    /// </remarks>
    private bool OverButton(nint lParam)
    {
        // A rebuilt strip may not be attached yet; never translate a detached visual to screen coordinates.
        // Windows offers no flyout for a window it cannot maximize, and neither does this. Whether the window
        // is also resizable is deliberately not asked: the framework keeps that inside its sizing mode, and a
        // window that can be maximized but not snapped is one Windows declines to offer a flyout for anyway.
        if (!_window.CanMaximize || _window.Handle == 0 || _button is not { IsVisible: true } button ||
            !ReferenceEquals(button.FindVisualRoot(), _window))
        {
            return false;
        }

        var size = button.RenderSize;
        if (size.Width <= 0 || size.Height <= 0)
        {
            return false;
        }

        return button.RectToScreen(new Rect(0, 0, size.Width, size.Height)).Contains(ScreenPoint(lParam));
    }

    /// <summary>The screen point a mouse message carries: two signed 16-bit halves of its lParam.</summary>
    private static Point ScreenPoint(nint lParam)
    {
        long packed = lParam.ToInt64();
        return new Point((short)(packed & 0xFFFF), (short)((packed >> 16) & 0xFFFF));
    }

    /// <summary>
    /// Asks for one message when the pointer leaves the window.
    /// </summary>
    /// <remarks>
    /// The only way to hear about it while the pointer is in the non-client area, and without it a button
    /// left under a pointer that walked off the window would stay lit.
    /// </remarks>
    private void TrackLeave()
    {
        if (_tracking)
        {
            return;
        }

        var data = new TrackMouseEventData
        {
            Size = Marshal.SizeOf<TrackMouseEventData>(),
            Flags = TmeLeave | TmeNonClient,
            Track = _window.Handle,
        };

        _tracking = TrackMouseEvent(ref data);
    }

    private void Release(nint hwnd)
    {
        RemoveWindowSubclass(hwnd, Proc, SubclassId);

        if (_self.IsAllocated)
        {
            _self.Free();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TrackMouseEventData
    {
        public int Size;
        public uint Flags;
        public nint Track;
        public uint HoverTime;
    }

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowSubclass(nint hwnd, nint proc, nuint id, nint refData);

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveWindowSubclass(nint hwnd, nint proc, nuint id);

    [DllImport("comctl32.dll", ExactSpelling = true)]
    private static extern nint DefSubclassProc(nint hwnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TrackMouseEvent(ref TrackMouseEventData data);
}

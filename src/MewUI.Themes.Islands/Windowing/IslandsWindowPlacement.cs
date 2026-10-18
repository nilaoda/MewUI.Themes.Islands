using System.Runtime.InteropServices;
using System.Text;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Windowing;

/// <summary>
/// Where a window that drew its own title bar lands, on the one platform that puts it in the wrong place.
/// </summary>
/// <remarks>
/// <para>
/// macOS answers a request to centre a window by placing it a quarter of the leftover height above the
/// middle. That compensation belongs to a window with a title bar: the bar sits on top of the content, so
/// the eye reads the frame as sitting higher than its content does. A window that drew its own strip has
/// already paid that inside its own content — the strip is the first thing in it — and paying it again
/// leaves the window visibly high, which every window it owns then inherits.
/// </para>
/// <para>
/// Windows and X11 place a centred window in the actual middle, so nothing is taken over there and the
/// platform answers for itself.
/// </para>
/// <para>
/// Taking over is two moments, and the reason it cannot be one is that the platform places the window
/// twice: once when the surface is created, and again once it is on screen. Asking for the exact position
/// from <c>Loaded</c> only survives if the platform's own answer has been switched off first, which has to
/// happen while the window is still unshown. So the constructor claims, <c>Loaded</c> places — and the
/// claim is what makes the placement land in the first painted frame rather than in a jump out of it.
/// </para>
/// </remarks>
internal sealed unsafe partial class IslandsWindowPlacement
{
    private readonly Window _window;

    /// <summary>The region the window was asked to centre on, or null when the platform was left alone.</summary>
    private readonly WindowStartupLocation? _centreOn;

    internal IslandsWindowPlacement(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        _window = window;

        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        _centreOn = window.StartupLocation switch
        {
            WindowStartupLocation.CenterOwner => WindowStartupLocation.CenterOwner,
            WindowStartupLocation.CenterScreen => WindowStartupLocation.CenterScreen,
            _ => null,
        };

        window.StartupLocation = _centreOn is null
            ? window.StartupLocation
            : WindowStartupLocation.Manual;
    }

    /// <summary>
    /// Puts the window in the middle of the region it was asked to centre on.
    /// </summary>
    /// <remarks>
    /// Runs from <c>Loaded</c>, which the framework raises before the window is first painted, and which is
    /// the first moment the window has both a size and a screen to centre against.
    /// </remarks>
    internal void Place()
    {
        switch (_centreOn)
        {
            case null:
                return;

            // The owner's own placement is public arithmetic: the framework's managed centre is exact, and it
            // is the native one that was biased, so the answer is computed rather than corrected.
            case WindowStartupLocation.CenterOwner when _window.Owner is { } owner && owner.Handle != 0:
                MoveToCentre(owner.Position, owner.ClientSize);
                break;

            case WindowStartupLocation.CenterScreen when TryGetWorkArea(out Rect area):
                MoveToCentre(new Point(area.X, area.Y), new Size(area.Width, area.Height));
                break;
        }
    }

    // AppKit can retain the drag origin while the extended-titlebar window is enlarged.
    // Reconcile only a maximized frame, after the title-strip gesture has finished. The backend
    // retains its original restore frame; moving this frame does not change the restore position.
    internal void CorrectMaximizedPosition()
    {
        if (!OperatingSystem.IsMacOS() || _window.WindowState != WindowState.Maximized
            || !TryGetWorkArea(out var area))
        {
            return;
        }

        var position = _window.Position;
        if (Math.Abs(position.X - area.X) > 0.5 || Math.Abs(position.Y - area.Y) > 0.5)
        {
            _window.MoveTo(area.X, area.Y);
        }
    }

    private void MoveToCentre(Point regionTopLeft, Size region)
    {
        Size size = _window.ClientSize;

        _window.MoveTo(
            regionTopLeft.X + ((region.Width - size.Width) / 2),
            regionTopLeft.Y + ((region.Height - size.Height) / 2));
    }

    /// <summary>
    /// The region of the screen this window sits on that is not the menu bar or the dock, in the same
    /// top-down device-independent units <see cref="Window.MoveTo"/> takes.
    /// </summary>
    /// <remarks>
    /// The framework keeps its work area behind an internal platform interface, so it is read from AppKit
    /// here. The flip uses the reference screen — the one the backend flips its own coordinates through —
    /// so the two agree on where the top of the display is.
    /// </remarks>
    private bool TryGetWorkArea(out Rect workArea)
    {
        workArea = default;

        nint reference = ReferenceScreen();
        if (_window.Handle == 0 || reference == 0)
        {
            return false;
        }

        nint nsWindow = Send(_window.Handle, Selector("window"));
        nint screen = nsWindow == 0 ? 0 : Send(nsWindow, Selector("screen"));

        // A window not yet on a screen has nothing to be centred against, and the screen the menu bar is on
        // is the one the platform would have used anyway.
        screen = screen == 0 ? reference : screen;

        CGRect visible = ReadRect(screen, Selector("visibleFrame"));
        CGRect full = ReadRect(reference, Selector("frame"));

        if (visible.size.width <= 0 || visible.size.height <= 0)
        {
            return false;
        }

        workArea = new Rect(
            visible.origin.x,
            (full.origin.y + full.size.height) - (visible.origin.y + visible.size.height),
            visible.size.width,
            visible.size.height);
        return true;
    }

    /// <summary>The screen the coordinate space is measured from: the one carrying the menu bar.</summary>
    private static nint ReferenceScreen()
    {
        nint screens = Send(GetClass("NSScreen"), Selector("screens"));
        return screens == 0 ? 0 : SendWithIndex(screens, Selector("objectAtIndex:"), 0);
    }

    private static nint GetClass(string name)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(name + "\0");

        fixed (byte* pointer = utf8)
        {
            return objc_getClass(pointer);
        }
    }

    private static nint Selector(string name)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(name + "\0");

        fixed (byte* pointer = utf8)
        {
            return sel_registerName(pointer);
        }
    }

    /// <summary>
    /// Reads a struct the shape of <see cref="CGRect"/> off an object.
    /// </summary>
    /// <remarks>
    /// SysV x86_64 returns a struct wider than 16 bytes through a hidden pointer, so on that architecture
    /// the call has to go to <c>objc_msgSend_stret</c> and plain <c>objc_msgSend</c> would yield garbage.
    /// arm64 has no such variant: its ABI returns the struct through the regular entry.
    /// </remarks>
    private static CGRect ReadRect(nint receiver, nint selector) =>
        RuntimeInformation.ProcessArchitecture == Architecture.X64
            ? objc_msgSend_stret_rect(receiver, selector)
            : objc_msgSend_rect(receiver, selector);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct CGPoint
    {
        public readonly double x;
        public readonly double y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct CGSize
    {
        public readonly double width;
        public readonly double height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct CGRect
    {
        public readonly CGPoint origin;
        public readonly CGSize size;
    }

    private const string LibObjC = "/usr/lib/libobjc.A.dylib";

    [LibraryImport(LibObjC)]
    private static partial nint objc_getClass(byte* name);

    [LibraryImport(LibObjC)]
    private static partial nint sel_registerName(byte* name);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial nint Send(nint receiver, nint selector);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial nint SendWithIndex(nint receiver, nint selector, nint index);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial CGRect objc_msgSend_rect(nint receiver, nint selector);

    // Only ever invoked on x86_64; arm64 libobjc does not export this symbol, and a lazy bind keeps the
    // declaration harmless there.
    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend_stret")]
    private static partial CGRect objc_msgSend_stret_rect(nint receiver, nint selector);
}

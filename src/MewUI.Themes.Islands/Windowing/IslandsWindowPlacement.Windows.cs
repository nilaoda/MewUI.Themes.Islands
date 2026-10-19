using System.Runtime.InteropServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Windowing;

internal sealed unsafe partial class IslandsWindowPlacement
{
    private bool _windowsPlacementPending;

    // Called by the shared Win32 hook. Loaded arms this correction; PresentSurface then moves the
    // hidden window before its first paint. Only that first move is corrected, never later user moves.
    internal void CorrectWindowsStartupPosition(nint lParam)
    {
        if (!_windowsPlacementPending || lParam == 0) return;
        const uint SwpNoSize = 0x0001;
        const uint SwpNoMove = 0x0002;
        var position = (WindowsPosition*)lParam;
        if ((position->Flags & SwpNoMove) != 0) return;
        _windowsPlacementPending = false;

        if (_window.WindowState != WindowState.Normal
            || _window.StartupLocation == WindowStartupLocation.Manual
            || _window.ExtendClientAreaTitleBarHeight <= 0
            || !GetWindowRect(_window.Handle, out var bounds)) return;

        var owner = _window.StartupLocation == WindowStartupLocation.CenterOwner
            ? _window.Owner?.Handle ?? 0 : 0;
        var monitor = MonitorFromWindow(owner != 0 ? owner : _window.Handle, 2);
        var info = new WindowsMonitorInfo { Size = (uint)sizeof(WindowsMonitorInfo) };
        if (!GetMonitorInfoW(monitor, ref info)) return;

        var region = info.Work;
        if (owner != 0 && GetWindowRect(owner, out var ownerBounds)) region = ownerBounds;
        int width = (position->Flags & SwpNoSize) != 0 ? bounds.Width : position->Width;
        int height = (position->Flags & SwpNoSize) != 0 ? bounds.Height : position->Height;
        var target = CenterWindowsBounds(region.ToRect(), new Size(width, height), info.Work.ToRect());
        position->X = (int)target.X;
        position->Y = (int)target.Y;
    }

    // All inputs are screen pixels. Neither monitor origins nor owner bounds may be scaled by DPI.
    internal static Point CenterWindowsBounds(Rect region, Size size, Rect workArea) => new(
        Math.Clamp((int)region.X + ((int)region.Width - (int)size.Width) / 2,
            (int)workArea.X, Math.Max((int)workArea.X, (int)workArea.Right - (int)size.Width)),
        Math.Clamp((int)region.Y + ((int)region.Height - (int)size.Height) / 2,
            (int)workArea.Y, Math.Max((int)workArea.Y, (int)workArea.Bottom - (int)size.Height)));

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsPosition
    {
        public nint Window, InsertAfter;
        public int X, Y, Width, Height;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsRect
    {
        public int Left, Top, Right, Bottom;
        public readonly int Width => Right - Left;
        public readonly int Height => Bottom - Top;
        public readonly Rect ToRect() => new(Left, Top, Width, Height);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsMonitorInfo
    {
        public uint Size;
        public WindowsRect Monitor, Work;
        public uint Flags;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(nint window, out WindowsRect rect);

    [LibraryImport("user32.dll")]
    private static partial nint MonitorFromWindow(nint window, uint flags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorInfoW(nint monitor, ref WindowsMonitorInfo info);
}

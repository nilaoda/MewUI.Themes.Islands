using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Windowing;

/// <summary>Spends a title-bar press on dismissing the popup whose capture forwarded it.</summary>
internal static partial class IslandsWindowPopupDismissal
{
    internal static bool TryDismiss(Window window, uint message, nint wParam, nint lParam,
        Func<UIElement?, bool> isDragTarget)
    {
        const uint WmLButtonDown = 0x0201;
        const uint WmLButtonDoubleClick = 0x0203;
        const uint WmNcLButtonDown = 0x00A1;
        const uint WmNcLButtonDoubleClick = 0x00A3;
        const int HtCaption = 2;

        bool clientPress = message is WmLButtonDown or WmLButtonDoubleClick;
        bool captionPress = (message is WmNcLButtonDown or WmNcLButtonDoubleClick) && wParam == HtCaption;
        if ((!clientPress && !captionPress) || !Application.IsRunning) return false;

        var captureHandle = GetCapture();
        if (captureHandle == 0 || captureHandle == window.Handle) return false;
        var capture = Application.Current.AllWindows.FirstOrDefault(candidate => candidate.Handle == captureHandle);
        if (!IsOwnedPopup(window, capture)) return false;

        if (clientPress && !isDragTarget(window.HitTest(ClientPoint(lParam, window.DpiScale)))) return false;

        // Run BEFORE MewUI routes the forwarded press: WindowInputRouter closes popups first,
        // then bubbles that same press to the title strip, which would otherwise start DragMove.
        // Use the captured surface's existing close policy, preserving StaysOpen popups.
        DismissPopup(capture!);
        // A StaysOpen surface can decline dismissal and keep its capture; that press is not spent.
        return GetCapture() != captureHandle;
    }

    internal static bool IsTitleBarDragTarget(UIElement? hit, IReadOnlyCollection<Border> strips,
        IReadOnlyCollection<UIElement> captionRows)
    {
        for (var current = hit; current is not null; current = current.Parent as UIElement)
        {
            if (captionRows.Contains(current)) return false;
            if (current is Border bar && strips.Contains(bar)) return true;
            // Text in the heading is draggable; controls in the actions and caption buttons are not.
            if (current.Focusable || current is not (Panel or Border or TextBlock)) return false;
        }
        return false;
    }

    internal static bool IsOwnedPopup(Window window, Window? capture)
    {
        if (capture is null || !IsNonActivatingSurface(capture)) return false;
        for (var owner = capture.Owner; owner is not null; owner = owner.Owner)
        {
            if (ReferenceEquals(owner, window)) return true;
        }
        return false;
    }

    internal static Point ClientPoint(nint lParam, double dpiScale)
    {
        long packed = lParam.ToInt64();
        return new Point((short)(packed & 0xFFFF) / dpiScale, (short)((packed >> 16) & 0xFFFF) / dpiScale);
    }

    // MewUI 0.22.1 exposes these only to its backends. Static access keeps the workaround
    // compatible with trimming/NativeAOT and avoids a dependency on a platform assembly.
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsNonActivatingSurface")]
    private static extern bool IsNonActivatingSurface(Window window);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "OnPopupSurfaceOutsidePress")]
    internal static extern void DismissPopup(Window window);

    [LibraryImport("user32.dll")]
    private static partial nint GetCapture();
}

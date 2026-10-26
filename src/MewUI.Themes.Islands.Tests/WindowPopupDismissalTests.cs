using System.Reflection;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Windowing;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

public class WindowPopupDismissalTests
{
    [Fact]
    public void Only_the_heading_and_passive_title_background_spend_the_press_on_dismissal()
    {
        var label = new TextBlock().Text("Action");
        var button = new Button().Content(label);
        var editor = new TextBox();
        var heading = new TextBlock().Text("Title");
        var captionFace = new Border().Child(new TextBlock().Text("Close"));
        var captionRow = new StackPanel().Children(captionFace);
        var bar = new Border().Child(new StackPanel().Children(heading, button, editor, captionRow));
        Border[] strips = [bar];
        UIElement[] captions = [captionRow];

        Assert.True(IslandsWindowPopupDismissal.IsTitleBarDragTarget(bar, strips, captions));
        Assert.True(IslandsWindowPopupDismissal.IsTitleBarDragTarget(heading, strips, captions));
        Assert.False(IslandsWindowPopupDismissal.IsTitleBarDragTarget(button, strips, captions));
        Assert.False(IslandsWindowPopupDismissal.IsTitleBarDragTarget(label, strips, captions));
        Assert.False(IslandsWindowPopupDismissal.IsTitleBarDragTarget(editor, strips, captions));
        Assert.False(IslandsWindowPopupDismissal.IsTitleBarDragTarget(captionFace, strips, captions));
        Assert.False(IslandsWindowPopupDismissal.IsTitleBarDragTarget(new TextBlock(), strips, captions));
    }

    [Fact]
    public void Only_owned_popup_capture_is_dismissed_through_the_framework_callback()
    {
        var owner = new Window();
        var other = new Window();
        var dismissals = 0;
        // The real framework popup type exercises both static accessors, including virtual dispatch,
        // without requiring an OS window or changing production metadata access to reflection.
        var type = typeof(Window).Assembly.GetType("Aprillz.MewUI.PopupWindow", throwOnError: true)!;
        var popup = (Window)Activator.CreateInstance(type, new object[] { new Border(), new Size(100, 80), true })!;
        typeof(Window).GetProperty(nameof(Window.Owner))!.SetValue(popup, owner);
        type.GetProperty("DismissRequested", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(popup, (Action)(() => dismissals++));

        Assert.True(IslandsWindowPopupDismissal.IsOwnedPopup(owner, popup));
        Assert.False(IslandsWindowPopupDismissal.IsOwnedPopup(other, popup));
        Assert.False(IslandsWindowPopupDismissal.IsOwnedPopup(owner, owner));
        Assert.False(IslandsWindowPopupDismissal.IsOwnedPopup(owner, null));
        IslandsWindowPopupDismissal.DismissPopup(popup);
        Assert.Equal(1, dismissals);
    }

    [Theory]
    [InlineData(150, 50, 1.25, 120, 40)]
    [InlineData(-30, -20, 1.0, -30, -20)]
    public void Forwarded_client_points_preserve_signed_coordinates_and_apply_dpi_once(
        int x, int y, double scale, double expectedX, double expectedY)
    {
        var packed = (nint)((uint)(ushort)x | ((uint)(ushort)y << 16));
        Assert.Equal(new Point(expectedX, expectedY), IslandsWindowPopupDismissal.ClientPoint(packed, scale));
    }
}

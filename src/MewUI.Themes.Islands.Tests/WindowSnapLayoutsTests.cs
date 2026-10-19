using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Xunit;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands.Tests;

public class WindowSnapLayoutsTests
{
    [Fact]
    public void Rebuilding_the_caption_rebinds_hover_and_press_to_the_new_button()
    {
        var snap = new IslandsWindowNativeHook(new Window());
        int oldCalls = 0;
        bool hot = true;
        bool pressed = false;
        snap.UpdateTarget(new Border(), _ => oldCalls++, _ => oldCalls++);
        snap.UpdateTarget(new Border(), value => hot = value, value => pressed = value);

        snap.Answer(0x00A0, 0, 0, out _); // WM_NCMOUSEMOVE outside HTMAXBUTTON
        snap.Answer(0x00A1, 9, 0, out _); // WM_NCLBUTTONDOWN
        Assert.False(hot);
        Assert.True(pressed);
        Assert.Equal(0, oldCalls);

        snap.Answer(0x02A2, 0, 0, out _); // WM_NCMOUSELEAVE
        Assert.False(hot);
        Assert.False(pressed);
    }

    [Fact]
    public void Hit_testing_a_detached_previously_arranged_button_defers_to_the_platform()
    {
        var button = new Border { Width = 46, Height = 28 };
        button.Measure(new Size(46, 28));
        button.Arrange(new Rect(0, 0, 46, 28));
        Assert.True(button.RenderSize.Width > 0);
        var snap = new IslandsWindowNativeHook(new Window());
        snap.UpdateTarget(button, _ => { }, _ => { });

        Assert.False(snap.Answer(0x0084, 0, 0, out _)); // WM_NCHITTEST
    }
}

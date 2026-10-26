using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Windowing;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

public class TitleBarLayoutTests
{
    [Theory]
    [InlineData(true, WindowChromeCapabilities.None, true)]
    [InlineData(true, WindowChromeCapabilities.NativeWindowBorder, true)]
    [InlineData(true, WindowChromeCapabilities.ExtendClientArea, false)]
    [InlineData(false, WindowChromeCapabilities.None, false)]
    [InlineData(false, WindowChromeCapabilities.NativeChromeButtons, false)]
    public void Only_Linux_without_extended_client_area_keeps_the_native_title_bar(
        bool linux, WindowChromeCapabilities capabilities, bool expected)
    {
        Assert.Equal(expected, IslandsWindowChrome.UseNativeTitleBar(linux, capabilities));
    }

    [Theory]
    [InlineData(900, 240, true)]
    [InlineData(500, 300, true)]
    [InlineData(500, 300, false)]
    public void A_long_heading_never_overlaps_actions(double width, double actionWidth, bool centered)
    {
        var heading = new Probe(1000);
        var actions = new Probe(actionWidth);
        var panel = new IslandsTitleBarLayout(heading, actions, centered);
        panel.Measure(new Size(width, 32));
        panel.Arrange(new Rect(0, 0, width, 32));

        Assert.Equal(width - actionWidth, actions.Bounds.X);
        Assert.True(heading.RenderSize.Width == 0 || heading.Bounds.Right <= actions.Bounds.X - 8);
        if (centered && heading.RenderSize.Width > 0)
        {
            Assert.Equal(width / 2, heading.Bounds.X + heading.Bounds.Width / 2);
        }
    }

    [Fact]
    public void Hiding_captions_releases_space_for_the_heading()
    {
        var heading = new Probe(1000);
        var actions = new Border { Width = 240, Height = 32 };
        var panel = new IslandsTitleBarLayout(heading, actions, centered: true);
        panel.Measure(new Size(900, 32));
        panel.Arrange(new Rect(0, 0, 900, 32));
        double before = heading.RenderSize.Width;

        actions.IsVisible = false;
        panel.Measure(new Size(900, 32));
        panel.Arrange(new Rect(0, 0, 900, 32));
        Assert.True(heading.RenderSize.Width > before);
        Assert.Equal(900, heading.RenderSize.Width);
    }

    private sealed class Probe(double width) : FrameworkElement
    {
        // Like a non-wrapping TextBlock, request the full natural width even in a narrow slot.
        protected override Size MeasureContent(Size availableSize) => new(width, 24);
    }
}

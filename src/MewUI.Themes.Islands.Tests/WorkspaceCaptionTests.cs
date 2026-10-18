using Aprillz.MewUI;
using Xunit;
using MewUI.Themes.Islands.Theming;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands.Tests;

public class WorkspaceCaptionTests
{
    [Fact]
    public void A_left_aligned_title_reserves_the_native_traffic_lights()
    {
        var padding = IslandsWindowChrome.TitlePadding(false, new Thickness(78, 0, 0, 0), true);
        Assert.Equal(78 + IslandsMetrics.TitleBarGap, padding.Left);
        Assert.Equal(IslandsMetrics.TitleBarPadding.Right, padding.Right);
    }

    [Fact]
    public void A_centered_dialog_keeps_its_symmetric_inset()
    {
        var padding = IslandsWindowChrome.TitlePadding(false, new Thickness(78, 0, 0, 0), false);
        Assert.Equal(IslandsMetrics.TitleBarPadding, padding);
    }

    [Fact]
    public void Custom_captions_keep_the_full_height_and_touch_the_window_edge()
    {
        var padding = IslandsWindowChrome.TitlePadding(true, Thickness.Zero, true);
        Assert.Equal(0, padding.Top);
        Assert.Equal(0, padding.Bottom);
        Assert.Equal(0, padding.Right);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 138)]
    public void Centered_titles_keep_equal_side_insets_with_trailing_caption_buttons(bool drawCaptions, double nativeRight)
    {
        var padding = IslandsWindowChrome.TitlePadding(drawCaptions, new Thickness(0, 0, nativeRight, 0), false);
        Assert.Equal(padding.Left, padding.Right);
        if (drawCaptions) Assert.Equal(Thickness.Zero, padding);
        else Assert.Equal(nativeRight + IslandsMetrics.TitleBarGap, padding.Right);
    }
}

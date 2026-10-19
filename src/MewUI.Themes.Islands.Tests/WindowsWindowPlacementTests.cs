using Aprillz.MewUI;
using MewUI.Themes.Islands.Windowing;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

public class WindowsWindowPlacementTests
{
    [Theory]
    [InlineData(1920, 1040, 1180, 720, 370, 160)]
    [InlineData(3240, 1788, 2360, 1440, 440, 174)]
    [InlineData(3840, 2080, 2360, 1440, 740, 320)]
    public void Actual_frame_is_centered_in_the_work_area_at_different_pixel_densities(
        int workWidth, int workHeight, int width, int height, int x, int y)
    {
        var work = new Rect(0, 0, workWidth, workHeight);
        Assert.Equal(new Point(x, y),
            IslandsWindowPlacement.CenterWindowsBounds(work, new Size(width, height), work));
    }

    [Fact]
    public void Secondary_monitor_origins_and_side_taskbars_are_preserved()
    {
        var work = new Rect(-1840, -120, 1840, 1040);
        Assert.Equal(new Point(-1510, 40),
            IslandsWindowPlacement.CenterWindowsBounds(work, new Size(1180, 720), work));
    }

    [Fact]
    public void Owner_center_is_clamped_to_the_owners_monitor_work_area()
    {
        var work = new Rect(1920, 0, 1920, 1040);
        Assert.Equal(new Point(2400, 210),
            IslandsWindowPlacement.CenterWindowsBounds(new Rect(2200, 100, 1000, 720), new Size(600, 500), work));
        Assert.Equal(new Point(1920, 0),
            IslandsWindowPlacement.CenterWindowsBounds(new Rect(1800, -100, 400, 200), new Size(600, 500), work));
    }

    [Fact]
    public void An_oversized_window_keeps_its_top_left_accessible()
    {
        var work = new Rect(-1280, 40, 1280, 680);
        Assert.Equal(new Point(-1280, 40),
            IslandsWindowPlacement.CenterWindowsBounds(work, new Size(1400, 900), work));
    }
}

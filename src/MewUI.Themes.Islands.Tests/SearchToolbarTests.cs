using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Controls;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

public class SearchToolbarTests
{
    [Theory]
    [InlineData(600, true)]
    [InlineData(400, false)]
    [InlineData(200, false)]
    public void Search_and_actions_reflow_without_overlapping(double width, bool inline)
    {
        var field = new Probe(180, 28);
        var firstAction = new Probe(80, 28);
        var lastAction = new Probe(80, 28);
        var actions = new WrapPanel { Spacing = 6 };
        actions.AddRange(firstAction, new Probe(80, 28), lastAction);
        var toolbar = new IslandsSearchToolbar(field, actions);

        toolbar.Measure(new Size(width, double.PositiveInfinity));
        toolbar.Arrange(new Rect(0, 0, width, toolbar.DesiredSize.Height));

        if (inline) Assert.True(actions.Bounds.X >= field.Bounds.Right);
        else Assert.True(actions.Bounds.Y >= field.Bounds.Bottom);
        Assert.True(field.Bounds.Right <= width);
        Assert.True(lastAction.Bounds.Right <= width);
        Assert.True(lastAction.Bounds.Bottom <= toolbar.Bounds.Bottom);
        if (width == 200) Assert.True(lastAction.Bounds.Y > firstAction.Bounds.Y);
    }

    private sealed class Probe(double width, double height) : FrameworkElement
    {
        protected override Size MeasureContent(Size availableSize) => new(width, height);
    }
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Windowing;

/// <summary>Keeps the heading clear of trailing actions while retaining window-centered alignment.</summary>
internal sealed class IslandsTitleBarLayout : Panel
{
    private const double Gap = 8;
    private readonly UIElement _heading;
    private readonly UIElement _actions;
    private readonly bool _centered;

    internal IslandsTitleBarLayout(UIElement heading, UIElement actions, bool centered)
    {
        _heading = heading;
        _actions = actions;
        _centered = centered;
        ClipToBounds = true;
        Add(heading);
        Add(actions);
    }

    protected override Size MeasureContent(Size availableSize)
    {
        var available = availableSize.Deflate(Padding);
        _actions.Measure(available);
        double reserved = Reservation(_actions.DesiredSize.Width);
        _heading.Measure(new Size(Math.Max(0, available.Width - reserved), available.Height));
        return new Size(
            Math.Min(available.Width, _heading.DesiredSize.Width + reserved),
            Math.Max(_heading.DesiredSize.Height, _actions.DesiredSize.Height)).Inflate(Padding);
    }

    protected override void ArrangeContent(Rect bounds)
    {
        var content = bounds.Deflate(Padding);
        double actionWidth = Math.Min(content.Width, _actions.DesiredSize.Width);
        _actions.Arrange(new Rect(content.Right - actionWidth, content.Y, actionWidth, content.Height));

        double headingWidth = Math.Max(0, content.Width - Reservation(actionWidth));
        double headingX = _centered ? content.X + (content.Width - headingWidth) / 2 : content.X;
        _heading.Arrange(new Rect(headingX, content.Y, headingWidth, content.Height));
    }

    private double Reservation(double actionWidth) =>
        actionWidth <= 0 ? 0 : (actionWidth + Gap) * (_centered ? 2 : 1);
}

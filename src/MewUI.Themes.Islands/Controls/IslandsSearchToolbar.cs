using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Controls;

/// <summary>Places a flexible search field beside actions, or above them when space is limited.</summary>
public sealed class IslandsSearchToolbar : Panel
{
    private const double Gap = 6;
    private readonly UIElement _field;
    private readonly UIElement _actions;
    private readonly double _minimumFieldWidth;
    private bool _inline;

    /// <summary>The actions may be a WrapPanel so that longer localized labels can wrap independently.</summary>
    public IslandsSearchToolbar(UIElement field, UIElement actions, double minimumFieldWidth = 240)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(actions);
        if (!double.IsFinite(minimumFieldWidth) || minimumFieldWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumFieldWidth));
        _field = field;
        _actions = actions;
        _minimumFieldWidth = minimumFieldWidth;
        Add(field);
        Add(actions);
    }

    protected override Size MeasureContent(Size availableSize)
    {
        var size = availableSize.Deflate(Padding);
        _actions.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _inline = size.Width >= _minimumFieldWidth + Gap + _actions.DesiredSize.Width;
        if (!_inline) _actions.Measure(new Size(size.Width, double.PositiveInfinity));
        var fieldWidth = _inline ? size.Width - Gap - _actions.DesiredSize.Width : size.Width;
        _field.Measure(new Size(Math.Max(0, fieldWidth), double.PositiveInfinity));
        var width = double.IsFinite(size.Width) ? size.Width : _field.DesiredSize.Width + Gap + _actions.DesiredSize.Width;
        var height = _inline ? Math.Max(_field.DesiredSize.Height, _actions.DesiredSize.Height)
            : _field.DesiredSize.Height + Gap + _actions.DesiredSize.Height;
        return new Size(width, height).Inflate(Padding);
    }

    protected override void ArrangeContent(Rect bounds)
    {
        var rect = bounds.Deflate(Padding);
        var actionHeight = _actions.DesiredSize.Height;
        var fieldHeight = _field.DesiredSize.Height;
        if (_inline)
        {
            var actionWidth = _actions.DesiredSize.Width;
            var fieldWidth = Math.Max(0, rect.Width - Gap - actionWidth);
            _field.Arrange(new Rect(rect.X, rect.Y + Math.Max(0, (rect.Height - fieldHeight) / 2), fieldWidth, fieldHeight));
            _actions.Arrange(new Rect(rect.Right - actionWidth, rect.Y + Math.Max(0, (rect.Height - actionHeight) / 2), actionWidth, actionHeight));
        }
        else
        {
            _field.Arrange(new Rect(rect.X, rect.Y, rect.Width, fieldHeight));
            _actions.Arrange(new Rect(rect.X, rect.Y + fieldHeight + Gap, rect.Width, actionHeight));
        }
    }
}

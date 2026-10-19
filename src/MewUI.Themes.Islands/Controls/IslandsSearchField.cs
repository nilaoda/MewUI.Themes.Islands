using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Controls;

/// <summary>A native text box with a search icon and an optional scope control inside its outline.</summary>
/// <remarks>The caller owns text bindings, search behavior, localization and the scope menu.</remarks>
public sealed class IslandsSearchField : Panel
{
    private readonly TextBox _input;
    private readonly UIElement? _scope;
    private readonly FrameworkElement _icon;

    public IslandsSearchField(TextBox input, UIElement? scope = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        _input = input;
        _scope = scope;
        _icon = IslandsIcon.Create(IslandsIconKind.Search);
        _icon.IsHitTestVisible = false;
        Add(input);
        Add(_icon);
        if (scope is not null) Add(scope);
    }

    protected override Size MeasureContent(Size availableSize)
    {
        var size = availableSize.Deflate(Padding);
        _scope?.Measure(new Size(Math.Max(0, size.Width - 48), double.PositiveInfinity));
        var scopeWidth = _scope?.DesiredSize.Width ?? 0;
        // Reserve text space using the actual localized scope width, including native text selection.
        _input.Padding = new Thickness(30, 5, scopeWidth + 10, 5);
        _input.Measure(size);
        _icon.Measure(new Size(16, 16));
        return new Size(_input.DesiredSize.Width, Math.Max(_input.DesiredSize.Height,
            (_scope?.DesiredSize.Height ?? 0) + 4)).Inflate(Padding);
    }

    protected override void ArrangeContent(Rect bounds)
    {
        var rect = bounds.Deflate(Padding);
        _input.Arrange(rect);
        _icon.Arrange(new Rect(rect.X + 8, rect.Y + Math.Max(0, (rect.Height - 16) / 2), 16, 16));
        if (_scope is not null)
        {
            var width = _scope.DesiredSize.Width;
            var height = _scope.DesiredSize.Height;
            _scope.Arrange(new Rect(rect.Right - width - 4, rect.Y + Math.Max(0, (rect.Height - height) / 2), width, height));
        }
    }
}

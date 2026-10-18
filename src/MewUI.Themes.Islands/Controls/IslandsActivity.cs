using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Controls;

/// <summary>Reusable activity-list and inspector layouts. All text belongs to the host.</summary>
public static class IslandsActivity
{
    /// <summary>A compact activity row with an emphasized summary and a secondary timestamp.</summary>
    public static UIElement Row(string summary, string time, IslandsIconKind icon) =>
        new Grid().Columns(GridLength.Auto, GridLength.Star, GridLength.Auto).Spacing(8).Margin(4, 7)
            .Children(IslandsIcon.Create(icon).CenterVertical().GridPosition(0, 0),
                new TextBlock().Text(summary).TextTrimming(TextTrimming.CharacterEllipsis)
                    .ToolTip(summary).CenterVertical().GridPosition(0, 1),
                new TextBlock().Text(time).FontSize(12).IslandsHint().CenterVertical().GridPosition(0, 2));

    /// <summary>A property label beside host-owned value content; values may wrap or contain comparisons.</summary>
    public static UIElement Property(string label, UIElement value) =>
        new Grid().Columns(new GridLength(88, GridUnitType.Pixel), GridLength.Star).Spacing(12).Margin(0, 4)
            .Children(new TextBlock().Text(label).FontSize(12).IslandsHint()
                    .TextWrapping(TextWrapping.Wrap).GridPosition(0, 0), value.GridPosition(0, 1));
}

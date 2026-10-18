using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Controls;

/// <summary>Compact, reusable building blocks for tool-window headers and action strips.</summary>
public static class IslandsToolbar
{
    /// <summary>A header with flexible context on the left and content-sized actions on the right.</summary>
    public static Border Header(UIElement context, UIElement? actions = null)
    {
        var grid = new Grid().Columns(GridLength.Star, GridLength.Auto).Spacing(8)
            .Children(context.GridPosition(0, 0));
        if (actions is not null)
        {
            grid.Children(actions.GridPosition(0, 1));
        }

        return new Border().IslandsContent().Padding(8, 6)
            .Child(grid);
    }

    /// <summary>An icon-only toolbar action; the host supplies its localized accessible description.</summary>
    public static Button IconButton(IslandsIconKind icon, string description) =>
        new Button().Content(IslandsIcon.Create(icon)).IslandsToolbarButton()
            .Width(28).Height(28).MinWidth(0).MinHeight(28).Padding(5)
            .ToolTip(description);

    /// <summary>A compact action with a monochrome icon and a visible label.</summary>
    public static Button ActionButton(IslandsIconKind icon, string text) =>
        new Button().Content(new StackPanel().Horizontal().Spacing(6).Children(
            IslandsIcon.Create(icon).CenterVertical(), new TextBlock().Text(text).CenterVertical()))
            .IslandsToolbarButton().MinWidth(0).Padding(7, 4);

    /// <summary>A primary action whose icon shares the text's contrast over the accent.</summary>
    public static Button PrimaryActionButton(IslandsIconKind icon, string text) =>
        new Button().Content(new StackPanel().Horizontal().Spacing(6).Children(
            IslandsIcon.Create(icon, overAccent: true).CenterVertical(), new TextBlock().Text(text).CenterVertical()))
            .IslandsPrimaryButton().MinWidth(0).Padding(7, 4);

    /// <summary>A keyboard-operable disclosure action for a host-owned group.</summary>
    public static Button DisclosureButton(string text, bool expanded) =>
        ActionButton(expanded ? IslandsIconKind.ChevronUp : IslandsIconKind.ChevronDown, text)
            .ToolTip(text);

    /// <summary>A narrow tool-window rail with persistent tools at the bottom.</summary>
    public static UIElement Rail(UIElement top, UIElement bottom) =>
        new Grid().Rows(GridLength.Auto, GridLength.Star, GridLength.Auto)
            .Width(34).Margin(0, 6, 2, 6)
            .Children(top.GridPosition(0, 0), bottom.GridPosition(2, 0));
}

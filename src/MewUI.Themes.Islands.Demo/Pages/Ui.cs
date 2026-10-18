using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;

namespace MewUI.Themes.Islands.Demo.Pages;

/// <summary>Small layout helpers shared by the gallery pages.</summary>
internal static class Ui
{
    public static StackPanel Page(string title, string subtitle, params UIElement[] children)
    {
        var stack = new StackPanel().Vertical().Spacing(16);
        stack.Children(
            new TextBlock().Text(title).IslandsTitle(),
            new TextBlock().Text(subtitle).IslandsHint());
        stack.Children(children);
        return stack;
    }

    public static Border Section(string title, params UIElement[] children) =>
        new Border().IslandsSection().Padding(16).Child(
            new StackPanel().Vertical().Spacing(12).Children(
                new TextBlock().Text(title).IslandsSectionTitle(),
                new StackPanel().Vertical().Spacing(10).Children(children)));

    public static UIElement Row(string label, UIElement control) =>
        new Grid().Columns(new GridLength(190, GridUnitType.Pixel), GridLength.Star)
            .Spacing(12)
            .Children(
                new TextBlock().Text(label).IslandsFieldLabel().CenterVertical().GridPosition(0, 0),
                control.GridPosition(0, 1));

    public static Border Swatch(Color color, string name) =>
        new Border().IslandsCard().Padding(12).Child(
            new StackPanel().Horizontal().Spacing(10).Children(
                new Border().Width(28).Height(28).CornerRadius(6).Background(color),
                new StackPanel().Vertical().Spacing(2).Children(
                    new TextBlock().Text(name),
                    new TextBlock().Text(Hex(color)).IslandsMonospace().IslandsHint())));

    public static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}

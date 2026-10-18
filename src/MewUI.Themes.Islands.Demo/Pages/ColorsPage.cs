using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Demo.Pages;

internal static class ColorsPage
{
    public static UIElement Build(DemoWindow host)
    {
        var colors = IslandsTheme.CurrentColors;

        var swatches = new UniformGrid().Columns(3).Spacing(12);
        swatches.Children(
            Ui.Swatch(colors.Accent, "Accent"),
            Ui.Swatch(colors.PanelBackground, "PanelBackground"),
            Ui.Swatch(colors.ComponentBackground, "ComponentBackground"),
            Ui.Swatch(colors.Selection, "Selection"),
            Ui.Swatch(colors.HoverBackground, "HoverBackground"),
            Ui.Swatch(colors.Text, "Text"),
            Ui.Swatch(colors.SecondaryText, "SecondaryText"),
            Ui.Swatch(colors.Border, "Border"),
            Ui.Swatch(colors.Error, "Error"),
            Ui.Swatch(colors.Warning, "Warning"),
            Ui.Swatch(colors.Success, "Success"),
            Ui.Swatch(colors.Link, "Link"));

        var density = IslandsMetrics.Density == IslandsDensity.Compact ? "Compact" : "Regular";
        var metrics = new StackPanel().Vertical().Spacing(6).Children(
            new TextBlock().Text($"Density: {density}").IslandsFieldLabel(),
            new TextBlock().Text($"Base control height: {IslandsMetrics.Default.BaseControlHeight}").IslandsMonospace(),
            new TextBlock().Text($"List row height: {IslandsMetrics.ListRowHeight}").IslandsMonospace(),
            new TextBlock().Text($"Island arc: {IslandsMetrics.IslandArc}").IslandsMonospace(),
            new TextBlock().Text($"Selection arc: {IslandsMetrics.SelectionArc}").IslandsMonospace(),
            new TextBlock().Text($"Accent: {Ui.Hex(colors.Accent)}").IslandsMonospace());

        return Ui.Page(
            "Colors & Metrics",
            "The active variant's color table and the metrics the theme installed.",
            Ui.Section("Colors", swatches),
            Ui.Section("Metrics", metrics));
    }
}

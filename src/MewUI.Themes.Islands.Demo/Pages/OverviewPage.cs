using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;

namespace MewUI.Themes.Islands.Demo.Pages;

internal static class OverviewPage
{
    public static UIElement Build(DemoWindow host)
    {
        var hero = new Border().IslandsSection().Padding(24).Child(
            new Grid().Columns(GridLength.Star, GridLength.Auto).Spacing(24).Children(
                new StackPanel().Vertical().Spacing(10).Children(
                    new TextBlock().Text("Islands for MewUI").IslandsTitle(),
                    new TextBlock().Text("JetBrains' New UI look as a MewUI theme: surfaces, typography, controls and window chrome.").IslandsHint(),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        Action("Primary action", primary: true, host),
                        Action("Secondary action", primary: false, host),
                        Flat("Flat action", host))).GridPosition(0, 0),
                new StackPanel().Vertical().Spacing(8).Children(
                    IslandsActivity.Row("Theme installed", "now", IslandsIconKind.Settings),
                    IslandsActivity.Row("Workspace ready", "1 min", IslandsIconKind.Library),
                    IslandsActivity.Row("Controls loaded", "2 min", IslandsIconKind.History)).GridPosition(0, 1)));

        var rows = new IslandsRowList();
        rows.AddHeader(new Grid()
            .Columns(GridLength.Star, new GridLength(100, GridUnitType.Pixel))
            .Spacing(8)
            .Margin(10, 6)
            .Children(
                new TextBlock().Text("Recent activity").IslandsSectionTitle().GridPosition(0, 0),
                new TextBlock().Text("Status").IslandsHint().GridPosition(0, 1)));

        AddRow(rows, "a", "Imported a batch", "Done");
        AddRow(rows, "b", "Reviewed conflicts", "Done");
        AddRow(rows, "c", "Exported a series", "Done");

        var product = IslandsProductInfo.Entry(
            "Islands Theme",
            "0.1.0",
            "MewUI theme gallery",
            () => host.SetStatus("MewUI.Themes.Islands · MIT"));

        return Ui.Page(
            "Overview",
            "The signature Islands surfaces, actions and activity rows.",
            hero,
            Ui.Section("Row list", new Border().Height(180).Child(rows.Element)),
            Ui.Section("Product info", product));
    }

    private static void AddRow(IslandsRowList rows, object key, string activity, string status) =>
        rows.AddRow(
            key,
            [GridLength.Star, new GridLength(100, GridUnitType.Pixel)],
            new TextBlock().Text(activity),
            new TextBlock().Text(status).IslandsHint());

    private static Button Action(string text, bool primary, DemoWindow host)
    {
        var button = new Button().Content(text);
        if (primary)
        {
            button.IslandsPrimaryButton();
        }

        button.OnClick(() => host.SetStatus(text));
        return button;
    }

    private static Button Flat(string text, DemoWindow host)
    {
        var button = new Button().Content(text).IslandsButton();
        button.OnClick(() => host.SetStatus(text));
        return button;
    }
}

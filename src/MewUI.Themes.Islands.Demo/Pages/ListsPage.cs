using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;
using MewUI.Themes.Islands.Controls;

namespace MewUI.Themes.Islands.Demo.Pages;

internal static class ListsPage
{
    private sealed record Node(string Text, IReadOnlyList<Node> Children);

    public static UIElement Build(DemoWindow host)
    {
        var rows = new IslandsRowList();
        rows.AddHeader(new Grid()
            .Columns(GridLength.Star, new GridLength(120, GridUnitType.Pixel), new GridLength(90, GridUnitType.Pixel))
            .Spacing(8)
            .Margin(10, 6)
            .Children(
                new TextBlock().Text("Name").IslandsSectionTitle().GridPosition(0, 0),
                new TextBlock().Text("Kind").IslandsHint().GridPosition(0, 1),
                new TextBlock().Text("State").IslandsHint().GridPosition(0, 2)));

        AddRow(rows, "1", "Project", "Folder", "Open");
        AddRow(rows, "2", "README.md", "File", "Edited");
        AddRow(rows, "3", "Program.cs", "File", "Clean");

        var listBox = new ListBox()
            .Items("Alpha", "Beta", "Gamma", "Delta")
            .SelectedIndex(0)
            .Height(180);

        var roots = new[]
        {
            new Node("Project", [
                new Node("src", [new Node("Program.cs", []), new Node("Theme.cs", [])]),
                new Node("docs", [new Node("color-map.md", [])]),
            ]),
            new Node("Samples", [new Node("Gallery", [])]),
        };

        var tree = new TreeView().Height(220);
        tree.Items(roots, node => node.Children, node => node.Text, node => node.Text, node => node.Children.Count > 0);

        return Ui.Page(
            "Lists & Trees",
            "Row lists, list boxes and trees, with the theme's selection and hover states.",
            Ui.Section("Row list", new Border().Height(190).Child(rows.Element)),
            Ui.Section("List box", listBox),
            Ui.Section("Tree", tree));
    }

    private static void AddRow(IslandsRowList rows, object key, string name, string kind, string state) =>
        rows.AddRow(
            key,
            [GridLength.Star, new GridLength(120, GridUnitType.Pixel), new GridLength(90, GridUnitType.Pixel)],
            new TextBlock().Text(name),
            new TextBlock().Text(kind).IslandsHint(),
            new TextBlock().Text(state).IslandsHint());
}

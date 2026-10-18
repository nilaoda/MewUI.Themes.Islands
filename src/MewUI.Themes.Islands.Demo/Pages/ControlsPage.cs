using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;
using MewUI.Themes.Islands.Controls;
using MewUI.Themes.Islands.Drawing;

namespace MewUI.Themes.Islands.Demo.Pages;

internal static class ControlsPage
{
    public static UIElement Build(DemoWindow host)
    {
        var buttons = new StackPanel().Horizontal().Spacing(8).Children(
            Button("Primary", host).IslandsPrimaryButton(),
            Button("Default", host),
            Button("Flat", host).IslandsButton(),
            Button("Compact", host).IslandsCompactButton());

        var toolbar = new StackPanel().Horizontal().Spacing(4).Children(
            IslandsToolbar.ActionButton(IslandsIconKind.Plus, "New"),
            IslandsToolbar.ActionButton(IslandsIconKind.Pencil, "Edit"),
            IslandsToolbar.IconButton(IslandsIconKind.More, "More"),
            IslandsToolbar.PrimaryActionButton(IslandsIconKind.Export, "Export"));

        var textBox = new TextBox().Placeholder("Type a name...").Text("Islands");
        var combo = new ComboBox()
            .Items("Regular", "Compact", "Comfortable")
            .SelectedIndex(0)
            .IslandsDropDownArrow();

        var check = new CheckBox().Content("Enable animations").IsChecked(true);
        var radioA = new RadioButton().Content("Light").GroupName("theme").IsChecked(true);
        var radioB = new RadioButton().Content("Dark").GroupName("theme");
        var toggle = new ToggleSwitch().IsChecked(true).HorizontalAlignment(HorizontalAlignment.Left);

        var slider = new Slider().Minimum(0).Maximum(100).Value(64).Width(260);
        var progress = new ProgressBar().Minimum(0).Maximum(100).Value(68).Width(260);

        var tabs = new TabControl().Width(520).Height(170);
        tabs.AddTab(new TabItem
        {
            Header = new TextBlock().Text("General"),
            Content = new StackPanel().Vertical().Spacing(8).Margin(12).Children(
                new TextBlock().Text("Tabs use the Islands tab plate and underline.").IslandsHint(),
                new CheckBox().Content("Show status bar").IsChecked(true)),
        });
        tabs.AddTab(new TabItem
        {
            Header = new TextBlock().Text("Advanced"),
            Content = new StackPanel().Vertical().Spacing(8).Margin(12).Children(
                new TextBlock().Text("Second tab content.").IslandsHint(),
                new CheckBox().Content("Enable experimental features")),
        });

        var expander = new Expander
        {
            Header = new TextBlock().Text("Advanced options"),
            IsExpanded = true,
            Content = new TextBlock().Text("Expander content uses the theme's surfaces and spacing.").IslandsHint(),
        };

        var group = new GroupBox
        {
            Header = new TextBlock().Text("Group box"),
            Content = new TextBlock().Text("A grouped set of controls.").IslandsHint(),
        };

        return Ui.Page(
            "Controls",
            "Buttons, fields, toggles, ranges, tabs and containers.",
            Ui.Section("Buttons", buttons, new Separator(), toolbar),
            Ui.Section("Fields", Ui.Row("Text box", textBox), Ui.Row("Combo box", combo)),
            Ui.Section(
                "Toggles",
                Ui.Row("Check box", check),
                Ui.Row("Radio buttons", new StackPanel().Horizontal().Spacing(12).Children(radioA, radioB)),
                Ui.Row("Toggle switch", toggle)),
            Ui.Section("Ranges", Ui.Row("Slider", slider), Ui.Row("Progress", progress)),
            Ui.Section("Tabs", tabs),
            Ui.Section("Containers", expander, group));
    }

    private static Button Button(string text, DemoWindow host)
    {
        var button = new Button().Content(text);
        button.OnClick(() => host.SetStatus(text));
        return button;
    }
}

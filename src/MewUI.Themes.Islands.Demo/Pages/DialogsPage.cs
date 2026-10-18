using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;
using MewUI.Themes.Islands.Controls;
using MewUI.Themes.Islands.Menus;

namespace MewUI.Themes.Islands.Demo.Pages;

internal static class DialogsPage
{
    public static UIElement Build(DemoWindow host)
    {
        var dialog = IslandsToolbar.PrimaryActionButton(IslandsIconKind.Settings, "Open dialog");
        dialog.OnClick(() => _ = host.ShowSampleDialogAsync());

        var frameless = IslandsToolbar.ActionButton(IslandsIconKind.Library, "Open frameless window");
        frameless.OnClick(() => _ = host.OpenFramelessWindowAsync());

        var menu = IslandsToolbar.ActionButton(IslandsIconKind.More, "Show context menu");
        menu.OnClick(() => host.ShowContextMenu(menu));

        var footer = new Border().IslandsDialogFooter().Child(
            new StackPanel().Horizontal().Spacing(8).Children(
                new Button().Content("Cancel"),
                new Button().Content("Save").IslandsPrimaryButton()));

        return Ui.Page(
            "Dialogs & Menus",
            "Modal dialogs, context menus and the dialog footer.",
            Ui.Section("Actions", new StackPanel().Horizontal().Spacing(8).Children(dialog, frameless, menu)),
            Ui.Section("Dialog footer", footer),
            Ui.Section(
                "Menu rows",
                new TextBlock().Text("IslandsContextMenu draws a flat panel in the window's overlay layer.").IslandsHint(),
                new TextBlock().Text("Click Show context menu to see it.").IslandsHint()));
    }
}

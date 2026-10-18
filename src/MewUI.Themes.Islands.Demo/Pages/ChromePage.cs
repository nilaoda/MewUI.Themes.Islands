using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;

namespace MewUI.Themes.Islands.Demo.Pages;

internal static class ChromePage
{
    public static UIElement Build(DemoWindow host)
    {
        var open = IslandsToolbar.PrimaryActionButton(IslandsIconKind.Library, "Open frameless window");
        open.OnClick(() => _ = host.OpenFramelessWindowAsync());

        var header = IslandsToolbar.Header(
            new TextBlock().Text("Tool window header").IslandsSectionTitle(),
            new StackPanel().Horizontal().Spacing(4).Children(
                IslandsToolbar.IconButton(IslandsIconKind.Plus, "Add"),
                IslandsToolbar.IconButton(IslandsIconKind.More, "More")));

        return Ui.Page(
            "Window Chrome",
            "The frameless window keeps the platform's caption buttons and supplies its own title strip.",
            Ui.Section("Try it", open),
            Ui.Section("Toolbar header", header),
            Ui.Section(
                "What the chrome owns",
                new TextBlock().Text("Title strip, drag region and caption buttons").IslandsHint(),
                new TextBlock().Text("Windows 11 snap layouts").IslandsHint(),
                new TextBlock().Text("macOS traffic-light inset and placement").IslandsHint(),
                new TextBlock().Text("Window hover and backdrop").IslandsHint()));
    }
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands;

/// <summary>A quiet, keyboard-accessible product identity entry for the foot of a navigation pane.</summary>
public static class IslandsProductInfo
{
    /// <summary>The host supplies the product identity, accessible description and about action.</summary>
    public static Button Entry(string name, string version, string description, Action openAbout) =>
        new Button()
            .IslandsToolbarButton()
            .MinWidth(0).Height(58).MinHeight(58).Padding(10, 8)
            .Content(new StackPanel().Vertical().Spacing(3).HorizontalAlignment(HorizontalAlignment.Left)
                .Children(
                    new TextBlock().Text(name).FontSize(12).FontWeight(FontWeight.SemiBold).IslandsHint(),
                    new TextBlock().Text(version).FontSize(12).IslandsHint()))
            .ToolTip(description)
            .OnClick(openAbout);
}

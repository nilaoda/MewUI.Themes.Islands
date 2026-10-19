using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// The menu bar and the pop-up menus it and every other control opens.
/// </summary>
internal static class IslandsMenuStyles
{
    internal static void Register(StyleSheet sheet)
    {
        sheet.Define<MenuBar>(MenuBarStyle());
        sheet.Define<ContextMenu>(ContextMenuStyle());
    }

    /// <summary>
    /// Islands lays the menu bar on the panel rather than on a button face, and closes it with the
    /// separator line that Islands keeps even when the frame is flat.
    /// </summary>
    private static Style MenuBarStyle() =>
        Style.DeriveFromDefault<MenuBar>(
            [
                IslandsStyles.Background(c => c.ToolBarBackground),
                IslandsStyles.BorderBrush(c => c.Separator),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, 0.0),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarPadding),
                // The entries carry their own hover plate, so the bar leaves no gap between them.
                IslandsStyles.Fixed(MenuBar.SpacingProperty, 0.0),
                IslandsStyles.Fixed(MenuBar.DrawBottomSeparatorProperty, true),
            ],
            [
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    /// <summary>
    /// A menu is its own surface: Islands's <c>PopupMenu.background</c> is whiter than the panel and its
    /// edge is darker than a component's, because the menu floats over both.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The rows themselves are an internal element type colored from <c>Palette.SelectionBackground</c>,
    /// which MewUI derives as an accent wash rather than Islands's flat selection color; the plate is
    /// therefore paler than Islands'. No style key reaches it. See <c>docs/color-map.md</c>.
    /// </para>
    /// <para>
    /// A menu row is Islands's own <c>List|Tree/rowHeight</c> (24) rather than the height MewUI derives from
    /// the control height (26), because Islands states the two as separate keys. Its plate stays as round as
    /// the menu's own corner minus the border — a menu is read by its silhouette, so unlike a drop-down's list
    /// it keeps the radius the chrome blurs its shadow with.
    /// </para>
    /// </remarks>
    private static Style ContextMenuStyle() =>
        Style.DeriveFromDefault<ContextMenu>(
            [
                IslandsStyles.Background(c => c.MenuBackground),
                IslandsStyles.BorderBrush(c => c.MenuBorder),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.PopupCornerRadius(),
                IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
                IslandsStyles.Metric(TextElement.FontFamilyProperty, t => t.Metrics.FontFamily),
                IslandsStyles.Metric(TextElement.FontSizeProperty, t => t.Metrics.FontSize + 1),
                IslandsStyles.Fixed(ContextMenu.ItemHeightProperty, IslandsMetrics.NavigationRowHeight),
                IslandsStyles.Fixed(ContextMenu.ItemPaddingProperty, IslandsMetrics.MenuItemPadding),
                // The rows float inside the popup: a plate keeps a small inset from every edge.
                IslandsStyles.Fixed(Control.PaddingProperty, new Thickness(4)),
                // MenuItem.minimumWidth: Islands floors every row at 72, which is what keeps a menu of
                // short labels from collapsing to a column narrower than the button that opened it.
                IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, IslandsMetrics.MenuMinWidth),
            ],
            [
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);
}

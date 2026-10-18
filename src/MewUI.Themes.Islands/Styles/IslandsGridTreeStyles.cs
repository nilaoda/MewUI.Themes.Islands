using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// The grid and the tree: their plates, their rows and the metrics that decide row height and cell insets.
/// </summary>
internal static class IslandsGridTreeStyles
{
    internal static void Register(StyleSheet sheet)
    {
        sheet.Define<GridView>(GridStyle());
        sheet.Define<GridViewRow>(GridRowStyle());
        sheet.Define<TreeView>(TreeStyle());
    }

    /// <summary>
    /// A grid is an item view plus Islands's table metrics. The type rule for the control takes precedence
    /// over the one on <see cref="ScrollableItemsBase"/>, so the plate and its edge are restated here.
    /// </summary>
    /// <remarks>
    /// <c>ZebraStriping</c> goes off: Islands's table has no alternating rows. <c>ShowGridLines</c> stays as
    /// the control left it, because the line color itself is read from the palette and cannot be moved to
    /// <c>Table.gridColor</c> by a style.
    /// </remarks>
    private static Style GridStyle() =>
        Style.DeriveFromDefault<GridView>(
            [
                .. IslandsListStyles.ItemViewSetters(),
                IslandsStyles.Fixed(GridView.ZebraStripingProperty, false),
                // TableHeader.height. The framework would derive 26 from the control height, one over the
                // token, and there is room for the token: a cell's own text fits in a 24 DIP row.
                IslandsStyles.Fixed(GridView.HeaderHeightProperty, IslandsMetrics.GridHeaderHeight),
                // Islands states no Table.rowHeight, and MewUI's own grid derivation is
                // Max(BaseControlHeight, 24) = 28 — four more than a list row. A table keeps the row rhythm
                // Islands states for everything else (List|Tree/rowHeight) rather than the old New UI's 22,
                // which is what the SDK's table reference images measure; see docs/color-map.md.
                IslandsStyles.Fixed(GridView.RowHeightProperty, IslandsMetrics.ListRowHeight),
            ],
            IslandsListStyles.ItemViewTriggers());

    /// <summary>
    /// A grid row is the same transparent plate as a list row: the fill belongs to the hover and to the
    /// selection, and the header's stripe underneath is what shows at rest.
    /// </summary>
    /// <remarks>
    /// <c>Table.hoverBackground</c> is the same <c>selection-bg-hovered</c> overlay a list row takes, and it
    /// has to be restated for the same reason: the transparent idle fill that keeps the table unstriped
    /// outranks the framework's own hover trigger. A table has no selection arc, so the plate stays square.
    /// </remarks>
    private static Style GridRowStyle() =>
        Style.DeriveFromDefault<GridViewRow>(
            [
                IslandsStyles.Background(c => c.ComponentBackground.WithAlpha(0)),
                // The framework insets the row's arc from the metric; Islands's Table.selectionArc is 0.
                IslandsStyles.Fixed(Control.CornerRadiusProperty, 0.0),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.HoverBackground)),
                IslandsStyles.When(VisualStateFlags.Selected,
                    IslandsStyles.SelectionPlate(),
                    IslandsStyles.SelectionTextLabel()),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    /// <summary>
    /// A tree draws its own rows through <see cref="ItemContainer"/>, so this rule carries the plate and the
    /// indent; Islands gives a tree the same 18 DIP per level that its <c>leftChildIndent</c> plus
    /// <c>rightChildIndent</c> add up to.
    /// </summary>
    private static Style TreeStyle() =>
        Style.DeriveFromDefault<TreeView>(
            [
                .. IslandsListStyles.ItemViewSetters(),
                // Tree.border = 4,12,4,12: a tree insets its rows twelve where a list insets eight.
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.TreeContentPadding),
                IslandsStyles.Fixed(TreeView.ItemPaddingProperty, IslandsMetrics.RowPadding),
                IslandsStyles.Fixed(TreeView.ItemHeightProperty, IslandsMetrics.NavigationRowHeight),
                IslandsStyles.Fixed(TreeView.IndentProperty, 18.0),
                // MewUI derives every row's corner radius from the control's own CornerRadius, and
                // ItemViewSetters squares the plate for lists — which squared the tree's selection too.
                // The tree's plate is transparent over the panel, so rounding it only shows on the rows.
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SelectionArc),
            ],
            IslandsListStyles.ItemViewTriggers());
}

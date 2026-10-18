using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// Scrollable item views: list boxes, items controls, trees, and the rows inside them.
/// </summary>
internal static class IslandsListStyles
{
    /// <summary>The framework key <c>ItemsControl</c> gives the rows it builds for itself.</summary>
    private const string ItemsControlRowStyleName = "items-control-row";

    internal static void Register(StyleSheet sheet)
    {
        // One rule on the base type covers the tree, the navigation list and the plain items control: the
        // sheet walks a control's base types and stops at the nearest rule.
        sheet.Define<ScrollableItemsBase>(
            Style.DeriveFromDefault<ScrollableItemsBase>(ItemViewSetters(), ItemViewTriggers()));

        // A list does add two things of its own — a zebra switch and a row height — and only a rule on the
        // list may state either, because a setter is refused when its property is registered on a type below
        // the rule's target.
        sheet.Define<ListBox>(
            Style.DeriveFromDefault<ListBox>(
                [
                    .. ItemViewSetters(),
                    IslandsStyles.Fixed(ListBox.ZebraStripingProperty, false),
                    IslandsStyles.Fixed(ListBox.ItemHeightProperty, IslandsMetrics.ListRowHeight),
                ],
                ItemViewTriggers()));

        // A row of a plain items control is found by name, which skips type-rule lookup entirely, so the
        // framework key has to be shadowed as well as the container type given a rule.
        sheet.Define(ItemsControlRowStyleName, static () => RowStyle());
        sheet.Define<ItemContainer>(RowStyle());
    }

    /// <summary>
    /// Islands paints a list or tree as a bare component: a square plate with one pixel of edge, and no
    /// rounding on either the plate or its rows.
    /// </summary>
    internal static SetterBase[] ItemViewSetters() =>
    [
        IslandsStyles.Background(c => c.ComponentBackground),
        IslandsStyles.BorderBrush(c => c.Border),
        IslandsStyles.Foreground(c => c.Text),
        IslandsStyles.Fixed(Control.CornerRadiusProperty, 0.0),
        IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
    ];

    internal static StateTrigger[] ItemViewTriggers() =>
    [
        IslandsStyles.Disabled(
            IslandsStyles.Background(c => c.ButtonDisabledBackground),
            IslandsStyles.BorderBrush(c => c.BorderDisabled),
            IslandsStyles.Foreground(c => c.DisabledText)),
        IslandsStyles.Invalid(),
    ];

    /// <summary>
    /// The row: transparent at rest, so the owner's zebra stripe shows through, tinted under the pointer and
    /// plated when selected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Islands does state a hover fill — <c>List.hoverBackground</c>, <c>Tree.hoverBackground</c> and
    /// <c>Table.hoverBackground</c> all resolve to <c>selection-bg-hovered</c> — and the framework's own
    /// hover trigger cannot supply it: an ordinary setter of this style outranks a trigger of the style it is
    /// based on, so the transparent idle fill that keeps a list unstriped also drops the derived tint. The
    /// trigger is restated here instead, ahead of the selection's, because a later trigger wins.
    /// </para>
    /// <para>
    /// Neither inset is this rule's. The plate's comes from the owner's <c>Padding</c>: a plate is inflated
    /// back over the item padding and would otherwise reach the edge of the scrolled content. The label's
    /// comes from the item padding itself, which the presenter deflates the row rect by
    /// (<c>FixedHeightItemsPresenter</c>) — a row's own <c>Control.Padding</c> would sit on top of that and
    /// double it, which is how a drop-down's rows came out with twelve pixels of air instead of Islands's six.
    /// </para>
    /// <para>
    /// The row never carries the focus state of its owner, so Islands's
    /// <c>List.selectionInactiveBackground</c> has no state to hang on here; selection stays accent-colored
    /// whether or not the list owns the focus.
    /// </para>
    /// </remarks>
    private static Style RowStyle() =>
        Style.DeriveFromDefault<ItemContainer>(
            [IslandsStyles.Background(c => c.ComponentBackground.WithAlpha(0))],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.HoverBackground)),
                IslandsStyles.When(VisualStateFlags.Selected,
                    IslandsStyles.SelectionPlate(),
                    IslandsStyles.SelectionTextLabel()),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// The tabbed pane's content plate. The chip on each tab is named on from
/// <c>IslandsTabPlate</c>, because the header type is internal and only a style name reaches it.
/// </summary>
internal static class IslandsTabStyles
{
    internal static void Register(StyleSheet sheet)
    {
        sheet.Define<TabControl>(TabControlStyle());
        sheet.Define<ContentControl>(ContentSurfaceStyle());
        sheet.Define(IslandsStyleNames.TabHeader, static () => TabHeaderStyle(inactive: false));
        sheet.Define(IslandsStyleNames.TabHeaderInactive, static () => TabHeaderStyle(inactive: true));
    }

    /// <summary>The fill under a claimed tab's label.</summary>
    internal static Color ClaimedFill(IslandsVariantColors colors, bool inactive) =>
        inactive ? colors.TabSelectedInactiveBackground : colors.TabSelectedBackground;

    /// <summary>The edge around a claimed tab's label.</summary>
    internal static Color ClaimedEdge(IslandsVariantColors colors, bool inactive) =>
        inactive ? colors.TabSelectedInactiveBorder : colors.TabSelectedBorder;

    /// <summary>
    /// The fill an unclaimed tab draws, which is the claimed one with nothing showing rather than nothing
    /// at all.
    /// </summary>
    /// <remarks>
    /// The distinction is the whole reason a click does not flash. A transition interpolates each channel
    /// independently of the others and of alpha, so a resting value of transparent <em>black</em> crossing to
    /// an opaque pale blue passes through a half-transparent dark grey — the chip dims before it brightens,
    /// and dims again on the way back out. Sharing the channels leaves alpha as the only thing that moves, and
    /// a value at alpha zero paints nothing either way.
    /// <para>
    /// <see cref="IslandsVariantColors.TabHoverBackground"/> is deliberately not used here even though it is
    /// the strip's resting colour by the book: it is a neutral wash, and only the <em>claimed</em> plate's
    /// own hue lets the pair interpolate cleanly.
    /// </para>
    /// </remarks>
    internal static Color RestingFill(IslandsVariantColors colors, bool inactive) =>
        ClaimedFill(colors, inactive).WithAlpha(0);

    /// <summary>The edge an unclaimed tab draws, on the same terms as <see cref="RestingFill"/>.</summary>
    internal static Color RestingEdge(IslandsVariantColors colors, bool inactive) =>
        ClaimedEdge(colors, inactive).WithAlpha(0);

    /// <summary>
    /// Islands's <c>TabbedPane</c> content area carries no fill of its own: the tabs sit on whatever surface
    /// holds them, and the only line is <c>TabbedPane.contentAreaColor</c>, one pixel under the strip, which
    /// the underline drawer owns.
    /// </summary>
    /// <remarks>
    /// The zero alpha matters as much as the color: a tabbed pane laid inside an island would otherwise paint
    /// the window's own layer over the island and read as a darker plate inset into the panel.
    /// </remarks>
    private static Style TabControlStyle() =>
        Style.DeriveFromDefault<TabControl>(
            [
                IslandsStyles.Background(c => c.PanelBackground.WithAlpha(0)),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, 0.0),
                IslandsStyles.Metric(Control.PaddingProperty, t => t.Metrics.ContainerPadding),
            ],
            [
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    /// <summary>
    /// The plate every other <see cref="ContentControl"/> carries: nothing. The type is the base of
    /// <c>ScrollViewer</c>, <c>UserControl</c> and <c>HeaderedContentControl</c>, so a rule that colored it
    /// by state would color the page those containers fill.
    /// </summary>
    /// <remarks>
    /// The fill is the hover hue at zero alpha rather than <see cref="Color.Transparent"/>, which is white
    /// and would flash bright against a dark surface should a state ever animate it.
    /// </remarks>
    private static Style ContentSurfaceStyle() =>
        Style.DeriveFromDefault<ContentControl>(
            [
                IslandsStyles.Background(c => c.TabHoverBackground.WithAlpha(0)),
                IslandsStyles.BorderBrush(c => c.Border.WithAlpha(0)),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, 0.0),
                IslandsStyles.Metric(TextElement.FontFamilyProperty, t => t.Metrics.FontFamily),
                IslandsStyles.Metric(TextElement.FontSizeProperty, t => t.Metrics.FontSize),
            ]);

    /// <summary>
    /// The plate a tab header draws under its label.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Islands claims a tab with a chip: a rounded rectangle that is filled and edged, standing on the strip.
    /// An earlier revision claimed it with a bar under the label instead, which is the older New UI's
    /// <c>TabbedPane</c> treatment — the two are different shapes, not two colours of one shape, so a tab
    /// cannot be moved between them by recolouring alone.
    /// </para>
    /// <para>
    /// The border is stated at every state, one pixel wide and transparent when nothing is claimed. A border
    /// that appeared only on selection would resize the tab the moment it was clicked, and a strip of tabs
    /// that shifts under the pointer is worse than one with a line around it.
    /// </para>
    /// </remarks>
    private static Style TabHeaderStyle(bool inactive) =>
        Style.DeriveFromDefault<ContentControl>(
            [
                IslandsStyles.Background(c => RestingFill(c, inactive)),
                IslandsStyles.BorderBrush(c => RestingEdge(c, inactive)),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 1.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.TabPadding),
                IslandsStyles.Fixed(FrameworkElement.MinHeightProperty, IslandsMetrics.TabHeight),
                IslandsStyles.Metric(TextElement.FontFamilyProperty, t => t.Metrics.FontFamily),
                IslandsStyles.Metric(TextElement.FontSizeProperty, t => t.Metrics.FontSize),
            ],
            [
                // An unclaimed tab is a wash of the surface, not an edge; the chip is the only thing edged.
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.TabHoverBackground)),
                IslandsStyles.When(VisualStateFlags.Pressed, IslandsStyles.Background(c => c.TabHoverBackground)),
                IslandsStyles.When(VisualStateFlags.Selected,
                    IslandsStyles.Background(c => ClaimedFill(c, inactive)),
                    IslandsStyles.BorderBrush(c => ClaimedEdge(c, inactive)),
                    IslandsStyles.Foreground(c => c.Text)),
                // A claimed tab keeps its chip while hovered; the wash would only muddy the fill.
                IslandsStyles.When(VisualStateFlags.Selected | VisualStateFlags.Hot,
                    IslandsStyles.Background(c => ClaimedFill(c, inactive)),
                    IslandsStyles.BorderBrush(c => ClaimedEdge(c, inactive))),
                IslandsStyles.Disabled(
                    IslandsStyles.Foreground(c => c.DisabledText),
                    IslandsStyles.BorderBrush(c => c.BorderDisabled)),
            ],
            [
                // Snapped rather than faded, because the hue alone does not save this one: hover is a neutral
                // wash and the chip is a tint, so any interpolation between them still crosses the muddy
                // middle. A strip also has two plates changing at once — one arriving, one leaving — and two
                // plates cross-fading beside each other read as a flicker however well each one is chosen.
                // A tab is claimed the instant it is clicked. A transition declared here wins, because the
                // lookup takes the first match while walking the inheritance chain, and the shortest duration
                // the type accepts stands in for "none".
                Transition.Create(Control.BackgroundProperty, SnappedMs),
                Transition.Create(Control.BorderBrushProperty, SnappedMs),
            ]);

    /// <summary>
    /// A transition short enough to be a cut, used where a fade would be read as a flicker.
    /// </summary>
    /// <remarks>
    /// Not zero: <see cref="Transition"/> rejects a non-positive duration outright, so the shortest duration
    /// it accepts is the way to say "change now, do not animate".
    /// </remarks>
    internal const int SnappedMs = 1;
}

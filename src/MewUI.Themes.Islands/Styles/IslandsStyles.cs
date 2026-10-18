using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands;

/// <summary>
/// Registers the Islands rules into an application style sheet.
/// </summary>
/// <remarks>
/// Two routes reach a control's visuals. A type rule (<see cref="StyleSheet.Define{T}(Style)"/>) is
/// looked up by walking the control's base types, so one rule covers a family. A named style shadows a
/// framework key of the same name (<see cref="StyleSheet.Define(string, Func{Style})"/> resolves before
/// <c>FrameworkNamedStyles</c>, <c>StyleScopeResolver.cs:78-86</c>), which is the only way to restyle a
/// sub-element whose type is internal — the tab strip's scroll buttons, a combo box's popup list, a
/// menu row's plate.
/// </remarks>
internal static class IslandsStyles
{
    internal static void Register(StyleSheet sheet)
    {
        IslandsButtonStyles.Register(sheet);
        IslandsTextInputStyles.Register(sheet);
        IslandsListStyles.Register(sheet);
        IslandsGridTreeStyles.Register(sheet);
        IslandsMenuStyles.Register(sheet);
        IslandsTabStyles.Register(sheet);
        IslandsWindowStyles.Register(sheet);
        IslandsSurfaceStyles.Register(sheet);
    }

    /// <summary>
    /// A setter whose value comes from the color table of the theme being rendered.
    /// </summary>
    /// <remarks>
    /// Resolving through <see cref="Theme"/> rather than capturing a variant is what makes a light/dark
    /// switch repaint without rebuilding the tree: every state change re-runs these lambdas with the
    /// new theme.
    /// </remarks>
    internal static Setter From<T>(MewProperty<T> property, Func<IslandsVariantColors, T> select) =>
        Setter.Create(property, theme => select(IslandsTheme.Colors(theme)));

    /// <summary>A setter that reads the metrics record, so a custom <c>UseMetrics</c> still applies.</summary>
    internal static Setter Metric<T>(MewProperty<T> property, Func<Theme, T> select) =>
        Setter.Create(property, select);

    /// <summary>A setter with a fixed Islands constant (an arc, an inset) that no metric can express.</summary>
    internal static Setter Fixed<T>(MewProperty<T> property, T value) =>
        Setter.Create(property, value);

    internal static Setter Foreground(Func<IslandsVariantColors, Color> select) =>
        From(TextElement.ForegroundProperty, select);

    internal static Setter Background(Func<IslandsVariantColors, Color> select) =>
        From(Control.BackgroundProperty, select);

    internal static Setter BorderBrush(Func<IslandsVariantColors, Color> select) =>
        From(Control.BorderBrushProperty, select);

    /// <summary>The Islands text color of a control that is not enabled.</summary>
    internal static Setter DisabledText() =>
        Foreground(c => c.DisabledText);

    /// <summary>
    /// A fill Islands paints as a translucent overlay, composited onto the surface it lands on.
    /// </summary>
    /// <remarks>
    /// Islands states hover and press as overlays so they tint whatever is behind them. MewUI animates a
    /// color change with a straight RGBA interpolation (<c>Color.Lerp</c>), so ramping an <em>opaque</em> face
    /// into a translucent overlay passes through a half-transparent mid-tone: on IntelliJ Light a white button
    /// dips to about <c>#c5c5c5</c> on its way into hover and back, which reads as a flicker at the framework's
    /// 200 ms. Compositing the overlay onto the face first leaves both ends of the ramp opaque, so the change
    /// stays monotone — and since the face is what the overlay would have landed on, the settled fill is
    /// exactly the one Islands draws.
    /// </remarks>
    internal static Setter Overlay(
        Func<IslandsVariantColors, Color> face,
        Func<IslandsVariantColors, Color> overlay) =>
        Background(colors => Color.Composite(face(colors), overlay(colors)));

    /// <summary>
    /// The plate of a selected row: the wash MewUI derives for a selection, which is also what it paints
    /// behind selected text and inside a menu.
    /// </summary>
    /// <remarks>
    /// Not Islands's solid <c>List.selectionBackground</c>. An item's label is colored by the control itself
    /// from the palette's selection text, once, when the row is realized — a selection change re-runs only the
    /// row's visual states — so a plate the label's weight cannot survive would leave clicked rows unreadable.
    /// See <see cref="IslandsSeeds"/>.
    /// </remarks>
    internal static Setter SelectionPlate() =>
        Metric(Control.BackgroundProperty, t => t.Palette.SelectionBackground);

    /// <summary>The label color the framework writes onto a selected row, restated for non-text content.</summary>
    internal static Setter SelectionTextLabel() =>
        Metric(TextElement.ForegroundProperty, t => t.Palette.SelectionText);

    /// <summary>
    /// The arc of a surface that floats.
    /// </summary>
    /// <remarks>
    /// Not Islands's <c>Popup.borderCornerRadius</c> but the radius the chrome around a popup draws its drop
    /// shadow with, which MewUI takes from the metrics record and nothing else. A surface that keeps the square
    /// plate of an inline list leaves its corners outside that shadow, so the blur curves away from the panel it
    /// is supposed to be under.
    /// </remarks>
    internal static Setter PopupCornerRadius() =>
        Metric(Control.CornerRadiusProperty, t => t.Metrics.ControlCornerRadius);

    /// <summary>
    /// The arc of a drop-down's floating list, stated one step under the radius its shadow is blurred with so
    /// that the rows inside it land on Islands's own plate arc.
    /// </summary>
    /// <remarks>
    /// A menu and a tool tip are surfaces whose own corner is what the eye reads, and they keep
    /// <see cref="PopupCornerRadius"/>. A drop-down's list is read through its rows: MewUI gives a row plate
    /// the arc of the list minus that list's border inset (<c>ItemsControl.UpdateItemCornerRadius</c>), so a
    /// list rounded like the shadow rounds its pills seven pixels where Islands rounds them
    /// <see cref="IslandsMetrics.SelectionArc"/>. The list therefore takes the shadow radius less
    /// <see cref="PopupArcStep"/>, and the three pixels of disagreement sit on a blurred edge; it is the one
    /// deviation this file takes on purpose and it is recorded in <c>docs/color-map.md</c>.
    /// </remarks>
    internal static Setter DropDownListCornerRadius() =>
        Metric(Control.CornerRadiusProperty, t => Math.Max(0, t.Metrics.ControlCornerRadius - PopupArcStep));

    /// <summary>How far under the shadow's radius a drop-down's list is rounded, to land its pill on 4.</summary>
    private const double PopupArcStep = 3;

    internal static StateTrigger When(VisualStateFlags match, params SetterBase[] setters) =>
        new() { Match = match, Setters = setters };

    internal static StateTrigger When(VisualStateFlags match, VisualStateFlags exclude, params SetterBase[] setters) =>
        new() { Match = match, Exclude = exclude, Setters = setters };

    /// <summary>The state every control has when it is unusable: Islands greys the fill and the text.</summary>
    internal static StateTrigger Disabled(params SetterBase[] setters) =>
        new() { Match = VisualStateFlags.None, Exclude = VisualStateFlags.Enabled, Setters = setters };

    /// <summary><c>Component.error.focusedBorderColor</c>, drawn as the validation border.</summary>
    internal static StateTrigger Invalid() =>
        When(VisualStateFlags.Invalid, BorderBrush(c => c.Error));
}

/// <summary>
/// The style keys this theme defines besides the framework's own. They are internal because a control
/// reaches them through the extension methods in <c>IslandsExtensions</c>, not by naming them.
/// </summary>
internal static class IslandsStyleNames
{
    /// <summary>A button with the toolbar's insets and no minimum width.</summary>
    internal const string CompactButton = "islands-compact-button";

    /// <summary>The same metric for a button that carries a menu, small enough to sit in a list header band.</summary>
    internal const string CompactDropDownButton = "islands-compact-dropdown";

    /// <summary>A flat drop-down for a panel header, on the tool strip's face.</summary>
    internal const string ToolbarDropDownButton = "islands-toolbar-dropdown";

    /// <summary>
    /// The plate of a tab header. The header type is internal, so <c>IslandsTabPlate</c> names this style
    /// onto each header it finds in the strip rather than the strip's plates being colored by a type rule.
    /// </summary>
    internal const string TabHeader = "islands-tab-header";

    /// <summary>
    /// The same plate for a strip in a window that does not hold the focus.
    /// </summary>
    /// <remarks>
    /// A separate name rather than a state on the one above, because what changes is not the header's own
    /// state: the window lost the focus, and a style cannot read that. The drawer already walks the headers
    /// every pass and knows the window, so it is the one place that can tell them apart — and the selected
    /// tab is then a muted plate rather than the accent one, which is how the system's own windows behave.
    /// </remarks>
    internal const string TabHeaderInactive = "islands-tab-header-inactive";
}

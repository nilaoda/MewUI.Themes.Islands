using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// Buttons, toggles, drop-downs and toolbars.
/// </summary>
internal static class IslandsButtonStyles
{
    internal static void Register(StyleSheet sheet)
    {
        sheet.Define<Button>(ButtonStyle());
        sheet.Define<ToggleButton>(ToggleButtonStyle());
        sheet.Define<SplitButton>(SplitButtonStyle());
        sheet.Define<DropDownButton>(DropDownButtonStyle());
        sheet.Define<DropDownBase>(DropDownBaseStyle());
        sheet.Define<ComboBox>(ComboBoxStyle());
        sheet.Define<ToolBar>(ToolBarStyle());

        // Shadowed framework keys. A control that resolves a StyleName never consults the type rules, so
        // this is the only handle an application has on the sub-elements the framework builds for itself:
        // a tab strip's scroll arrows, a date picker's calendar, a toolbar's own buttons.
        sheet.Define(BuiltInStyles.FlatButton, static () => IslandsButtonShadow());
        sheet.Define(BuiltInStyles.AccentButton, static () => AccentButtonShadow());
        sheet.Define(BuiltInStyles.ToolBarButton, static () => ToolBarButtonShadow());
        sheet.Define(BuiltInStyles.ToolBarToggleButton, static () => ToolBarToggleButtonShadow());
        sheet.Define(BuiltInStyles.ComboBoxPopup, static () => ComboBoxPopupShadow());

        sheet.Define(BuiltInStyles.DatePickerPopup, static () => DatePickerPopupShadow());

        sheet.Define(IslandsStyleNames.CompactButton, static () => CompactButtonStyle());
        sheet.Define(IslandsStyleNames.CompactDropDownButton, static () => CompactDropDownButtonStyle());
        sheet.Define(IslandsStyleNames.ToolbarDropDownButton, static () => ToolbarDropDownButtonStyle());
    }

    /// <summary>
    /// The chrome every button family member shares. Each state has to be restated as a trigger of the
    /// derived style: an ordinary setter of this style outranks a trigger of the <c>BasedOn</c> style, so
    /// a state left out of the list below stops reacting at all.
    /// </summary>
    private static SetterBase[] ChromeSetters() =>
    [
        IslandsStyles.Background(c => c.ButtonBackground),
        IslandsStyles.BorderBrush(c => c.ButtonBorder),
        IslandsStyles.Foreground(c => c.ButtonForeground),
        IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ButtonPadding),
        IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.ButtonArc),
        IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
        IslandsStyles.Metric(FrameworkElement.MinHeightProperty, t => t.Metrics.BaseControlHeight),
        IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, IslandsMetrics.ButtonMinWidth),
    ];

    /// <summary>
    /// The states of a button. Islands gives a button a hover <em>fill</em> and no hover edge — there is no
    /// <c>Button.hoverBorderColor</c> in either variant, while <c>ActionButton.hoverBorderColor</c> shows that
    /// JetBrains states one where it wants one — so the edge is restated at its idle color to keep the
    /// framework's own hover tint out of this rule's state, and only the fill moves.
    /// </summary>
    /// <remarks>
    /// The fill goes through <see cref="IslandsStyles.Overlay"/> rather than straight at the overlay: a ramp
    /// from an opaque face into a translucent one is what made a hovered button flash mid-grey. Focus keeps
    /// the accent edge — the one deliberate deviation, because MewUI's focus ring is an opt-in adorner and a
    /// control that is focused without it would otherwise show no cue at all; see <c>docs/color-map.md</c>.
    /// </remarks>
    private static StateTrigger[] ChromeTriggers() =>
    [
        IslandsStyles.When(VisualStateFlags.Hot,
            IslandsStyles.Overlay(c => c.ButtonBackground, c => c.ButtonHover),
            IslandsStyles.BorderBrush(c => c.ButtonBorder)),
        IslandsStyles.When(VisualStateFlags.Focused,
            IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.When(VisualStateFlags.Pressed,
            IslandsStyles.Overlay(c => c.ButtonBackground, c => c.ButtonPressed),
            IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.Disabled(
            IslandsStyles.Background(c => c.ButtonDisabledBackground),
            IslandsStyles.BorderBrush(c => c.BorderDisabled),
            IslandsStyles.Foreground(c => c.ButtonDisabledForeground)),
    ];

    private static Style ButtonStyle() =>
        Style.DeriveFromDefault<Button>(ChromeSetters(), ChromeTriggers());

    private static Style ToggleButtonStyle() =>
        Style.DeriveFromDefault<ToggleButton>(
            ChromeSetters(),
            [
                .. ChromeTriggers(),
                // A checked toggle keeps its fill under the pointer; Islands has no hover-over-selected tint,
                // and — as for an unchecked button — no hover edge either.
                IslandsStyles.When(VisualStateFlags.Checked, IslandsStyles.Background(c => c.ButtonSelected)),
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Hot,
                    IslandsStyles.Background(c => c.ButtonSelected),
                    IslandsStyles.BorderBrush(c => c.ButtonBorder)),
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Focused,
                    IslandsStyles.BorderBrush(c => c.BorderFocused)),
                // Pressed composited onto the plate it lands on, not onto the unchecked face.
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Pressed,
                    IslandsStyles.Overlay(c => c.ButtonSelected, c => c.ButtonPressed),
                    IslandsStyles.BorderBrush(c => c.BorderFocused)),
                // ToggleButton.disabledSelectedBackground, which is neither the disabled nor the selected fill.
                IslandsStyles.When(VisualStateFlags.Checked, VisualStateFlags.Enabled,
                    IslandsStyles.Background(c => c.ToggleDisabledSelectedBackground)),
            ]);

    private static Style SplitButtonStyle() =>
        Style.DeriveFromDefault<SplitButton>(ChromeSetters(), ChromeTriggers());

    private static Style DropDownButtonStyle() =>
        Style.DeriveFromDefault<DropDownButton>(ChromeSetters(), ChromeTriggers());

    /// <summary>
    /// A drop-down field is a component rather than a button: Islands paints it on the input surface and
    /// gives it <c>ComboBox.borderCornerRadius</c>, one step tighter than a button's arc.
    /// </summary>
    private static Style DropDownBaseStyle() =>
        Style.DeriveFromDefault<DropDownBase>(DropDownSetters(), DropDownTriggers());

    /// <summary>
    /// A combo box copies its own zebra switch into the popup list it builds, as a local value on that list,
    /// so the switch has to be thrown here — Islands's dropdowns are unstriped, and the stripe a list now
    /// mixes out of the darkened control-background seed would otherwise show.
    /// </summary>
    private static Style ComboBoxStyle() =>
        Style.DeriveFromDefault<ComboBox>(
            [
                .. DropDownSetters(),
                IslandsStyles.Fixed(ComboBox.ZebraStripingProperty, false),
                // The popup list copies the box's item height into itself as a local value the moment it
                // opens, so a row height stated on the list's own style would be overwritten. The box is
                // where it has to be said, and the box's rule is the only rule that may say it: the property
                // is registered on ComboBox, and a setter naming a type below the rule's target is refused.
                IslandsStyles.Fixed(ComboBox.ItemHeightProperty, IslandsMetrics.ListRowHeight),
            ],
            DropDownTriggers());

    private static SetterBase[] DropDownSetters() =>
    [
        IslandsStyles.Background(c => c.ComponentBackground),
        IslandsStyles.BorderBrush(c => c.Border),
        IslandsStyles.Foreground(c => c.Text),
        IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ComponentPadding),
        IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
        IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
        IslandsStyles.Metric(FrameworkElement.MinHeightProperty, t => t.Metrics.BaseControlHeight),
        IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, IslandsMetrics.ComponentMinWidth),
    ];

    private static StateTrigger[] DropDownTriggers() =>
    [
        IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.When(VisualStateFlags.Focused, IslandsStyles.BorderBrush(c => c.BorderFocused)),
        // The popup takes the focus away from the field, so the open state has to hold the edge lit.
        IslandsStyles.When(VisualStateFlags.Active, IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.When(VisualStateFlags.Pressed,
            IslandsStyles.Overlay(c => c.ComponentBackground, c => c.HoverBackground),
            IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.Disabled(
            IslandsStyles.Background(c => c.ButtonDisabledBackground),
            IslandsStyles.BorderBrush(c => c.BorderDisabled),
            IslandsStyles.Foreground(c => c.DisabledText)),
        IslandsStyles.Invalid(),
    ];

    private static Style ToolBarStyle() =>
        Style.DeriveFromDefault<ToolBar>(
        [
            IslandsStyles.Background(c => c.ToolBarBackground),
            IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarPadding),
            IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
        ]);

    /// <summary>The borderless button behind a tab strip's scroll arrows, a calendar header and a breadcrumb.</summary>
    private static Style IslandsButtonShadow() =>
        Style.DeriveFromDefault<Button>(
            [
                // The hover hue at zero alpha rather than Color.Transparent, which is white and would flash
                // bright against a dark toolbar on the way in and out.
                IslandsStyles.Background(c => c.ToolbarHover.WithAlpha(0)),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarButtonPadding),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.ToolbarHover)),
                IslandsStyles.When(VisualStateFlags.Focused, IslandsStyles.Background(c => c.FocusRing.WithAlpha(96))),
                IslandsStyles.When(VisualStateFlags.Focused | VisualStateFlags.Hot,
                    IslandsStyles.Background(c => c.FocusRing.WithAlpha(153))),
                IslandsStyles.When(VisualStateFlags.Pressed, IslandsStyles.Background(c => c.ToolbarPressed)),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    /// <summary>
    /// Islands' default button: a solid blue face, a darker edge, white bold text. Shadowing the
    /// framework's accent key is what makes <c>.IslandsPrimaryButton()</c> and every accent-styled button the
    /// framework builds for itself read as Islands.
    /// </summary>
    private static Style AccentButtonShadow() =>
        Style.DeriveFromDefault<Button>(
            [
                IslandsStyles.Background(c => c.PrimaryButtonBackground),
                IslandsStyles.BorderBrush(c => c.PrimaryButtonBorder),
                IslandsStyles.Foreground(c => c.PrimaryButtonForeground),
                IslandsStyles.Fixed(TextElement.FontWeightProperty, FontWeight.Bold),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ButtonPadding),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.ButtonArc),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, IslandsMetrics.BorderThickness),
                IslandsStyles.Metric(FrameworkElement.MinHeightProperty, t => t.Metrics.BaseControlHeight),
                IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, IslandsMetrics.ButtonMinWidth),
            ],
            [
                // As for an ordinary button: only the fill moves on hover and press, and the edge the theme
                // states for the default button is what stays on it.
                IslandsStyles.When(VisualStateFlags.Hot,
                    IslandsStyles.Background(c => c.PrimaryButtonHover),
                    IslandsStyles.BorderBrush(c => c.PrimaryButtonBorder)),
                IslandsStyles.When(VisualStateFlags.Focused,
                    IslandsStyles.BorderBrush(c => c.PrimaryButtonFocusedBorder)),
                IslandsStyles.When(VisualStateFlags.Pressed,
                    IslandsStyles.Background(c => c.PrimaryButtonPressed),
                    IslandsStyles.BorderBrush(c => c.PrimaryButtonBorder)),
                IslandsStyles.Disabled(
                    IslandsStyles.Background(c => c.ButtonDisabledBackground),
                    IslandsStyles.BorderBrush(c => c.BorderDisabled),
                    IslandsStyles.Foreground(c => c.ButtonDisabledForeground),
                    IslandsStyles.Fixed(TextElement.FontWeightProperty, FontWeight.Normal)),
            ]);

    private static Style ToolBarButtonShadow() =>
        Style.DeriveFromDefault<Button>(
            [
                IslandsStyles.Background(c => c.ToolbarHover.WithAlpha(0)),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarButtonPadding),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.ToolbarHover)),
                IslandsStyles.When(VisualStateFlags.Pressed, IslandsStyles.Background(c => c.ToolbarPressed)),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    private static Style ToolBarToggleButtonShadow() =>
        Style.DeriveFromDefault<ToggleButton>(
            [
                IslandsStyles.Background(c => c.ToolbarHover.WithAlpha(0)),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarButtonPadding),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.ToolbarHover)),
                IslandsStyles.When(VisualStateFlags.Pressed, IslandsStyles.Background(c => c.ToolbarPressed)),
                IslandsStyles.When(VisualStateFlags.Checked, IslandsStyles.Background(c => c.ToolbarSelected)),
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Hot,
                    IslandsStyles.Background(c => c.ToolbarHover)),
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Pressed,
                    IslandsStyles.Background(c => c.ToolbarPressed)),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    /// <summary>
    /// A drop-down's list: Islands's <c>popup-bg</c> surface, its own darker edge, and rows that read as pills
    /// rather than as full-width bands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The list is reached by name and never by type — a control with a <c>StyleName</c> does not consult the
    /// type rules — so this rule is the only place a drop-down's geometry can be stated. Three numbers carry
    /// it, and none of them is inherited from anywhere else:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>Padding</c> is what insets a row, and with it the selection plate. A plate is inflated back over the
    /// item padding (<c>ItemContainer.OnRender</c>) and so always reaches the edge of the scrolled content,
    /// which is why the item padding cannot do this and why the vertical inset has to share the property.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>ItemPadding</c> insets the row's own label inside its plate; the framework only ever copies
    /// <c>ItemHeight</c>, the zebra switch, the selection and the template into this list, so this one is
    /// free to state.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>CornerRadius</c> is the pill's arc as much as the surface's: MewUI derives a row plate's radius
    /// from its list's (<c>ItemsControl.UpdateItemCornerRadius</c>), so it is stated one step under the radius
    /// the chrome blurs the drop shadow with.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    private static Style ComboBoxPopupShadow() =>
        Style.DeriveFromDefault<ListBox>(
        [
            IslandsStyles.Background(c => c.MenuBackground),
            IslandsStyles.BorderBrush(c => c.ComboPopupBorder),
            IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
            IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.PopupContentPadding),
            IslandsStyles.Fixed(ListBox.ItemPaddingProperty, IslandsMetrics.RowPadding),
            IslandsStyles.Fixed(ListBox.ItemHeightProperty, IslandsMetrics.ListRowHeight),
            IslandsStyles.DropDownListCornerRadius(),
        ]);

    private static Style DatePickerPopupShadow() =>
        Style.DeriveFromDefault<Calendar>(
        [
            IslandsStyles.Background(c => c.MenuBackground),
            IslandsStyles.BorderBrush(c => c.MenuBorder),
            IslandsStyles.PopupCornerRadius(),
        ]);

    /// <summary>Islands's toolbar metric on a button: the strip of actions next to a search field.</summary>
    private static Style CompactButtonStyle() =>
        Style.DeriveFromDefault<Button>(
            [
                .. ChromeSetters(),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarButtonPadding),
                IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, 0.0),
            ],
            ChromeTriggers());

    /// <summary>The same metric on a drop-down, so a group menu fits inside a list's header band.</summary>
    private static Style CompactDropDownButtonStyle() =>
        Style.DeriveFromDefault<DropDownButton>(
            [
                .. ChromeSetters(),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarButtonPadding),
                IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, 0.0),
            ],
            ChromeTriggers());

    /// <summary>
    /// A drop-down in a panel header: the tool strip's flat face and no minimum width, so a panel's
    /// "+" and "⋯" read as small buttons rather than as fields.
    /// </summary>
    private static Style ToolbarDropDownButtonStyle() =>
        Style.DeriveFromDefault<DropDownButton>(
            [
                IslandsStyles.Background(c => c.ToolbarHover.WithAlpha(0)),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarButtonPadding),
                IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, 0.0),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.ToolbarHover)),
                IslandsStyles.When(VisualStateFlags.Pressed, IslandsStyles.Background(c => c.ToolbarPressed)),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);
}

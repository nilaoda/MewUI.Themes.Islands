using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Drawing;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// Text fields, spinners, check boxes, radio buttons, expanders and segmented controls.
/// </summary>
internal static class IslandsTextInputStyles
{
    internal static void Register(StyleSheet sheet)
    {
        // One rule for the whole family: the sheet walks base types, so TextBase covers TextBox,
        // PasswordBox, MultiLineTextBox and SyntaxViewer, which add no colors of their own in Islands.
        sheet.Define<TextBase>(TextInputStyle(TextBaseFaceSetters()));
        sheet.Define<NumericUpDown>(NumericUpDownStyle());
        sheet.Define<CheckBox>(CheckLikeStyle<CheckBox>(CheckBoxFaceSetters(), CheckBoxTriggers()));
        sheet.Define<RadioButton>(CheckLikeStyle<RadioButton>(RadioButtonFaceSetters(), RadioButtonTriggers()));
        sheet.Define<Expander>(ExpanderStyle());
        sheet.Define<SegmentedControl>(SegmentedControlStyle());
        sheet.Define<SegmentButton>(SegmentButtonStyle());
    }

    /// <summary>The Islands component: a flat field, one pixel of grey edge, the focus color on the edge when active.</summary>
    private static SetterBase[] TextBaseFaceSetters() =>
    [
        IslandsStyles.Background(c => c.ComponentBackground),
        IslandsStyles.BorderBrush(c => c.Border),
        IslandsStyles.Foreground(c => c.Text),
        IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ComponentPadding),
        IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.ComponentArc),
        IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
        IslandsStyles.Metric(FrameworkElement.MinHeightProperty, t => t.Metrics.BaseControlHeight),
    ];

    private static StateTrigger[] ComponentTriggers() =>
    [
        IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.When(VisualStateFlags.Focused, IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.When(VisualStateFlags.Pressed, IslandsStyles.BorderBrush(c => c.BorderFocused)),
        IslandsStyles.Disabled(
            IslandsStyles.Background(c => c.ButtonDisabledBackground),
            IslandsStyles.BorderBrush(c => c.BorderDisabled),
            IslandsStyles.Foreground(c => c.DisabledText)),
        IslandsStyles.Invalid(),
    ];

    private static Style TextInputStyle(SetterBase[] setters) =>
        Style.DeriveFromDefault<TextBase>(setters, ComponentTriggers());

    private static Style NumericUpDownStyle() =>
        Style.DeriveFromDefault<NumericUpDown>(
            [
                .. TextBaseFaceSetters(),
                // Spinner/minimumSize = 72,28: a spinner is as wide as a button, not as a field.
                IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, IslandsMetrics.ButtonMinWidth),
            ],
            ComponentTriggers());

    /// <summary>
    /// A check box draws its face from its own fill and edge, and keeps its label at the window text color
    /// in every state — Islands lights the mark instead of the label, so no trigger here touches the
    /// foreground. The face itself is drawn by <see cref="IslandsToggleIndicator"/>, which the template puts in
    /// place of the framework's icon.
    /// </summary>
    private static SetterBase[] CheckBoxFaceSetters() =>
    [
        IslandsStyles.Background(c => c.ComponentBackground),
        IslandsStyles.BorderBrush(c => c.CheckBorder),
        IslandsStyles.Foreground(c => c.Text),
        IslandsStyles.Fixed(Control.PaddingProperty, new Thickness(2, 2, 2, 2)),
        // The indicator halves this to get the box's corner, so Islands's CheckBox.arc = 4 is 4 here.
        IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
        IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
        IslandsStyles.Metric(FrameworkElement.MinHeightProperty, t => t.Metrics.BaseControlHeight),
        IslandsStyles.Fixed(Control.TemplateProperty, (ControlTemplate?)IslandsToggleTemplates.CheckBoxFace),
    ];

    private static SetterBase[] RadioButtonFaceSetters() =>
    [
        IslandsStyles.Background(c => c.ComponentBackground),
        IslandsStyles.BorderBrush(c => c.CheckBorder),
        IslandsStyles.Foreground(c => c.Text),
        IslandsStyles.Fixed(Control.PaddingProperty, new Thickness(2, 2, 2, 2)),
        IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
        IslandsStyles.Metric(FrameworkElement.MinHeightProperty, t => t.Metrics.BaseControlHeight),
        IslandsStyles.Fixed(Control.TemplateProperty, (ControlTemplate?)IslandsToggleTemplates.RadioButtonFace),
    ];

    private static Style CheckLikeStyle<T>(SetterBase[] setters, StateTrigger[] triggers)
        where T : Control =>
        Style.DeriveFromDefault<T>(setters, triggers);

    /// <summary>Islands lights the icon's edge on hover and never its fill.</summary>
    private static SetterBase[] HoverEdge() => [IslandsStyles.BorderBrush(c => c.CheckBorderActive)];

    /// <summary>
    /// Focus is stronger than hover: <c>checkBoxFocused.svg</c> draws the idle box wearing a 2 DIP accent
    /// ring, which is the edge grown to two pixels. The indicator follows the control's border thickness.
    /// </summary>
    private static SetterBase[] FocusRing() =>
    [
        IslandsStyles.BorderBrush(c => c.CheckBorderActive),
        IslandsStyles.Fixed(Control.BorderThicknessProperty, 2.0),
    ];

    private static StateTrigger[] CheckBoxTriggers() =>
    [
        IslandsStyles.When(VisualStateFlags.Hot, HoverEdge()),
        IslandsStyles.When(VisualStateFlags.Focused, FocusRing()),
        IslandsStyles.When(VisualStateFlags.Pressed, HoverEdge()),
        IslandsStyles.When(VisualStateFlags.Checked, CheckedPlate()),
        IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Hot, CheckedPlate()),
        IslandsStyles.When(VisualStateFlags.Indeterminate, CheckedPlate()),
        IslandsStyles.Disabled(
            IslandsStyles.Background(c => c.ButtonDisabledBackground),
            IslandsStyles.BorderBrush(c => c.CheckDisabledBorder),
            IslandsStyles.Foreground(c => c.DisabledText)),
        IslandsStyles.Invalid(),
    ];

    /// <summary>A checked box keeps its plate and lights its edge; the mark is what says it is checked.</summary>
    private static SetterBase[] CheckedPlate() =>
    [
        IslandsStyles.Background(c => c.CheckFilledBackground),
        IslandsStyles.BorderBrush(c => c.CheckSelectedBorder),
    ];

    private static StateTrigger[] RadioButtonTriggers() =>
    [
        IslandsStyles.When(VisualStateFlags.Hot, HoverEdge()),
        IslandsStyles.When(VisualStateFlags.Focused, FocusRing()),
        IslandsStyles.When(VisualStateFlags.Pressed, HoverEdge()),
        IslandsStyles.When(VisualStateFlags.Checked, CheckedPlate()),
        IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Hot, CheckedPlate()),
        IslandsStyles.Disabled(
            IslandsStyles.Background(c => c.ButtonDisabledBackground),
            IslandsStyles.BorderBrush(c => c.CheckDisabledBorder),
            IslandsStyles.Foreground(c => c.DisabledText)),
        IslandsStyles.Invalid(),
    ];

    private static Style ExpanderStyle() =>
        Style.DeriveFromDefault<Expander>(
            [
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ComponentPadding),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Focused, IslandsStyles.BorderBrush(c => c.BorderFocused)),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    /// <summary>A segmented control is a row of buttons joined by one edge, so it carries the button arc.</summary>
    private static Style SegmentedControlStyle() =>
        Style.DeriveFromDefault<SegmentedControl>(
            [
                IslandsStyles.Background(c => c.ButtonBackground),
                IslandsStyles.BorderBrush(c => c.ButtonBorder),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.ButtonArc),
                IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
                IslandsStyles.Metric(FrameworkElement.MinHeightProperty, t => t.Metrics.BaseControlHeight),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.BorderBrush(c => c.BorderFocused)),
                IslandsStyles.When(VisualStateFlags.Focused, IslandsStyles.BorderBrush(c => c.BorderFocused)),
                IslandsStyles.Disabled(
                    IslandsStyles.Background(c => c.ButtonDisabledBackground),
                    IslandsStyles.BorderBrush(c => c.BorderDisabled)),
                IslandsStyles.Invalid(),
            ]);

    private static Style SegmentButtonStyle() =>
        Style.DeriveFromDefault<SegmentButton>(
            [
                IslandsStyles.Background(c => c.ButtonBackground.WithAlpha(0)),
                IslandsStyles.Foreground(c => c.ButtonForeground),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ButtonPadding),
            ],
            [
                // The segment sits on the control's own face, so the overlay is composited onto it: an opaque
                // fill on both ends of the ramp is what keeps a hover from flashing mid-grey.
                IslandsStyles.When(VisualStateFlags.Hot,
                    IslandsStyles.Overlay(c => c.ButtonBackground, c => c.ButtonHover)),
                IslandsStyles.When(VisualStateFlags.Pressed,
                    IslandsStyles.Overlay(c => c.ButtonBackground, c => c.ButtonPressed)),
                IslandsStyles.When(VisualStateFlags.Selected, IslandsStyles.Background(c => c.ButtonSelected)),
                IslandsStyles.When(VisualStateFlags.Selected | VisualStateFlags.Hot,
                    IslandsStyles.Background(c => c.ButtonSelected)),
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.ButtonDisabledForeground)),
                IslandsStyles.When(VisualStateFlags.Selected, VisualStateFlags.Enabled,
                    IslandsStyles.Background(c => c.ToggleDisabledSelectedBackground)),
            ]);
}

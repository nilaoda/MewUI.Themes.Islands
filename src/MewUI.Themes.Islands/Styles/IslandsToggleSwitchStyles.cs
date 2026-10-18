using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands;

/// <summary>
/// The Islands switch: a compact 32 x 18 capsule with a 14 DIP thumb, drawn by
/// <see cref="IslandsSwitchIndicator"/> through the control's template.
/// </summary>
/// <remarks>
/// Islands itself has no switch control; this is the theme's own extension of its vocabulary. It stays
/// deliberately quieter than an iOS switch: the track is the component surface in both states, and the
/// accent is carried by the thumb and the selected track's tint rather than by a solid accent pill. A
/// focused switch carries the 2 DIP focus ring; a disabled one keeps the muted selected plate.
/// </remarks>
internal static class IslandsToggleSwitchStyles
{
    internal static void Register(StyleSheet sheet) =>
        sheet.Define<ToggleSwitch>(ToggleSwitchStyle());

    private static Style ToggleSwitchStyle() =>
        Style.DeriveFromDefault<ToggleSwitch>(
            [
                IslandsStyles.Background(c => c.ComponentBackground),
                IslandsStyles.BorderBrush(c => c.Border),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.From(ToggleSwitch.ThumbBrushProperty, c => c.SecondaryText),
                IslandsStyles.Fixed(Control.PaddingProperty, Thickness.Zero),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.CapsuleArc),
                IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
                IslandsStyles.Fixed(FrameworkElement.MinWidthProperty, IslandsSwitchIndicator.SwitchWidth),
                IslandsStyles.Fixed(FrameworkElement.MinHeightProperty, IslandsSwitchIndicator.SwitchHeight),
                IslandsStyles.Fixed(FrameworkElement.WidthProperty, IslandsSwitchIndicator.SwitchWidth),
                IslandsStyles.Fixed(FrameworkElement.HeightProperty, IslandsSwitchIndicator.SwitchHeight),
                IslandsStyles.Fixed(Control.TemplateProperty, (ControlTemplate?)IslandsToggleTemplates.SwitchFace),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot,
                    IslandsStyles.Overlay(c => c.ComponentBackground, c => c.HoverBackground),
                    IslandsStyles.BorderBrush(c => c.BorderFocused)),
                IslandsStyles.When(VisualStateFlags.Pressed,
                    IslandsStyles.Background(c => c.ButtonPressed),
                    IslandsStyles.BorderBrush(c => c.BorderFocused)),
                IslandsStyles.When(VisualStateFlags.Checked,
                    IslandsStyles.Background(c => c.Selection),
                    IslandsStyles.BorderBrush(c => c.Selection),
                    IslandsStyles.From(ToggleSwitch.ThumbBrushProperty, c => c.Accent)),
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Hot,
                    IslandsStyles.Overlay(c => c.Selection, c => c.HoverBackground),
                    IslandsStyles.BorderBrush(c => c.Selection),
                    IslandsStyles.From(ToggleSwitch.ThumbBrushProperty, c => c.Accent)),
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Pressed,
                    IslandsStyles.Background(c => c.Selection),
                    IslandsStyles.BorderBrush(c => c.Selection),
                    IslandsStyles.From(ToggleSwitch.ThumbBrushProperty, c => c.Accent)),
                IslandsStyles.When(VisualStateFlags.Focused,
                    IslandsStyles.BorderBrush(c => c.FocusRing),
                    IslandsStyles.Fixed(Control.BorderThicknessProperty, 2.0)),
                IslandsStyles.When(VisualStateFlags.Checked | VisualStateFlags.Focused,
                    IslandsStyles.BorderBrush(c => c.FocusRing),
                    IslandsStyles.Fixed(Control.BorderThicknessProperty, 2.0)),
                IslandsStyles.Disabled(
                    IslandsStyles.Background(c => c.ButtonDisabledBackground),
                    IslandsStyles.BorderBrush(c => c.BorderDisabled),
                    IslandsStyles.From(ToggleSwitch.ThumbBrushProperty, c => c.DisabledText)),
                IslandsStyles.When(
                    VisualStateFlags.Checked,
                    VisualStateFlags.Enabled,
                    IslandsStyles.Background(c => c.ToggleDisabledSelectedBackground),
                    IslandsStyles.BorderBrush(c => c.BorderDisabled),
                    IslandsStyles.From(ToggleSwitch.ThumbBrushProperty, c => c.DisabledText)),
                IslandsStyles.Invalid(),
            ]);
}

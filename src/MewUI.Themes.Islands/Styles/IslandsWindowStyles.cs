using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Theming;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// The surfaces and the range controls: window, group box, label, tool tip, scroll bar, slider, progress bar.
/// </summary>
internal static class IslandsWindowStyles
{
    internal static void Register(StyleSheet sheet)
    {
        sheet.Define<Window>(WindowStyle());
        sheet.Define<GroupBox>(GroupBoxStyle());
        sheet.Define<Label>(LabelStyle());
        sheet.Define<ToolTip>(ToolTipStyle());
        sheet.Define<ScrollBar>(ScrollBarStyle());
        sheet.Define<ScrollViewer>(ScrollViewerStyle());
        sheet.Define<Slider>(SliderStyle());
        sheet.Define<ProgressBar>(ProgressBarStyle());

        // A toolbar label is a Label the toolbar builds for itself, found by name rather than by type.
        sheet.Define(BuiltInStyles.ToolBarLabel, static () => ToolBarLabelStyle());
    }

    /// <summary>
    /// The window carries the colors every descendant inherits, so this is where Islands's panel surface and
    /// body text enter the tree.
    /// </summary>
    /// <remarks>
    /// The foreground uses the explicit theme text color rather than the palette seed. The light variant's
    /// seed text is <c>#3c3c3c</c> to balance derived borders and row labels, while its body text is
    /// <c>#000000</c>. The padding is MewUI's container inset, where Islands leaves a root panel
    /// flush against the frame and lets each panel carry its own gaps.
    /// </remarks>
    private static Style WindowStyle() =>
        Style.DeriveFromDefault<Window>(
            [
                IslandsStyles.Background(c => c.PanelBackground),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Metric(TextElement.FontFamilyProperty, t => t.Metrics.FontFamily),
                IslandsStyles.Metric(TextElement.FontSizeProperty, t => t.Metrics.FontSize),
                IslandsStyles.Metric(TextElement.FontWeightProperty, t => t.Metrics.FontWeight),
                IslandsStyles.Metric(Control.PaddingProperty, t => t.Metrics.ContainerPadding),
                IslandsStyles.Fixed(IslandsX11PopupDismissal.EnabledProperty, OperatingSystem.IsLinux()),
            ]);

    /// <summary>Islands's titled pane: the panel surface, an edge one step lighter than a component's, rounded like a component.</summary>
    private static Style GroupBoxStyle() =>
        Style.DeriveFromDefault<GroupBox>(
            [
                IslandsStyles.Background(c => c.PanelBackground),
                IslandsStyles.BorderBrush(c => c.Border),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.ComponentArc),
                IslandsStyles.Metric(Control.PaddingProperty, t => t.Metrics.ContainerPadding),
                IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
            ],
            [
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    private static Style LabelStyle() =>
        Style.DeriveFromDefault<Label>(
            [
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
            ],
            [
                // A label runs nothing, so it dims only with the panel it belongs to.
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    private static Style ToolBarLabelStyle() =>
        Style.DeriveFromDefault<Label>(
            [
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.PaddingProperty, Thickness.Zero),
                IslandsStyles.Fixed(FrameworkElement.MarginProperty, new Thickness(4, 0, 4, 0)),
                IslandsStyles.Fixed(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center),
            ],
            [
                IslandsStyles.Disabled(IslandsStyles.Foreground(c => c.DisabledText)),
            ]);

    /// <summary>
    /// A tooltip uses the feedback surface, body text and a 6 DIP corner.
    /// </summary>
    /// <remarks>
    /// The chip's corner is Islands's 6 rather than the radius the chrome blurs the drop shadow with; the
    /// two pixels of disagreement sit on a blurred edge and are recorded in <c>docs/color-map.md</c>.
    /// </remarks>
    private static Style ToolTipStyle() =>
        Style.DeriveFromDefault<ToolTip>(
            [
                IslandsStyles.Background(c => c.ToolTipBackground),
                IslandsStyles.BorderBrush(c => c.ToolTipBorder ?? c.MenuBorder),
                IslandsStyles.Foreground(c => c.ToolTipForeground),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.ToolTipArc),
                IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
                IslandsStyles.Metric(TextElement.FontFamilyProperty, t => t.Metrics.FontFamily),
                IslandsStyles.Metric(TextElement.FontSizeProperty, t => t.Metrics.FontSize),
                IslandsStyles.Metric(TextElement.FontWeightProperty, t => t.Metrics.FontWeight),
                IslandsStyles.Fixed(Control.PaddingProperty, new Thickness(8, 4, 8, 4)),
            ]);

    /// <remarks>
    /// A scroll bar's background <em>is</em> its thumb in MewUI, which is why the hover and pressed states
    /// recolor the fill rather than the edge. <c>ScrollBar.thumbArc = 999</c> is a capsule, clamped to the
    /// thumb's own thickness by the renderer.
    /// </remarks>
    private static Style ScrollBarStyle() =>
        Style.DeriveFromDefault<ScrollBar>(
            [
                IslandsStyles.Background(c => c.ScrollThumb),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.CapsuleArc),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot, IslandsStyles.Background(c => c.ScrollThumbHover)),
                IslandsStyles.When(VisualStateFlags.Pressed, IslandsStyles.Background(c => c.ScrollThumbPressed)),
                IslandsStyles.Disabled(IslandsStyles.Background(c => c.ScrollThumb.WithAlpha(128))),
            ]);

    /// <summary>
    /// A scroll pane is a viewport, not a surface: Islands gives a scrolled panel no fill and no edge of its
    /// own, because the panel or component behind it is what the user sees. The rule exists so that the
    /// viewport answers to this rather than to whatever its base type would resolve to.
    /// </summary>
    private static Style ScrollViewerStyle() =>
        Style.DeriveFromDefault<ScrollViewer>(
            [
                IslandsStyles.Background(c => c.PanelBackground.WithAlpha(0)),
                IslandsStyles.BorderBrush(c => c.Border.WithAlpha(0)),
                IslandsStyles.Foreground(c => c.Text),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, 0.0),
                IslandsStyles.Fixed(Control.PaddingProperty, Thickness.Zero),
            ]);

    /// <summary>
    /// The track and thumb use separate brushes. MewUI paints the value fill using <c>Palette.Accent</c>.
    /// </summary>
    private static Style SliderStyle() =>
        Style.DeriveFromDefault<Slider>(
            [
                IslandsStyles.Background(c => c.SliderTrack),
                IslandsStyles.BorderBrush(c => c.SliderTrack.WithAlpha(0)),
                // Slider uses the control's border thickness for the thumb stroke.
                IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
                IslandsStyles.Foreground(c => c.Text),
                SliderThumb(Slider.ThumbBrushProperty, c => c.SliderFill),
                SliderThumb(Slider.ThumbBorderBrushProperty, c => c.SliderThumbBorder ?? c.Border),
            ],
            [
                IslandsStyles.When(VisualStateFlags.Hot,
                    SliderThumb(Slider.ThumbBorderBrushProperty, c => c.BorderFocused)),
                IslandsStyles.When(VisualStateFlags.Focused,
                    SliderThumb(Slider.ThumbBorderBrushProperty, c => c.CheckBorderActive)),
                IslandsStyles.When(VisualStateFlags.Pressed,
                    SliderThumb(Slider.ThumbBrushProperty, c => c.SliderFillHover),
                    SliderThumb(Slider.ThumbBorderBrushProperty, c => c.BorderFocused)),
                IslandsStyles.Disabled(
                    SliderThumb(Slider.ThumbBrushProperty, c => c.ButtonDisabledBackground),
                    SliderThumb(Slider.ThumbBorderBrushProperty, c => c.BorderDisabled),
                    IslandsStyles.Foreground(c => c.DisabledText)),
                IslandsStyles.Invalid(),
            ]);

    private static Setter SliderThumb<T>(MewProperty<T> property, Func<IslandsVariantColors, T> select) =>
        IslandsStyles.From(property, select);

    /// <remarks>
    /// Uses the 4 DIP stripe from DarculaProgressBarUI. MewUI paints the fill using <c>Palette.Accent</c>.
    /// </remarks>
    private static Style ProgressBarStyle() =>
        Style.DeriveFromDefault<ProgressBar>(
            [
                IslandsStyles.Background(c => c.ProgressTrack),
                IslandsStyles.Foreground(c => c.ProgressText),
                IslandsStyles.Fixed(FrameworkElement.HeightProperty, IslandsMetrics.ProgressBarThickness),
                IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
                IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.SmallArc),
            ]);
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands;

/// <summary>
/// The faces of a Islands check box and radio button: the indicator, then the gap, then the label.
/// </summary>
/// <remarks>
/// MewUI draws both controls itself, so the only way for a theme to reach the shape of the box, the
/// stroke of the mark and the ring of the radio is to replace the face with a template — see
/// <see cref="IslandsToggleIndicator"/>, which draws it. The label stays a projected
/// <see cref="ContentPresenter"/>, so content and padding behave exactly as they do without a template.
/// </remarks>
internal static class IslandsToggleTemplates
{
    private static DelegateControlTemplate<CheckBox>? _checkBox;
    private static DelegateControlTemplate<RadioButton>? _radioButton;
    private static DelegateControlTemplate<ToggleSwitch>? _switch;

    /// <summary>The check box face, shared by every check box the style applies it to.</summary>
    internal static DelegateControlTemplate<CheckBox> CheckBoxFace =>
        _checkBox ??= new DelegateControlTemplate<CheckBox>(BuildCheckBox);

    /// <summary>The radio button face, shared by every radio the style applies it to.</summary>
    internal static DelegateControlTemplate<RadioButton> RadioButtonFace =>
        _radioButton ??= new DelegateControlTemplate<RadioButton>(BuildRadioButton);

    /// <summary>The switch face, shared by every switch the style applies it to.</summary>
    internal static DelegateControlTemplate<ToggleSwitch> SwitchFace =>
        _switch ??= new DelegateControlTemplate<ToggleSwitch>(BuildSwitch);

    private static Element BuildCheckBox(CheckBox owner, ControlTemplateContext ctx)
    {
        IslandsToggleIndicator indicator = NewIndicator(IslandsToggleShape.CheckBox);
        ctx.Bind(indicator, IslandsToggleIndicator.CheckedProperty, CheckBox.IsCheckedProperty);
        return Face(ctx, indicator);
    }

    private static Element BuildRadioButton(RadioButton owner, ControlTemplateContext ctx)
    {
        IslandsToggleIndicator indicator = NewIndicator(IslandsToggleShape.Radio);
        ctx.Bind(indicator, IslandsToggleIndicator.SelectedProperty, ToggleBase.IsCheckedProperty);
        return Face(ctx, indicator);
    }

    private static Element BuildSwitch(ToggleSwitch owner, ControlTemplateContext ctx)
    {
        var indicator = new IslandsSwitchIndicator();
        ctx.Bind(indicator, IslandsSwitchIndicator.CheckedProperty, ToggleBase.IsCheckedProperty);
        ctx.Bind(indicator, IslandsSwitchIndicator.FaceProperty, Control.BackgroundProperty);
        ctx.Bind(indicator, IslandsSwitchIndicator.EdgeProperty, Control.BorderBrushProperty);
        ctx.Bind(indicator, IslandsSwitchIndicator.EdgeThicknessProperty, Control.BorderThicknessProperty);
        ctx.Bind(indicator, IslandsSwitchIndicator.ThumbProperty, ToggleSwitch.ThumbBrushProperty);
        return indicator;
    }

    private static IslandsToggleIndicator NewIndicator(IslandsToggleShape shape)
    {
        var indicator = new IslandsToggleIndicator { Shape = shape };
        return indicator.CenterVertical().Column(0);
    }

    private static Element Face(ControlTemplateContext ctx, IslandsToggleIndicator indicator)
    {
        // The plate and the edge follow the control, which is what keeps the style's triggers in charge
        // of hover, focus, the checked plate and the disabled one.
        ctx.Bind(indicator, IslandsToggleIndicator.FaceProperty, Control.BackgroundProperty);
        ctx.Bind(indicator, IslandsToggleIndicator.EdgeProperty, Control.BorderBrushProperty);
        // The edge's width follows the control too, so a focused box can carry Islands's 2 DIP accent ring.
        ctx.Bind(indicator, IslandsToggleIndicator.EdgeThicknessProperty, Control.BorderThicknessProperty);

        var content = new ContentPresenter().CenterVertical().Column(2);

        var grid = new Grid()
            .Columns(
                GridLength.Pixels(IslandsToggleIndicator.Size),
                GridLength.Pixels(IslandsToggleIndicator.Spacing),
                GridLength.Star)
            .Children(indicator, content);

        // Padding insets the whole face, as it does without a template.
        var padded = new Border { Child = grid };
        ctx.Bind(padded, Control.PaddingProperty);

        return padded;
    }
}

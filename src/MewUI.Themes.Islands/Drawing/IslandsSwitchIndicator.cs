using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace MewUI.Themes.Islands.Drawing;

/// <summary>
/// The Islands switch: a capsule track with a circular thumb at one end.
/// </summary>
/// <remarks>
/// MewUI's default switch is not part of the Islands vocabulary, so the theme draws its own face. The track
/// and the thumb follow the control's <c>Background</c>, <c>BorderBrush</c> and <c>ThumbBrush</c>, which keeps
/// the style's state triggers the single source of truth for hover, focus, the checked plate and the disabled
/// one. Only the thumb's travel is read from the control state here.
/// </remarks>
internal sealed class IslandsSwitchIndicator : FrameworkElement
{
    /// <summary>The capsule's width.</summary>
    internal const double SwitchWidth = 38;

    /// <summary>The capsule's height.</summary>
    internal const double SwitchHeight = 18;

    private const double ThumbDiameter = 14;
    private const double Inset = 2;

    private static readonly MewProperty<bool> _checkedProperty =
        MewProperty<bool>.Register<IslandsSwitchIndicator>(
            nameof(Checked), false, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<Color> _faceProperty =
        MewProperty<Color>.Register<IslandsSwitchIndicator>(
            nameof(Face), Color.Transparent, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<Color> _edgeProperty =
        MewProperty<Color>.Register<IslandsSwitchIndicator>(
            nameof(Edge), Color.Transparent, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<Color> _thumbProperty =
        MewProperty<Color>.Register<IslandsSwitchIndicator>(
            nameof(Thumb), Color.Transparent, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<double> _edgeThicknessProperty =
        MewProperty<double>.Register<IslandsSwitchIndicator>(
            nameof(EdgeThickness), 1.0, MewPropertyOptions.AffectsRender);

    /// <summary>Whether the thumb sits at the trailing end.</summary>
    internal bool Checked
    {
        get => GetValue(_checkedProperty);
        set => SetValue(_checkedProperty, value);
    }

    /// <summary>The capsule's fill, bound from the control's background.</summary>
    internal Color Face => GetValue(_faceProperty);

    /// <summary>The capsule's edge, bound from the control's border brush.</summary>
    internal Color Edge => GetValue(_edgeProperty);

    /// <summary>The thumb's fill, bound from the control's thumb brush.</summary>
    internal Color Thumb => GetValue(_thumbProperty);

    /// <summary>The capsule's edge width, bound from the control's border thickness.</summary>
    internal double EdgeThickness => GetValue(_edgeThicknessProperty);

    internal static MewProperty<bool> CheckedProperty => _checkedProperty;

    internal static MewProperty<Color> FaceProperty => _faceProperty;

    internal static MewProperty<Color> EdgeProperty => _edgeProperty;

    internal static MewProperty<Color> ThumbProperty => _thumbProperty;

    internal static MewProperty<double> EdgeThicknessProperty => _edgeThicknessProperty;

    protected override Size MeasureContent(Size availableSize) => new(SwitchWidth, SwitchHeight);

    protected override void OnRender(IGraphicsContext context)
    {
        Rect track = Bounds;
        double radius = track.Height / 2;

        context.FillRoundedRectangle(track, radius, radius, Face);

        if (EdgeThickness > 0)
        {
            context.DrawRoundedRectangle(track, radius, radius, Edge, EdgeThickness, strokeInset: true);
        }

        double x = Checked
            ? track.Right - Inset - ThumbDiameter
            : track.X + Inset;
        double y = track.Y + ((track.Height - ThumbDiameter) / 2);
        var thumb = new Rect(x, y, ThumbDiameter, ThumbDiameter);

        context.FillEllipse(thumb, Thumb);
    }
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace MewUI.Themes.Islands;

/// <summary>Which of the two Islands check faces this indicator draws.</summary>
internal enum IslandsToggleShape
{
    /// <summary>A square box, marked with Islands's check mark when checked.</summary>
    CheckBox,

    /// <summary>A circle, marked with a centered dot when selected.</summary>
    Radio,
}

/// <summary>
/// The face of a Islands check box or radio button: the 14 DIP box (or ring) with the mark Islands draws
/// inside it, stroked at the width and with the path Islands uses.
/// </summary>
/// <remarks>
/// <para>
/// MewUI paints these controls itself and takes the mark's color from <c>Palette.Accent</c>, so no style
/// property reaches either the box's corner or the mark — which is why a checked check box in this theme
/// used to read as an accent square rather than as Islands's mark on the plate. The theme replaces the
/// whole face with this element through the control's template.
/// </para>
/// <para>
/// The plate and the edge are bound from the control's <c>Background</c> and <c>BorderBrush</c>, so the
/// style's state triggers stay the single source of truth for hover, focus, the checked plate and the
/// disabled one. Only the mark is read from the palette here, because the control's <c>Foreground</c>
/// belongs to the label: Islands keeps the label at the window text color in every state.
/// </para>
/// </remarks>
internal sealed class IslandsToggleIndicator : FrameworkElement
{
    /// <summary>The indicator's edge length, and Islands's square as well.</summary>
    internal const double Size = 14;

    /// <summary><c>RadioButton/iconTextGap</c>: the gap between the icon and its label.</summary>
    internal const double Spacing = 4;

    /// <summary>Islands's <c>CheckBox.arc</c>: the corner of the box is half of it.</summary>
    private const double BoxArc = 4;

    /// <summary>Islands's <c>RadioButton.icon.centerDiameter</c> on both variants this theme ports.</summary>
    private const double DotDiameter = 5;

    /// <summary>Islands strokes the check mark at 1.9 and rounds both its ends and its elbow.</summary>
    private const double MarkThickness = 2;

    /// <summary>
    /// The edge's width. It is a property rather than a constant because a focused box draws Islands's 2 DIP
    /// accent ring: the style thickens the control's border on focus and this follows it.
    /// </summary>
    private static readonly MewProperty<double> _edgeThicknessProperty =
        MewProperty<double>.Register<IslandsToggleIndicator>(
            "EdgeThickness", 1.0, MewPropertyOptions.AffectsRender | MewPropertyOptions.AffectsLayout);

    private static readonly MewProperty<IslandsToggleShape> _shapeProperty =
        MewProperty<IslandsToggleShape>.Register<IslandsToggleIndicator>(
            nameof(Shape), IslandsToggleShape.CheckBox, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<Color> _faceProperty =
        MewProperty<Color>.Register<IslandsToggleIndicator>(
            nameof(Face), Color.Transparent, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<Color> _edgeProperty =
        MewProperty<Color>.Register<IslandsToggleIndicator>(
            nameof(Edge), Color.Transparent, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<bool?> _checkedProperty =
        MewProperty<bool?>.Register<IslandsToggleIndicator>(
            nameof(Checked), false, MewPropertyOptions.AffectsRender);

    private static readonly MewProperty<bool> _selectedProperty =
        MewProperty<bool>.Register<IslandsToggleIndicator>(
            nameof(Selected), false, MewPropertyOptions.AffectsRender);

    [ThreadStatic]
    private static PathGeometry? _cachedPath;

    /// <summary>The square or the ring to draw.</summary>
    internal IslandsToggleShape Shape
    {
        get => GetValue(_shapeProperty);
        set => SetValue(_shapeProperty, value);
    }

    /// <summary>The plate inside the edge, which the control's background carries.</summary>
    internal Color Face => GetValue(_faceProperty);

    /// <summary>The edge color, which the control's border brush carries.</summary>
    internal Color Edge => GetValue(_edgeProperty);

    /// <summary>The check box's state, including the indeterminate one.</summary>
    internal bool? Checked => GetValue(_checkedProperty);

    /// <summary>The radio's state.</summary>
    internal bool Selected => GetValue(_selectedProperty);

    /// <summary>The property carrying the shape, so a template can set it as it builds.</summary>
    internal static MewProperty<IslandsToggleShape> ShapeProperty => _shapeProperty;

    /// <summary>The property carrying the plate, bound from the control's background.</summary>
    internal static MewProperty<Color> FaceProperty => _faceProperty;

    /// <summary>The property carrying the edge, bound from the control's border brush.</summary>
    internal static MewProperty<Color> EdgeProperty => _edgeProperty;

    /// <summary>The property carrying the check box state, bound from the control.</summary>
    internal static MewProperty<bool?> CheckedProperty => _checkedProperty;

    /// <summary>The property carrying the radio state, bound from the control.</summary>
    internal static MewProperty<bool> SelectedProperty => _selectedProperty;

    /// <summary>The property carrying the edge width, bound from the control's border thickness.</summary>
    internal static MewProperty<double> EdgeThicknessProperty => _edgeThicknessProperty;

    protected override Size MeasureContent(Size availableSize) => new(Size, Size);

    protected override void OnRender(IGraphicsContext context)
    {
        if (Shape == IslandsToggleShape.Radio)
        {
            RenderRadio(context);
            return;
        }

        RenderCheckBox(context);
    }

    private void RenderCheckBox(IGraphicsContext context)
    {
        Rect box = Bounds;
        double radius = BoxArc * 0.5;

        context.FillRoundedRectangle(box, radius, radius, Face);
        context.DrawRoundedRectangle(box, radius, radius, Edge, GetValue(_edgeThicknessProperty), strokeInset: true);

        Color mark = MarkColor;
        if (Checked == true)
        {
            DrawCheckMark(context, box, mark);
        }
        else if (Checked == null)
        {
            DrawIndeterminateMark(context, box, mark);
        }
    }

    private void RenderRadio(IGraphicsContext context)
    {
        Rect ring = Bounds;

        context.FillEllipse(ring, Face);
        context.DrawEllipse(ring, Edge, GetValue(_edgeThicknessProperty), strokeInset: true);

        // Islands centers the dot on the icon, so its insets are equal on every side.
        if (Selected)
        {
            double inset = (ring.Width - DotDiameter) / 2;
            context.FillEllipse(ring.Deflate(new Thickness(inset)), MarkColor);
        }
    }

    /// <summary>
    /// The mark's color comes from the palette rather than the control, because the control's foreground
    /// carries the label's color: Islands leaves the label alone and lights the mark instead.
    /// </summary>
    private Color MarkColor
    {
        get
        {
            IslandsVariantColors colors = IslandsTheme.Colors(Theme);
            return IsEffectivelyEnabled ? colors.CheckFilledForeground : colors.CheckDisabledMark;
        }
    }

    /// <summary>
    /// Islands's check mark, in the 14 DIP square: from (3.5, 7.5) down to the elbow at (5.6, 10) and up
    /// to (10.25, 3.5), stroked with round ends. MewUI's own glyph is a wider, steeper stroke.
    /// </summary>
    private static void DrawCheckMark(IGraphicsContext context, Rect box, Color color)
    {
        PathGeometry path = _cachedPath ??= new PathGeometry();
        path.Reset();
        path.MoveTo(box.X + 3.5, box.Y + 7.5);
        path.LineTo(box.X + 5.6, box.Y + 10);
        path.LineTo(box.X + 10.25, box.Y + 3.5);
        context.DrawPath(path, color, MarkThickness);
    }

    /// <summary>Islands's indeterminate mark: a rounded bar 8.5 wide and 2.5 tall, centered in the box.</summary>
    private static void DrawIndeterminateMark(IGraphicsContext context, Rect box, Color color)
    {
        var bar = new Rect(box.X + 3.75, box.Y + 5.75, 8.5, 2.5);
        context.FillRoundedRectangle(bar, 1.25, 1.25, color);
    }
}

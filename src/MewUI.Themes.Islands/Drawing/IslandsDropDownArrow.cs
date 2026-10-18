using System.Runtime.CompilerServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Drawing;

/// <summary>
/// Islands's drop-down arrow: a solid triangle in the dimmed arrow color, centered in the column the
/// framework reserves for it.
/// </summary>
/// <remarks>
/// <para>
/// MewUI draws that arrow itself — a stroked chevron, in the control's own foreground — inside a private
/// method of <c>DropDownBase</c>, so neither its shape nor its color is reachable from a style, and
/// <c>ComboBox</c> exposes no template part for its value that a template could project. The arrow is
/// therefore overdrawn by an adorner: it repaints the reserved column in the control's background, which
/// is what removes the chevron, and then fills the triangle Islands draws there. Both themes this package
/// ports set <c>Component.arrowType = triangle</c>, so the triangle is always the arrow to draw.
/// </para>
/// <para>
/// Applying it twice is a no-op, and the adorner follows the control's own background, corner and border
/// thickness, so a hovered, focused or disabled drop-down needs nothing extra.
/// </para>
/// </remarks>
public static class IslandsDropDownArrow
{
    /// <summary>
    /// The width of the column <c>DropDownBase</c> reserves for the arrow, which Islands states as
    /// <c>Component/arrowAreaWidth = 28</c>. MewUI reserves 22, so the theme overdrawns the wider column.
    /// </summary>
    private const double ArrowAreaWidth = 28;

    /// <summary>
    /// Islands' arrow is a chevron: <c>/expui/general/chevronDown.svg</c> strokes
    /// <c>M11.5 6.25 L8 9.75 L4.5 6.25</c> in a 16 DIP grid — 7 wide, 3.5 tall, one DIP thick.
    /// </summary>
    private const double ArrowWidth = 7;

    private const double ArrowHeight = 3.5;

    private const double ArrowThickness = 1;

    private static readonly ConditionalWeakTable<Control, Arrow> _arrows = [];

    /// <summary>Draws Islands's triangle arrow in place of the framework's chevron.</summary>
    public static Control Apply(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (!_arrows.TryGetValue(target, out var arrow))
        {
            arrow = new Arrow(target);
            _arrows.Add(target, arrow);
        }

        arrow.Attach();
        return target;
    }

    /// <summary>Removes the triangle, leaving the framework's own chevron.</summary>
    public static Control Remove(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (_arrows.TryGetValue(target, out var arrow))
        {
            arrow.Detach();
            _arrows.Remove(target);
        }

        return target;
    }

    /// <summary>Whether the triangle is currently drawn for this control.</summary>
    public static bool IsApplied(Control target) => _arrows.TryGetValue(target, out _);

    private sealed class Arrow(Control target)
    {
        private readonly Control _target = target;
        private ArrowAdorner? _adorner;

        internal void Attach()
        {
            _target.SizeChanged += OnChanged;
            // The column is repainted in the control's own background, so every state that moves that
            // background has to repaint it: press and release, the edge lighting on hover, and the open
            // state that follows a press. A control's visual-state change is not observable from outside,
            // so the events that drive those states are what this subscribes to.
            _target.MouseEnter += OnInvalidated;
            _target.MouseLeave += OnInvalidated;
            _target.MouseDown += OnMouseInvalidated;
            _target.MouseUp += OnMouseInvalidated;
            _target.GotFocus += OnInvalidated;
            _target.LostFocus += OnInvalidated;

            Adorn();
        }

        internal void Detach()
        {
            _target.SizeChanged -= OnChanged;
            _target.MouseEnter -= OnInvalidated;
            _target.MouseLeave -= OnInvalidated;
            _target.MouseDown -= OnMouseInvalidated;
            _target.MouseUp -= OnMouseInvalidated;
            _target.GotFocus -= OnInvalidated;
            _target.LostFocus -= OnInvalidated;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(_target)?.Remove(_adorner);
                _adorner = null;
            }
        }

        private void OnChanged(SizeChangedEventArgs _) => Adorn();

        private void OnInvalidated() => _adorner?.InvalidateVisual();

        private void OnMouseInvalidated(MouseEventArgs _) => _adorner?.InvalidateVisual();

        /// <summary>
        /// The adorner layer only exists once the control sits inside a window, which is later than
        /// <c>Apply</c> is called, so the first layout pass that sizes the control retries until the
        /// adorner is in place.
        /// </summary>
        private void Adorn()
        {
            if (_adorner != null)
            {
                _adorner.InvalidateVisual();
                return;
            }

            AdornerLayer? layer = AdornerLayer.GetAdornerLayer(_target);
            if (layer == null)
            {
                return;
            }

            _adorner = new ArrowAdorner(_target);
            layer.Add(_adorner);
        }
    }

    private sealed class ArrowAdorner(Control target) : Adorner(target)
    {
        private readonly Control _target = target;

        [ThreadStatic]
        private static PathGeometry? _cachedPath;

        protected override void OnRender(IGraphicsContext context)
        {
            Rect bounds = _target.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            double border = _target.BorderThickness;
            double radius = Math.Max(0, _target.CornerRadius - border);
            Rect inner = bounds.Deflate(new Thickness(border));
            var column = new Rect(
                Math.Max(inner.X, inner.Right - ArrowAreaWidth),
                inner.Y,
                Math.Min(ArrowAreaWidth, inner.Width),
                inner.Height);

            IslandsVariantColors colors = IslandsTheme.Colors(Theme);
            Color arrowColor = _target.IsEffectivelyEnabled ? colors.Arrow : colors.ArrowDisabled;

            // Repainting the column in the control's own background is what erases the chevron: the
            // column is reserved for the arrow, so nothing else is drawn under it.
            context.Save();
            context.SetClipRoundedRect(inner, radius, radius);
            context.IntersectClip(column);
            context.FillRoundedRectangle(inner, radius, radius, _target.Background);
            context.Restore();

            double half = ArrowWidth / 2;
            double insetX = column.X + (column.Width / 2);
            double insetY = column.Y + (column.Height / 2);

            PathGeometry path = _cachedPath ??= new PathGeometry();
            path.Reset();
            path.MoveTo(insetX - half, insetY - (ArrowHeight / 2));
            path.LineTo(insetX, insetY + (ArrowHeight / 2));
            path.LineTo(insetX + half, insetY - (ArrowHeight / 2));
            context.DrawPath(path, arrowColor, ArrowThickness);
        }
    }
}

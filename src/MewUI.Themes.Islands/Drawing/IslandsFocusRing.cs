using System.Runtime.CompilerServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Drawing;

/// <summary>
/// Islands's outer focus ring: the 2 DIP halo that <c>Component.focusWidth</c> switches on around a focused
/// component, one pixel away from the edge the component's own style lights.
/// </summary>
/// <remarks>
/// MewUI gives a focused control a single accent-colored border, so the halo has no style property to live
/// in. It is drawn by an adorner, which the window lays out over the component's own rectangle on every pass,
/// and which exists only while the control holds the focus.
/// </remarks>
public static class IslandsFocusRing
{
    private static readonly ConditionalWeakTable<Control, Ring> _rings = [];

    /// <summary>Draws the Islands focus halo around this control whenever it holds the keyboard focus.</summary>
    public static Control Apply(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (!_rings.TryGetValue(target, out var ring))
        {
            ring = new Ring(target);
            _rings.Add(target, ring);
        }

        ring.Attach();
        return target;
    }

    /// <summary>Removes the halo, leaving the focused edge the control's own style draws.</summary>
    public static Control Remove(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (_rings.TryGetValue(target, out var ring))
        {
            ring.Detach();
            _rings.Remove(target);
        }

        return target;
    }

    /// <summary>Whether the halo is currently drawn for this control.</summary>
    public static bool IsApplied(Control target) => _rings.TryGetValue(target, out _);

    private sealed class Ring(Control target)
    {
        private readonly Control _target = target;
        private RingAdorner? _adorner;
        private bool _attached;

        internal void Attach()
        {
            if (_attached)
            {
                return;
            }

            _attached = true;
            _target.GotFocus += OnGotFocus;
            _target.LostFocus += OnLostFocus;

            // A control that already holds the focus when the ring is applied has seen its GotFocus event
            // pass, so it is lit here rather than never.
            if (_target.IsFocused)
            {
                Show();
            }
        }

        internal void Detach()
        {
            _attached = false;
            _target.GotFocus -= OnGotFocus;
            _target.LostFocus -= OnLostFocus;
            Hide();
        }

        private void OnGotFocus() => Show();

        private void OnLostFocus() => Hide();

        private void Show()
        {
            if (_adorner != null)
            {
                _adorner.InvalidateVisual();
                return;
            }

            // The layer only exists once the control is inside a window, which is later than Apply() when
            // the theme is installed against a window that has not been shown yet.
            AdornerLayer? layer = AdornerLayer.GetAdornerLayer(_target);
            if (layer == null)
            {
                return;
            }

            _adorner = new RingAdorner(_target);
            layer.Add(_adorner);
        }

        private void Hide()
        {
            if (_adorner == null)
            {
                return;
            }

            AdornerLayer.GetAdornerLayer(_target)?.Remove(_adorner);
            _adorner = null;
        }
    }

    private sealed class RingAdorner(Control target) : Adorner(target)
    {
        private readonly Control _target = target;

        protected override void OnRender(IGraphicsContext context)
        {
            // Islands draws the ring only while the window itself is the active one.
            if (_target.FindVisualRoot() is Window window && !window.IsActive)
            {
                return;
            }

            double thickness = IslandsMetrics.FocusRingThickness;

            // The ring sits outside the component's own edge, with the border's width of air between them,
            // and follows the corner the component was rounded to.
            double gap = _target.BorderThickness + 1.0;
            Rect rect = _target.Bounds.Inflate(gap + thickness / 2);
            double radius = _target.CornerRadius + gap + thickness / 2;
            context.DrawRoundedRectangle(rect, radius, radius, IslandsTheme.Colors(Theme).FocusRing, thickness);
        }
    }
}

using System.Runtime.CompilerServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using MewUI.Themes.Islands.Styles;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Drawing;

/// <summary>
/// Dresses a tab strip the way Islands does: every tab is a chip, and the strip is closed by the one pixel
/// line that separates it from the page beneath.
/// </summary>
/// <remarks>
/// <para>
/// MewUI's <c>TabHeaderButton</c> is internal, so each header's chip is named onto it from here
/// (<see cref="IslandsStyleNames.TabHeader"/>, see <c>IslandsTabStyles</c>), while the line under the strip is
/// drawn from that same handle. Which of the two chip styles a header wears turns on whether the window holds
/// the focus, and that is a fact about the window rather than about the header: a style cannot read it, so
/// this is the one place that can tell them apart.
/// </para>
/// <para>
/// An earlier revision claimed a tab with a bar under its label — the older New UI's <c>TabbedPane</c>
/// treatment. The two are different shapes and not two colours of one shape, which is why the bar is gone
/// rather than recoloured.
/// </para>
/// <para>
/// Applying it twice is a no-op. The strip is read from the visual tree on every pass rather than cached,
/// so adding, removing or reordering tabs needs no notification.
/// </para>
/// </remarks>
public static class IslandsTabPlate
{
    private static readonly ConditionalWeakTable<TabControl, Drawer> _drawers = [];

    /// <summary>Draws Islands's tab plates for these tabs, in place of the plate their style would paint.</summary>
    public static TabControl Apply(TabControl tabs)
    {
        ArgumentNullException.ThrowIfNull(tabs);

        if (IsApplied(tabs))
        {
            return tabs;
        }

        // The drawer holds the event subscriptions, so keeping it referenced is what keeps the wiring alive.
        var drawer = new Drawer(tabs);
        _drawers.Add(tabs, drawer);
        drawer.Attach();
        return tabs;
    }

    /// <summary>Removes the drawer and lets the tabs fall back to the plate their style paints.</summary>
    public static TabControl Remove(TabControl tabs)
    {
        ArgumentNullException.ThrowIfNull(tabs);

        if (_drawers.TryGetValue(tabs, out var drawer))
        {
            _drawers.Remove(tabs);
            drawer.Detach();
        }

        return tabs;
    }

    /// <summary>Whether the Islands tab plates are already drawn for these tabs.</summary>
    public static bool IsApplied(TabControl tabs) => _drawers.TryGetValue(tabs, out _);

    /// <summary>Whether this element is one of the tabs' header buttons.</summary>
    /// <remarks>
    /// The header type is internal, so it cannot be named, and being a content control parented straight to
    /// the tabs does not identify one either: the selected tab's page and the overflow chevron are parented
    /// exactly the same way, and naming a header's hover plate onto the page tints the whole pane. A header
    /// is recognized instead by what it presents — the tab's own header element, the same instance.
    /// </remarks>
    internal static bool IsHeader(TabControl tabs, ContentControl control) =>
        ReferenceEquals(control.Parent, tabs)
        && control.Content != null
        && tabs.Tabs.Any(tab => ReferenceEquals(tab.Header, control.Content));

    private sealed class Drawer(TabControl tabs)
    {
        private PlateAdorner? _adorner;
        private Window? _window;

        internal void Attach()
        {
            tabs.SelectionChanged += OnChanged;
            tabs.SizeChanged += OnChanged;
            Adorn();
        }

        internal void Detach()
        {
            tabs.SelectionChanged -= OnChanged;
            tabs.SizeChanged -= OnChanged;
            WatchWindow(null);

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(tabs)?.Remove(_adorner);
                _adorner = null;
            }
        }

        private void OnChanged(object? _) => Adorn();

        private void OnChanged(SizeChangedEventArgs _) => Adorn();

        /// <summary>
        /// Keeps the activation subscription on whichever window the tabs currently sit in.
        /// </summary>
        /// <remarks>
        /// The focus can move between windows — a dialog opens over the one holding the strip — so the
        /// subscription follows rather than being taken once.
        /// </remarks>
        private void WatchWindow(Window? window)
        {
            if (ReferenceEquals(_window, window))
            {
                return;
            }

            if (_window != null)
            {
                _window.Activated -= OnActivationChanged;
                _window.Deactivated -= OnActivationChanged;
            }

            _window = window;

            if (_window != null)
            {
                _window.Activated += OnActivationChanged;
                _window.Deactivated += OnActivationChanged;
            }
        }

        private void OnActivationChanged() => Dress();

        /// <summary>
        /// The adorner layer only exists once the tabs sit inside a window, which happens later than
        /// <c>Apply</c> is called, so every layout pass retries until the adorner is in place.
        /// </summary>
        private void Adorn()
        {
            WatchWindow(tabs.FindVisualRoot() as Window);

            if (_adorner != null)
            {
                _adorner.InvalidateVisual();
                return;
            }

            AdornerLayer? layer = AdornerLayer.GetAdornerLayer(tabs);
            if (layer == null)
            {
                return;
            }

            _adorner = new PlateAdorner(tabs, Dress);
            layer.Add(_adorner);
        }

        /// <summary>
        /// Hands every header the chip its window's activation calls for.
        /// </summary>
        /// <remarks>
        /// Reached by naming a style onto each header, because the header type is internal: a type rule on the
        /// public base it resolves through would also give every scroll viewer and user control in the
        /// application a tab's plate. Assigning a style name re-resolves the style, so it is guarded. Called
        /// from the activation handlers as well as from the adorner's arrange, because the focus is not a
        /// layout change and a header would otherwise keep the other variant's chip until something else moved.
        /// </remarks>
        private void Dress()
        {
            string name = (_window?.IsActive ?? true)
                ? IslandsStyleNames.TabHeader
                : IslandsStyleNames.TabHeaderInactive;

            foreach (ContentControl header in Headers())
            {
                if (header.StyleName != name)
                {
                    header.StyleName = name;
                }
            }

            _adorner?.InvalidateVisual();
        }

        /// <summary>The header strip, in the order the tabs lay it out.</summary>
        private List<ContentControl> Headers()
        {
            var headers = new List<ContentControl>();
            VisualTree.Visit(tabs, child =>
            {
                if (child is ContentControl control && IsHeader(tabs, control))
                {
                    headers.Add(control);
                }
            });

            bool vertical = tabs.TabPlacement is TabPlacement.Left or TabPlacement.Right;
            headers.Sort(vertical
                ? static (a, b) => a.Bounds.Y.CompareTo(b.Bounds.Y)
                : static (a, b) => a.Bounds.X.CompareTo(b.Bounds.X));
            return headers;
        }

        private sealed class PlateAdorner(TabControl tabs, Action dress) : Adorner(tabs)
        {
            private readonly TabControl _tabs = tabs;
            private readonly Action _dress = dress;

            protected override void ArrangeContent(Rect bounds)
            {
                base.ArrangeContent(bounds);
                _dress();
            }

            protected override void OnRender(IGraphicsContext context)
            {
                var colors = IslandsTheme.Colors(Theme);
                List<ContentControl> headers = Headers();
                if (headers.Count == 0)
                {
                    return;
                }

                bool vertical = _tabs.TabPlacement is TabPlacement.Left or TabPlacement.Right;
                Rect strip = StripRect(headers);
                Rect control = _tabs.Bounds;
                // The page always sits on the far side of the strip from the edge the strip is flush with, so
                // the closing line belongs to the strip's inner edge.
                bool tabsOnRight = vertical && strip.X + strip.Width / 2 > control.X + control.Width / 2;
                bool tabsBelow = !vertical && strip.Y + strip.Height / 2 > control.Y + control.Height / 2;

                double separator = IslandsMetrics.TabContentSeparatorThickness;
                Rect separatorLine = vertical
                    ? new Rect(
                        tabsOnRight ? control.X : control.Right - separator,
                        control.Y,
                        separator,
                        control.Height)
                    : new Rect(
                        control.X,
                        tabsBelow ? strip.Y - separator : strip.Bottom,
                        control.Width,
                        separator);
                ContentControl? selected = SelectedHeader(headers);
                if (selected == null)
                {
                    context.FillRectangle(separatorLine, colors.TabContentSeparator);
                    return;
                }

                // Islands leaves the line open where the selected tab meets the page, so the chip reads as
                // part of the page rather than as a plate standing in front of it.
                foreach (Rect piece in SeparatorPieces(separatorLine, selected.Bounds, vertical))
                {
                    if (piece.Width > 0 && piece.Height > 0)
                    {
                        context.FillRectangle(piece, colors.TabContentSeparator);
                    }
                }
            }

            /// <summary>The content line, cut where the selected tab crosses it.</summary>
            private static IEnumerable<Rect> SeparatorPieces(Rect line, Rect header, bool vertical)
            {
                if (vertical)
                {
                    yield return new Rect(line.X, line.Y, line.Width, header.Y - line.Y);
                    yield return new Rect(line.X, header.Bottom, line.Width, line.Bottom - header.Bottom);
                }
                else
                {
                    yield return new Rect(line.X, line.Y, header.X - line.X, line.Height);
                    yield return new Rect(header.Right, line.Y, line.Right - header.Right, line.Height);
                }
            }

            /// <summary>
            /// The header of the selected tab, matched through the header element it presents rather than by
            /// position: an overflowing strip realizes only some headers, so position would name the wrong one.
            /// </summary>
            private ContentControl? SelectedHeader(List<ContentControl> headers)
            {
                Element? content = _tabs.SelectedTab?.Header;
                if (content == null)
                {
                    return null;
                }

                foreach (ContentControl header in headers)
                {
                    if (ReferenceEquals(header.Content, content))
                    {
                        return header;
                    }
                }

                return null;
            }

            private List<ContentControl> Headers()
            {
                var headers = new List<ContentControl>();
                VisualTree.Visit(_tabs, child =>
                {
                    if (child is ContentControl control && IsHeader(_tabs, control))
                    {
                        headers.Add(control);
                    }
                });

                bool vertical = _tabs.TabPlacement is TabPlacement.Left or TabPlacement.Right;
                headers.Sort(vertical
                    ? static (a, b) => a.Bounds.Y.CompareTo(b.Bounds.Y)
                    : static (a, b) => a.Bounds.X.CompareTo(b.Bounds.X));
                return headers;
            }

            private static Rect StripRect(List<ContentControl> headers)
            {
                double left = double.MaxValue, top = double.MaxValue, right = double.MinValue, bottom = double.MinValue;
                foreach (Rect bounds in headers.Select(header => header.Bounds))
                {
                    left = Math.Min(left, bounds.X);
                    top = Math.Min(top, bounds.Y);
                    right = Math.Max(right, bounds.Right);
                    bottom = Math.Max(bottom, bounds.Bottom);
                }

                return new Rect(left, top, right - left, bottom - top);
            }
        }
    }
}

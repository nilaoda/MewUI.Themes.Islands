using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace MewUI.Themes.Islands;

/// <summary>One row of an <see cref="IslandsContextMenu"/>. Separators carry no text or action.</summary>
public sealed class IslandsMenuItem
{
    public string Text { get; init; } = string.Empty;

    /// <summary>Builds the 16 DIP glyph for this row; called once per open.</summary>
    public Func<FrameworkElement>? Glyph { get; init; }

    /// <summary>The action run after the menu has closed.</summary>
    public Action? Invoke { get; init; }

    /// <summary>Evaluated when the menu opens.</summary>
    public Func<bool>? IsEnabled { get; init; }

    public bool IsSeparator { get; init; }

    public static IslandsMenuItem Separator { get; } = new() { IsSeparator = true };
}

/// <summary>
/// Islands' self-drawn context menu: a flat panel hosted in the window's overlay layer rather than a
/// separate OS popup. Sharing the window's visual tree keeps input routing, hover and dismissal on the
/// framework's normal path.
/// </summary>
/// <remarks>
/// A transparent backdrop covers the window while the menu is open: left clicks only dismiss, and a
/// right press first dismisses and then asks the caller to open the menu for whatever lies under the
/// pointer (<c>rightPressOutside</c>), which is how one right click can move a menu to another row.
/// </remarks>
public static class IslandsContextMenu
{
    private static readonly Dictionary<Window, MenuState> Open = [];

    public static bool IsOpen(Window owner) => Open.ContainsKey(owner);

    public static void Show(
        Window owner,
        Point position,
        IReadOnlyList<IslandsMenuItem> items,
        Func<Point, bool>? rightPressOutside = null)
    {
        Close(owner);

        var colors = IslandsTheme.CurrentColors;
        // The installed metrics, not the library's default record: a consumer on the compact density
        // expects this menu to step down with the rest of its interface.
        var metrics = Application.Current.Theme.Metrics;

        var rows = new StackPanel().Vertical().Spacing(0);
        foreach (var item in items)
        {
            rows.Children(item.IsSeparator ? SeparatorRow(colors) : ItemRow(owner, colors, metrics, item));
        }

        var content = new ScrollViewer()
            .MaxHeight(Math.Max(60, owner.ClientSize.Height - 24))
            .Content(rows);

        var surface = new Border().Background(colors.MenuBackground);
        surface.BorderBrush = colors.MenuBorder;
        surface.BorderThickness(new Thickness(metrics.ControlBorderThickness));
        surface.CornerRadius(metrics.ControlCornerRadius);
        surface.Padding(new Thickness(4));
        surface.Child(content);
        surface.HorizontalAlignment(HorizontalAlignment.Left);
        surface.VerticalAlignment(VerticalAlignment.Top);

        surface.Measure(new Size(owner.ClientSize.Width, owner.ClientSize.Height));
        var size = surface.DesiredSize;
        var x = Math.Clamp(position.X, 0, Math.Max(0, owner.ClientSize.Width - size.Width));
        var y = Math.Clamp(position.Y, 0, Math.Max(0, owner.ClientSize.Height - size.Height));
        // Alignment plus margin places the panel without a full-window canvas,
        // which would hit-test above the backdrop and eat every outside click.
        surface.Margin(new Thickness(x, y, 0, 0));

        var backdrop = new Border().Background(colors.HoverBackground.WithAlpha(0));
        backdrop
            .OnMouseMove(args => args.Handled = true)
            .OnMouseUp(args => args.Handled = true)
            .OnMouseDown(args =>
            {
                args.Handled = true;
                var point = args.GetPosition(backdrop);
                Close(owner);

                if (args.Button == MouseButton.Right)
                {
                    rightPressOutside?.Invoke(point);
                }
            });

        var root = new Grid()
            .Rows(GridLength.Star)
            .Children(backdrop, surface);

        void OnKeyDown(KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                args.Handled = true;
                Close(owner);
            }
        }

        void OnDeactivated() => Close(owner);

        void OnClosed() => Close(owner);

        owner.PreviewKeyDown += OnKeyDown;
        owner.Deactivated += OnDeactivated;
        owner.Closed += OnClosed;
        owner.OverlayLayer.Add(root);

        Open[owner] = new MenuState(root, OnKeyDown, OnDeactivated, OnClosed);
    }

    public static void Close(Window owner)
    {
        if (!Open.Remove(owner, out var state))
        {
            return;
        }

        owner.PreviewKeyDown -= state.KeyDown;
        owner.Deactivated -= state.Deactivated;
        owner.Closed -= state.Closed;
        owner.OverlayLayer.Remove(state.Root);
    }

    private static UIElement ItemRow(
        Window owner,
        IslandsVariantColors colors,
        ThemeMetrics metrics,
        IslandsMenuItem item)
    {
        var enabled = item.IsEnabled?.Invoke() ?? true;

        var label = new TextBlock()
            .Text(item.Text)
            .FontSize(metrics.FontSize + 1)
            .Foreground(enabled ? colors.Text : colors.DisabledText)
            .CenterVertical()
            .TextTrimming(TextTrimming.CharacterEllipsis);

        var content = new StackPanel()
            .Horizontal()
            .Spacing(8)
            .VerticalAlignment(VerticalAlignment.Center);

        if (item.Glyph?.Invoke() is { } glyph)
        {
            glyph.Width(16).Height(16).CenterVertical();
            glyph.Opacity = enabled ? 1.0 : 0.45;
            content.Children(glyph);
        }

        content.Children(label);

        var row = new Border()
            .Background(colors.HoverBackground.WithAlpha(0))
            .CornerRadius(IslandsMetrics.SelectionArc)
            .Height(IslandsMetrics.NavigationRowHeight)
            .Padding(new Thickness(IslandsMetrics.MenuItemPadding.Left, 0, IslandsMetrics.MenuItemPadding.Right, 0))
            .Child(content);

        if (enabled)
        {
            row
                .OnMouseEnter(() => row.Background(colors.HoverBackground))
                .OnMouseLeave(() => row.Background(colors.HoverBackground.WithAlpha(0)))
                .OnMouseUp(args =>
                {
                    if (args.Button != MouseButton.Left)
                    {
                        return;
                    }

                    args.Handled = true;
                    Close(owner);
                    item.Invoke?.Invoke();
                });
        }

        return row;
    }

    private static UIElement SeparatorRow(IslandsVariantColors colors) =>
        new Border()
            .Height(1)
            .Margin(new Thickness(6, 4, 6, 4))
            .Background(colors.Border);

    private sealed record MenuState(UIElement Root, Action<KeyEventArgs> KeyDown, Action Deactivated, Action Closed);
}

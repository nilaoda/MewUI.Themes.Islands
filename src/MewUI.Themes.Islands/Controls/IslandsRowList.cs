using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands;

/// <summary>A mixed-content list with selectable rows, non-selectable headers and extended selection.</summary>
/// <remarks>The host supplies row objects and content. Ctrl/Cmd toggles and Shift selects a visible range.</remarks>
public sealed class IslandsRowList
{
    private readonly StackPanel _rows = new StackPanel().Vertical().Spacing(2);
    private readonly ScrollViewer _scroll = new ScrollViewer();
    private readonly Dictionary<object, Row> _index = [];
    private readonly List<object> _order = [];

    private Row? _hovered;
    private HashSet<object> _selected = [];
    private object? _primary;
    private object? _anchor;
    private readonly bool _allowMultipleSelection;

    public IslandsRowList() : this(allowMultipleSelection: true) { }

    /// <summary>Creates a list with either a single current row or extended selection.</summary>
    public IslandsRowList(bool allowMultipleSelection)
    {
        _allowMultipleSelection = allowMultipleSelection;
        _scroll.Content(_rows).Focusable(true).OnKeyDown(args =>
        {
            if (_order.Count == 0 || args.Key is not (Key.Up or Key.Down or Key.Home or Key.End)) return;
            var index = _primary is null ? -1 : _order.IndexOf(_primary);
            index = args.Key switch
            {
                Key.Home => 0,
                Key.End => _order.Count - 1,
                Key.Up => Math.Max(0, index - 1),
                _ => Math.Min(_order.Count - 1, index + 1),
            };
            var item = _order[index];
            if (_allowMultipleSelection && (args.Modifiers & ModifierKeys.Shift) != 0) SelectRange(item);
            else Select(item);
            var plate = _index[item].Plate;
            var top = plate.TranslatePoint(new Point(0, 0), _scroll).Y;
            var offset = _scroll.VerticalOffset;
            if (top < 0) _scroll.SetScrollOffsets(_scroll.HorizontalOffset, offset + top);
            else if (top + plate.Bounds.Height > _scroll.ViewportHeight)
                _scroll.SetScrollOffsets(_scroll.HorizontalOffset, offset + top + plate.Bounds.Height - _scroll.ViewportHeight);
            args.Handled = true;
        });
    }
    public UIElement Element => _scroll;
    /// <summary>Returns the viewport to its start after the host changes a filter.</summary>
    public void ScrollToStart() => _scroll.SetScrollOffsets(0, 0);
    public event Action? SelectionChanged;
    public event Action<object, Point>? RowMenuRequested;
    public void AddHeader(UIElement content) => _rows.Add(content);
    public void AddRow(object item, GridLength[] widths, params UIElement[] cells)
    {
        var grid = new Grid().Columns(widths).Spacing(12);

        for (var i = 0; i < cells.Length; i++)
        {
            grid.Children(cells[i].GridPosition(0, i));
        }

        AddRow(item, grid);
    }
    public void AddRow(object item, UIElement content)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(content);
        if (_index.ContainsKey(item))
        {
            throw new ArgumentException("Row objects must be unique within a list.", nameof(item));
        }

        _index[item] = new Row(this, item, content);
        _order.Add(item);
    }
    public void Clear()
    {
        _hovered = null;
        _selected = [];
        _primary = null;
        _anchor = null;
        _index.Clear();
        _order.Clear();
        _rows.Clear();
    }
    public bool TryGetItemAt(Point windowPoint, out object? item)
    {
        var probe = new MouseEventArgs(windowPoint, default, MouseButton.Left);
        foreach (var row in _index.Values)
        {
            var local = probe.GetPosition(row.Plate);
            if (local.X >= 0 && local.Y >= 0 && local.X < row.Plate.Bounds.Width && local.Y < row.Plate.Bounds.Height)
            {
                item = row.Item;
                return true;
            }
        }

        item = null;
        return false;
    }
    public object? SelectedItem => _primary;
    public IReadOnlyList<object> SelectedItems => [.. _order.Where(_selected.Contains)];
    /// <summary>Row objects in display order; headers are excluded.</summary>
    public IReadOnlyList<object> Items => _order.AsReadOnly();

    /// <summary>Restores a selection after the host rebuilds the rows. Unknown objects are ignored.</summary>
    public void SelectMany(IEnumerable<object> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var selected = items.Where(_index.ContainsKey).ToHashSet();
        var primary = _order.FirstOrDefault(selected.Contains);
        if (!_allowMultipleSelection) selected = primary is null ? [] : [primary];
        ApplySelection(selected, primary, primary);
    }
    public void Select(object? item)
    {
        if (item is not null && !_index.ContainsKey(item))
        {
            item = null;
        }

        ApplySelection(
            item is null ? [] : new HashSet<object> { item },
            item,
            item);
    }

    private void ToggleSelection(object item)
    {
        var next = new HashSet<object>(_selected);

        if (!next.Remove(item))
        {
            next.Add(item);
        }

        var primary = next.Contains(item)
            ? item
            : ReferenceEquals(_primary, item)
                ? _order.FirstOrDefault(next.Contains)
                : _primary;

        ApplySelection(next, primary, item);
    }

    private void SelectRange(object item)
    {
        if (_anchor is null)
        {
            ApplySelection(new HashSet<object> { item }, item, item);
            return;
        }

        var from = _order.IndexOf(_anchor);
        var to = _order.IndexOf(item);
        if (from < 0 || to < 0)
        {
            ApplySelection(new HashSet<object> { item }, item, item);
            return;
        }

        var start = Math.Min(from, to);
        var end = Math.Max(from, to);
        var next = new HashSet<object>();

        for (var i = start; i <= end; i++)
        {
            next.Add(_order[i]);
        }

        ApplySelection(next, item, _anchor);
    }
    private void ApplySelection(HashSet<object> next, object? primary, object? anchor)
    {
        var repaint = new List<Row>();

        foreach (var row in _index.Values)
        {
            var was = _selected.Contains(row.Item);
            var now = next.Contains(row.Item);

            if (was != now)
            {
                repaint.Add(row);
            }
        }

        _selected = next;
        _primary = primary;
        _anchor = anchor;

        foreach (var row in repaint)
        {
            row.Repaint();
        }

        SelectionChanged?.Invoke();
    }

    private void HandleLeftClick(object item, MouseEventArgs args)
    {
        _scroll.Focus();
        if (!_allowMultipleSelection)
        {
            Select(item);
            return;
        }
        if ((args.Modifiers & ModifierKeys.Shift) != 0)
        {
            SelectRange(item);
            return;
        }

        if (args.PrimaryKey || (args.Modifiers & ModifierKeys.Control) != 0)
        {
            ToggleSelection(item);
            return;
        }

        Select(item);
    }

    private void SetHover(Row? next)
    {
        if (ReferenceEquals(next, _hovered))
        {
            return;
        }

        var previous = _hovered;
        _hovered = next;

        previous?.Repaint();
        next?.Repaint();
    }

    private sealed class Row
    {
        private readonly IslandsRowList _owner;

        public Row(IslandsRowList owner, object item, UIElement content)
        {
            _owner = owner;
            Item = item;

            Plate = new Border()
                .CornerRadius(IslandsMetrics.SelectionArc)
                .Padding(8, 3, 8, 3)
                .Child(content);
            owner._rows.Add(Plate);
            Plate.WithTheme((theme, _) =>
            {
                _colors = IslandsTheme.Colors(theme);
                Repaint();
            }, invokeImmediately: Application.IsRunning);

            Plate
                .OnMouseEnter(() => owner.SetHover(this))
                .OnMouseLeave(() => owner.SetHover(null))
                .OnMouseDown(args =>
                {
                    if (args.Button == MouseButton.Right)
                    {
                        _rightPressed = true;
                        if (!owner._selected.Contains(item))
                        {
                            owner.Select(item);
                        }

                        return;
                    }

                    if (args.Button != MouseButton.Left)
                    {
                        return;
                    }

                    args.Handled = true;
                    owner.HandleLeftClick(item, args);
                })
                .OnMouseUp(args =>
                {
                    if (args.Button != MouseButton.Right || !_rightPressed)
                    {
                        return;
                    }

                    _rightPressed = false;
                    if (Plate.FindVisualRoot() is Window window)
                    {
                        owner.RowMenuRequested?.Invoke(item, args.GetPosition(window));
                    }
                });
        }

        public object Item { get; }

        public Border Plate { get; }

        private bool _rightPressed;
        private IslandsVariantColors? _colors;
        internal void Repaint()
        {
            if (_colors is not { } colors)
            {
                return;
            }
            Plate.Background(_owner._selected.Contains(Item)
                ? colors.Selection
                : ReferenceEquals(this, _owner._hovered)
                    ? colors.HoverBackground
                    : colors.HoverBackground.WithAlpha(0));
        }
    }
}

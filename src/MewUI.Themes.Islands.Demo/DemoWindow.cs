using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;
using MewUI.Themes.Islands.Demo.Pages;

namespace MewUI.Themes.Islands.Demo;

/// <summary>
/// The gallery shell: a title strip from the theme, a navigation island and a page host.
/// </summary>
internal sealed class DemoWindow : Window
{
    private static readonly string[] PageNames =
    [
        "Overview",
        "Controls",
        "Lists & Trees",
        "Dialogs & Menus",
        "Window Chrome",
        "Colors & Metrics",
    ];

    private readonly DemoOptions _options;
    private readonly IslandsWindowChrome _chrome;
    private readonly ObservableValue<string> _status = new("Ready");
    private readonly List<(Border Row, TextBlock Label)> _navRows = [];
    private readonly UIElement?[] _pages = new UIElement?[PageNames.Length];

    private Border _pageHost = null!;
    private int _selectedPage;
    private int _hoveredNav = -1;
    private bool _closed;
    private bool _rebuildQueued;

    public DemoWindow(DemoOptions options)
    {
        _options = options;
        _selectedPage = Math.Max(0, Array.FindIndex(
            PageNames,
            name => string.Equals(name, options.InitialPage, StringComparison.OrdinalIgnoreCase)));

        this.Title("MewUI Themes · Islands")
            .Resizable(1280, 800, minWidth: 960, minHeight: 600)
            .StartCenterScreen();

        _chrome = this.IslandsFramelessChrome();
        this.Closed += () => _closed = true;
    }

    protected override void OnBuild() => this.Content(BuildLayout());

    /// <summary>
    /// Row surfaces are captured when a row is built, so the whole tree is rebuilt after a
    /// variant change. Deferred so the combo box that caused the change is not torn down
    /// inside its own callback.
    /// </summary>
    protected override void OnThemeChanged(Theme oldTheme, Theme newTheme)
    {
        base.OnThemeChanged(oldTheme, newTheme);
        ScheduleRebuild();
    }

    internal void SetStatus(string text) => _status.Value = text;

    private void ScheduleRebuild()
    {
        if (_closed || _rebuildQueued || Content is null)
        {
            return;
        }

        _rebuildQueued = true;

        void Run()
        {
            _rebuildQueued = false;

            if (!_closed)
            {
                RebuildContent();
            }
        }

        if (Application.Current.Dispatcher is { } dispatcher)
        {
            dispatcher.BeginInvoke(DispatcherPriority.Background, Run);
        }
        else
        {
            Run();
        }
    }

    private void RebuildContent()
    {
        var colors = IslandsTheme.CurrentColors;

        // Keep the window's alpha: on Windows the backdrop can be a translucent wash.
        var alpha = Background.A == 0 ? (byte)255 : Background.A;
        Background = colors.PanelBackground.WithAlpha(alpha);
        Foreground = colors.Text;

        Array.Clear(_pages);
        _navRows.Clear();
        _hoveredNav = -1;

        this.Content(BuildLayout());
    }

    private UIElement BuildLayout()
    {
        _pageHost = new Border().Padding(24, 16, 24, 24);

        var content = new ScrollViewer()
            .Content(_pageHost)
            .AutoVerticalScroll();

        var workspace = new IslandsWorkspace(BuildNav(), content, null, 232, 190, 640);

        return new DockPanel().Spacing(0).Children(
            _chrome.TitleBar("MewUI Themes · Islands", BuildTitleActions()).DockTop(),
            BuildStatusBar().DockBottom(),
            workspace.Element);
    }

    private UIElement BuildTitleActions()
    {
        var theme = new ComboBox()
            .Items("System", "Light", "Dark")
            .SelectedIndex((int)Application.Current.ThemeMode)
            .IslandsDropDownArrow()
            .Width(112);

        theme.OnSelectionChanged(_ =>
        {
            if (theme.SelectedIndex is < 0 or > 2)
            {
                return;
            }

            var mode = (ThemeVariant)theme.SelectedIndex;
            Application.Current.SetThemeMode(mode);
            SetStatus($"Theme: {mode}");
        });

        var density = new TextBlock()
            .Text(_options.Density == IslandsDensity.Compact ? "Compact density" : "Regular density")
            .IslandsHint()
            .CenterVertical();

        var dialog = IslandsToolbar.ActionButton(IslandsIconKind.Settings, "Dialog");
        dialog.OnClick(() => _ = ShowSampleDialogAsync());

        return new StackPanel().Horizontal().Spacing(10).Children(theme, density, dialog);
    }

    private UIElement BuildNav()
    {
        var nav = new StackPanel().Vertical().Spacing(2);
        for (var i = 0; i < PageNames.Length; i++)
        {
            nav.Children(NavRow(i, PageNames[i]));
        }

        SelectPage(_selectedPage);

        var product = IslandsProductInfo.Entry(
            "Islands Theme",
            "0.1.0",
            "MewUI theme gallery",
            () => SetStatus("MewUI.Themes.Islands · MIT"));

        return new Border().Padding(8, 10, 8, 8).Child(
            new DockPanel().Spacing(0).Children(
                product.DockBottom(),
                nav));
    }

    private UIElement NavRow(int index, string text)
    {
        var label = new TextBlock().Text(text).FontSize(13).CenterVertical();

        Border row = null!;
        row = new Border()
            .CornerRadius(IslandsMetrics.SelectionArc)
            .Padding(10, 5, 10, 5)
            .Child(label)
            .OnMouseEnter(() =>
            {
                _hoveredNav = index;
                RepaintNav();
            })
            .OnMouseLeave(() =>
            {
                if (_hoveredNav == index)
                {
                    _hoveredNav = -1;
                    RepaintNav();
                }
            })
            .OnMouseDown(args =>
            {
                if (args.Button == MouseButton.Left)
                {
                    args.Handled = true;
                    SelectPage(index);
                }
            });

        _navRows.Add((row, label));
        return row;
    }

    private void SelectPage(int index)
    {
        _selectedPage = index;
        _pages[index] ??= BuildPage(index);
        _pageHost.Child(_pages[index]!);
        RepaintNav();
    }

    private void RepaintNav()
    {
        var colors = IslandsTheme.CurrentColors;

        for (var i = 0; i < _navRows.Count; i++)
        {
            var selected = i == _selectedPage;

            _navRows[i].Row.Background(selected
                ? colors.Selection
                : i == _hoveredNav ? colors.HoverBackground : colors.HoverBackground.WithAlpha(0));
            _navRows[i].Label.Foreground(selected ? colors.SelectionText : colors.Text);
        }
    }

    private UIElement BuildPage(int index) => index switch
    {
        0 => OverviewPage.Build(this),
        1 => ControlsPage.Build(this),
        2 => ListsPage.Build(this),
        3 => DialogsPage.Build(this),
        4 => ChromePage.Build(this),
        _ => ColorsPage.Build(this),
    };

    private UIElement BuildStatusBar() =>
        new Border()
            .IslandsStatusBar()
            .Child(new TextBlock().BindText(_status));

    internal async Task ShowSampleDialogAsync()
    {
        var dialog = new Window()
            .Title("Islands Dialog")
            .Resizable(520, 340, minWidth: 420, minHeight: 280)
            .StartCenterOwner();

        var chrome = dialog.IslandsDialogChrome();
        var close = new Button().Content("Close").IslandsPrimaryButton();
        close.OnClick(() => dialog.Close());

        dialog.Content(new DockPanel().Spacing(0).Children(
            chrome.TitleBar("Islands Dialog").DockTop(),
            new Border().IslandsDialogFooter().Child(
                new StackPanel().Horizontal().Spacing(8).Children(close)).DockBottom(),
            new StackPanel().Vertical().Spacing(12).Margin(24).Children(
                new TextBlock().Text("A themed modal dialog").IslandsTitle(),
                new TextBlock().Text("The dialog uses the same Islands chrome, typography and footer as the main window.").IslandsHint(),
                new Separator(),
                new CheckBox().Content("Remember this choice").IsChecked(true))));

        SetStatus("Dialog opened");
        await dialog.ShowDialogAsync(this);
        SetStatus("Dialog closed");
    }

    internal void ShowContextMenu(FrameworkElement anchor)
    {
        var items = new List<IslandsMenuItem>
        {
            new()
            {
                Text = "New item",
                Glyph = () => IslandsIcon.Create(IslandsIconKind.Plus),
                Invoke = () => SetStatus("New item"),
            },
            new()
            {
                Text = "Rename",
                Glyph = () => IslandsIcon.Create(IslandsIconKind.Pencil),
                Invoke = () => SetStatus("Rename"),
            },
            new()
            {
                Text = "Duplicate",
                Glyph = () => IslandsIcon.Create(IslandsIconKind.Copy),
                Invoke = () => SetStatus("Duplicate"),
            },
            IslandsMenuItem.Separator,
            new()
            {
                Text = "Delete",
                Glyph = () => IslandsIcon.Create(IslandsIconKind.Trash),
                Invoke = () => SetStatus("Delete"),
            },
        };

        var origin = anchor.TranslatePoint(new Point(0, anchor.Bounds.Height + 4), this);
        IslandsContextMenu.Show(this, origin, items, null);
    }

    internal async Task OpenFramelessWindowAsync()
    {
        var window = new Window()
            .Title("Frameless Islands Window")
            .Resizable(720, 480, minWidth: 520, minHeight: 360)
            .StartCenterOwner();

        var chrome = window.IslandsFramelessChrome();
        var close = new Button().Content("Close").IslandsPrimaryButton();
        close.OnClick(() => window.Close());

        window.Content(new DockPanel().Spacing(0).Children(
            chrome.TitleBar("Frameless Islands Window").DockTop(),
            new StackPanel().Vertical().Spacing(12).Margin(24).Children(
                new TextBlock().Text("Frameless window").IslandsTitle(),
                new TextBlock().Text("The title bar, drag region, caption buttons and snap layouts come from the theme.").IslandsHint(),
                close)));

        await window.ShowDialogAsync(this);
    }
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace MewUI.Themes.Islands;

/// <summary>
/// A window whose title bar is the application's own: the strip carries the title and whatever actions the
/// window wants, and the platform's title bar is given up in exchange.
/// </summary>
/// <remarks>
/// <para>
/// Islands' own IDE windows are shaped this way — one bar across the top that is title, drag handle and
/// toolbar at once — which is why the capability lives here rather than in the application. An application
/// asks for it and gets it:
/// </para>
/// <code>
/// var chrome = this.IslandsFramelessChrome();
/// ...
/// .Children(chrome.TitleBar(Ui.T(Msg.UiAppTitle), ToolBarActions()).DockTop(), ...)
/// </code>
/// <para>
/// What the platform provides differs, and that difference is the whole reason this is not three lines in an
/// application:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// On macOS the extended title bar keeps the native traffic lights, and their width is only knowable once the
/// window is on screen — <see cref="Window.NativeChromeButtonInset"/> reads the zoom button's own frame. The
/// strip therefore starts flush and is inset from <c>Loaded</c>, not at build time.
/// </description>
/// </item>
/// <item>
/// <description>
/// On macOS the platform also turns <c>movableByWindowBackground</c> off — window drag is the application's
/// business there — so a strip that does not call <see cref="Window.DragMove"/> produces a window that cannot
/// be moved at all.
/// </description>
/// </item>
/// <item>
/// <description>
/// On macOS the platform also centres a window a quarter of the leftover height above the middle, which is a
/// compensation for a title bar this window gave up. <see cref="IslandsWindowPlacement"/> takes the window
/// back down to the middle from <c>Loaded</c>, so a window that asked to be centred is one that is centred.
/// </description>
/// </item>
/// </list>
/// </remarks>
public sealed class IslandsWindowChrome
{
    private readonly Window _window;
    private readonly List<Border> _strips = [];
    private readonly HashSet<Border> _leftAlignedStrips = [];
    private readonly List<TextBlock> _headings = [];
    private readonly List<UIElement> _captionRows = [];
    private readonly List<(UIElement Button, Func<bool> Allowed)> _captionButtons = [];

    /// <summary>Where this window should land, once the platform has been relieved of answering.</summary>
    private readonly IslandsWindowPlacement _placement;

    /// <summary>The button the system's layout flyout hangs off, once the strip has built one.</summary>
    private CaptionFace? _restore;
    private bool _pendingStripToggle;

    /// <summary>The window's one native hook, rebound whenever its caption controls are rebuilt.</summary>
    private readonly IslandsWindowSnapLayouts _snapLayouts;

    internal IslandsWindowChrome(Window window)
    {
        _window = window;
        IslandsWindowHover.Attach(window);
        _snapLayouts = new IslandsWindowSnapLayouts(window);

        // Claimed here, at the point the window is certainly still unshown, and applied from Loaded.
        _placement = new IslandsWindowPlacement(window);
    }

    /// <summary>
    /// The top strip: title centered on the window, actions on the right, draggable in between.
    /// </summary>
    /// <param name="title">The window's title, centered independently of its actions.</param>
    /// <param name="actions">The elements to place at the end of the strip; null leaves it empty.</param>
    /// <returns>The strip, ready to be docked at the top of the window's layout.</returns>
    public Border TitleBar(string title, UIElement? actions = null) =>
        TitleBar(title, actions, HorizontalAlignment.Center);

    /// <summary>A title strip with explicit heading alignment, for a workspace or a dialog.</summary>
    public Border TitleBar(string title, UIElement? actions, HorizontalAlignment titleAlignment)
    {
        ArgumentNullException.ThrowIfNull(title);

        var bar = new Border()
            .IslandsToolbarSurface()
            .Height(IslandsMetrics.TitleBarHeight)
            .Padding(IslandsMetrics.TitleBarPadding)
            .OnMouseDown(OnStripMouseDown)
            .OnMouseUp(OnStripMouseUp)
            .Child(Strip(_window, title, actions, titleAlignment));

        // A title bar spans the window; only the content is inset from the frame. The window's own horizontal
        // inset is applied to everything the window holds, so the strip has to reach back out through it —
        // otherwise the strip stops where the content stops and the window buttons end a gutter short of the
        // frame's corner, which is not where Windows puts them. The outdent is symmetric, so the centred
        // title does not move, and the strip's own surface is the panel's colour either way.
        bar.WithTheme((theme, element) => element.Margin(new Thickness(
            -theme.Metrics.ContainerPadding.Left,
            0,
            -theme.Metrics.ContainerPadding.Right,
            0)));

        _strips.Add(bar);
        if (titleAlignment == HorizontalAlignment.Left)
        {
            _leftAlignedStrips.Add(bar);
        }

        // The strip is built before the window is on screen, so the native inset is applied twice: once now
        // with whatever the theme states, and again when the platform can answer.
        ApplyInsets();
        return bar;
    }

    /// <summary>
    /// Replace the caption shown in every strip this chrome has built.
    /// </summary>
    /// <remarks>
    /// A window that changes what it is showing — a list, then one entry of that list — has to say so in the
    /// strip. Rebuilding the strip is not an option: the actions it hosts were fitted to the strip height when
    /// they were added, and freshly built ones would sit at their own height beside the old ones.
    /// </remarks>
    /// <param name="title">The caption to show; never empty.</param>
    public void SetTitle(string title)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);

        foreach (TextBlock heading in _headings)
        {
            heading.Text = title;
        }
    }

    /// <summary>
    /// The strip's one row: the title centred on the window, and the window's actions at its end.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Both go in one cell and take their own alignment, which is what centres the title on the window rather
    /// than on whatever is left over beside the actions. The SDK's own dialog reference
    /// (<c>dialog_popup_sizes_extra_small.png</c>) puts the title in the middle of the bar, between the window
    /// buttons, and the same position is what keeps a window title from reading as one more panel heading:
    /// a title at the start sits in the same column, in the same weight, as the heading of the pane right
    /// below it.
    /// </para>
    /// <para>
    /// The two only meet if a window is narrow enough for a centred title to reach its actions; a window that
    /// states a minimum width worth the name is wider than that.
    /// </para>
    /// </remarks>
    private UIElement Strip(Window window, string title, UIElement? actions, HorizontalAlignment titleAlignment)
    {
        var heading = new TextBlock()
            .Text(title)
            .IslandsSectionTitle()
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(titleAlignment)
            .GridPosition(0, 0);

        _headings.Add(heading);

        var row = new Grid().Columns(GridLength.Star);
        row.Children(heading);

        var tail = new StackPanel()
            .Horizontal()
            .Spacing(8)
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .GridPosition(0, 0);

        if (actions is not null)
        {
            FitIntoStrip(actions);
            tail.Children(actions);
        }

        // Built either way, shown once the window can say. Whether a platform draws its own window buttons is a
        // property of the realised window — macOS only reports NativeChromeButtons once its extended title bar
        // is in place — so deciding here would draw them on macOS too.
        var captions = CaptionButtons(window);
        _captionRows.Add(captions);
        tail.Children(captions);

        return row.Children(tail);
    }

    /// <summary>
    /// The window's own minimize, maximize and close, for a platform that draws none.
    /// </summary>
    /// <remarks>
    /// A platform either draws the window buttons itself — macOS, which says so through
    /// <see cref="Window.HasNativeChromeButtons"/> and keeps its traffic lights — or draws none, and then giving
    /// up the title bar leaves the window with no way to be minimized, maximized or closed at all. This is that
    /// second case, and it is why the strip draws them rather than the application.
    /// </remarks>
    private UIElement CaptionButtons(Window window)
    {
        var row = new StackPanel().Horizontal().IsVisible(false);

        Add(CaptionButton(IslandsMetrics.CaptionButtonWidth, Glyph(CaptionGlyph.Minimize), CaptionGlyph.Minimize, window.Minimize).Surface, () => window.CanMinimize);

        // Kept, because the system wants to be told where this one is: the layout flyout is offered over the
        // maximize button and nowhere else. See IslandsWindowSnapLayouts.
        var restore = MaximizeButton(window);
        _restore = restore;
        Add(restore.Surface, () => window.CanMaximize);

        Add(CaptionButton(IslandsMetrics.CaptionButtonWidth, Glyph(CaptionGlyph.Close), CaptionGlyph.Close, window.Close).Surface, () => window.CanClose);

        return row;

        // Which of the three a window allows is also only settled once it is realised — the Win32 side coerces
        // CanMaximize from the window style it has by then — so each button is built and asked for its answer
        // later, rather than being left out here.
        void Add(UIElement button, Func<bool> allowed)
        {
            _captionButtons.Add((button, allowed));
            row.Children(button);
        }
    }

    /// <summary>
    /// The maximize button, which is also the restore button: the system swaps the glyph with the window state,
    /// and so does this one.
    /// </summary>
    private CaptionFace MaximizeButton(Window window)
    {
        var maximized = Glyph(CaptionGlyph.Restore);
        var normal = Glyph(CaptionGlyph.Maximize);

        // The two marks share one cell and only one of them is ever shown, so they have to agree on where the
        // middle is: the restore mark is a pair of squares and the maximize mark is a single one, and a
        // corner-anchored pair would sit half a square away from the square it replaces.
        Centre(maximized);
        Centre(normal);

        void Sync(WindowState _)
        {
            bool isMaximized = window.WindowState == WindowState.Maximized;
            maximized.IsVisible(isMaximized);
            normal.IsVisible(!isMaximized);
        }

        Sync(window.WindowState);
        window.WindowStateChanged += Sync;

        return CaptionButton(
            IslandsMetrics.CaptionButtonWidth,
            new Grid().Children(maximized, normal),
            CaptionGlyph.Maximize,
            ToggleMaximize);
    }

    /// <summary>
    /// Maximize, or come back down from it: one answer for the caption button and for a double-click on the strip.
    /// </summary>
    private void ToggleMaximize()
    {
        if (!_window.CanMaximize)
        {
            return;
        }

        if (_window.WindowState == WindowState.Maximized)
        {
            _window.Restore();
        }
        else
        {
            _window.Maximize();
        }
    }

    /// <summary>
    /// One window button: square, as tall as the strip, and acting on release like the system's own.
    /// </summary>
    /// <remarks>
    /// A plain surface rather than a button control, deliberately: a window button is not a tab stop, and a
    /// surface can mark its own press as handled — which is what keeps the strip from reading the same press as
    /// the start of a window drag. Hover and press are the same hue at different alphas, because a colour
    /// transition between two hues passes through a mix of them; see the transition note in
    /// <c>docs/color-map.md</c>.
    /// </remarks>
    private static CaptionFace CaptionButton(double width, UIElement glyph, CaptionGlyph kind, Action activate)
    {
        // The glyph is handed in rather than built from the kind: the maximize button swaps its own child for a
        // two-glyph grid, and a button that painted the element it built first would leave that grid unpainted —
        // which is exactly how the maximize button came out blank.
        bool close = kind == CaptionGlyph.Close;

        // The mark inside a window button must not take the hit test, or the button only half works. A strip
        // that extends the client area gets its clicks through the framework's own hit test, which hands the
        // press to the caption — a window drag — unless the deepest element under the pointer is a control.
        // A glyph is not: the minimize dash is a shape, but the maximize and close marks are grids, and a grid
        // is a panel. Their middles would drag the window and only their edges would press. The face is the
        // control; the mark on it is decoration.
        glyph.IsHitTestVisible(false);
        Centre(glyph);

        var face = new Border()
            .Width(width)
            .Height(IslandsMetrics.TitleBarHeight)
            .Child(glyph);

        // The window paints the strip's background, so these sit on whatever the strip has — which on a system
        // backdrop is the backdrop itself, and that is the point of it.
        face.WithTheme((theme, element) =>
        {
            var colors = IslandsTheme.Colors(theme);
            element.Background(HoverHue(colors, close).WithAlpha(0));
            PaintGlyph(glyph, kind, colors.Text);
        });

        // Painted once up front as well: a shape with no brush of its own draws nothing, and an inherited
        // foreground is not something to rely on for a glyph.
        PaintGlyph(glyph, kind, IslandsTheme.CurrentColors.Text);

        bool hot = false;
        bool pressed = false;

        // One place decides what the button looks like, because two different things drive it: the pointer
        // while the strip owns its input, and the non-client messages once the system has taken the maximize
        // button for its layout flyout. See IslandsWindowSnapLayouts.
        void Repaint()
        {
            var hue = HoverHue(IslandsTheme.CurrentColors, close);
            Paint(face, glyph, kind, pressed
                ? hue.WithAlpha(IslandsMetrics.CaptionPressedAlpha)
                : hot ? hue : hue.WithAlpha(0), close);
        }

        face.OnMouseEnter(() => { hot = true; Repaint(); });
        face.OnMouseLeave(() => { hot = false; pressed = false; Repaint(); });

        face.OnMouseDown(args =>
        {
            // Handled: without it the strip reads the same press as the start of a drag.
            args.Handled = true;
            pressed = true;
            Repaint();
        });

        face.OnMouseUp(args =>
        {
            args.Handled = true;
            pressed = false;
            Repaint();
            activate();
        });

        return new CaptionFace(
            face,
            value => { hot = value; Repaint(); },
            value => { pressed = value; Repaint(); });
    }

    /// <summary>The fill a window button hovers with: the ordinary overlay, or the system's red for close.</summary>
    private static Color HoverHue(IslandsVariantColors colors, bool close) =>
        close ? colors.CaptionCloseHover : colors.HoverBackground;

    /// <summary>Paints a button's face and its glyph; the close button inverts its glyph while lit.</summary>
    private static void Paint(Border face, UIElement glyph, CaptionGlyph kind, Color background, bool close)
    {
        face.Background(background);

        // Over the close red the glyph goes white; Islands already states that colour for text over a solid
        // fill, so the table does not need a second one for it.
        var colors = IslandsTheme.CurrentColors;
        PaintGlyph(glyph, kind, close && background.A > 0 ? colors.AccentText : colors.Text);
    }

    /// <summary>
    /// Recolours a glyph by the kind of mark it is.
    /// </summary>
    /// <remarks>
    /// A filled mark takes a fill, an outline and a line take a stroke, and the two are not interchangeable: an
    /// outline given a fill of its own draws as a solid square, and one left with no brush at all draws
    /// nothing — which is how the maximize button came out invisible.
    /// </remarks>
    private static void PaintGlyph(UIElement glyph, CaptionGlyph kind, Color color)
    {
        switch (glyph)
        {
            case Panel panel:
                foreach (var child in panel.Children)
                {
                    if (child is UIElement element)
                    {
                        PaintGlyph(element, kind, color);
                    }
                }

                break;
            case Shape shape when kind == CaptionGlyph.Minimize:
                shape.Fill(color);
                break;
            case Shape shape:
                // Outlines and lines alike: both are drawn with a stroke, and a line is a shape too.
                shape.Stroke(color);
                break;
        }
    }

    /// <summary>
    /// The glyph a window button draws.
    /// </summary>
    /// <remarks>
    /// Drawn as shapes rather than taken from an icon font. The system's own caption font is Windows-only and
    /// differs between Windows 10 and 11, while these four glyphs are four lines and rectangles — and they then
    /// look the same wherever the strip is drawn.
    /// </remarks>
    /// <summary>The box every window mark is drawn in: ten across, ten down, the system's own caption size.</summary>
    private const double GlyphBox = 10;

    /// <summary>
    /// How much is taken off a window mark's corners. Windows 11 strokes its marks as thin rounded frames
    /// rather than as sharp rectangles, and a rounded corner is also what keeps a nine-by-nine frame from
    /// looking cut off at the stroke's ends.
    /// </summary>
    private const double GlyphArc = 1;

    private static UIElement Glyph(CaptionGlyph kind) => kind switch
    {
        CaptionGlyph.Minimize => new Rectangle { Width = 10, Height = 1 },
        CaptionGlyph.Maximize => Mark(RoundedFrame(0.5, 0.5, 9)),
        CaptionGlyph.Restore => RestoreGlyph(),
        _ => CloseGlyph(),
    };

    /// <summary>
    /// A mark, drawn in the box all three share.
    /// </summary>
    /// <remarks>
    /// The box is stated rather than measured: a shape draws its geometry in its own coordinates, and one
    /// left to size itself would be as wide as its ink. That matters most for the restore mark, whose two
    /// frames have different ink — sized to their ink they would not share an origin and would not line up.
    /// </remarks>
    private static Shape Mark(PathGeometry geometry)
    {
        var shape = new PathShape { Data = geometry, Width = GlyphBox, Height = GlyphBox };
        shape.HorizontalAlignment(HorizontalAlignment.Left);
        shape.VerticalAlignment(VerticalAlignment.Top);
        return shape;
    }

    /// <summary>
    /// A rounded frame, starting from the middle of its top edge.
    /// </summary>
    private static PathGeometry RoundedFrame(double x, double y, double size)
    {
        double right = x + size;
        double bottom = y + size;

        var frame = new PathGeometry();
        frame.MoveTo(x + GlyphArc, y);
        frame.LineTo(right - GlyphArc, y);
        frame.QuadTo(right, y, right, y + GlyphArc);
        frame.LineTo(right, bottom - GlyphArc);
        frame.QuadTo(right, bottom, right - GlyphArc, bottom);
        frame.LineTo(x + GlyphArc, bottom);
        frame.QuadTo(x, bottom, x, bottom - GlyphArc);
        frame.LineTo(x, y + GlyphArc);
        frame.QuadTo(x, y, x + GlyphArc, y);
        frame.Close();
        return frame;
    }

    /// <summary>
    /// The restore mark: two rounded frames, one behind and a corner above the other.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One path with two subpaths rather than two shapes, which is how the mark is normally written and also
    /// what keeps the pair in one ink: the back frame's ink reaches a corner above and right of the front
    /// frame's, so the mark's own bounds are the box rather than either frame's, and one element can say that
    /// where two cannot.
    /// </para>
    /// <para>
    /// The back frame stops where the front one starts rather than passing under it, and that is a decision
    /// rather than a simplification: the pair is usually drawn with the front one filled in the colour of the
    /// surface behind it, and there is no such fill here — the button sits on whatever the strip has, which on
    /// a system backdrop is the backdrop itself. A frame that is simply not drawn where it would be covered
    /// works on every surface.
    /// </para>
    /// </remarks>
    private static UIElement RestoreGlyph()
    {
        var mark = new PathGeometry();

        mark.MoveTo(2.5, 2.5);
        mark.LineTo(2.5, 1.5);
        mark.QuadTo(2.5, 0.5, 3.5, 0.5);
        mark.LineTo(8.5, 0.5);
        mark.QuadTo(9.5, 0.5, 9.5, 1.5);
        mark.LineTo(9.5, 6.5);
        mark.QuadTo(9.5, 7.5, 8.5, 7.5);
        mark.LineTo(7.5, 7.5);

        mark.MoveTo(1.5, 2.5);
        mark.LineTo(6.5, 2.5);
        mark.QuadTo(7.5, 2.5, 7.5, 3.5);
        mark.LineTo(7.5, 8.5);
        mark.QuadTo(7.5, 9.5, 6.5, 9.5);
        mark.LineTo(1.5, 9.5);
        mark.QuadTo(0.5, 9.5, 0.5, 8.5);
        mark.LineTo(0.5, 3.5);
        mark.QuadTo(0.5, 2.5, 1.5, 2.5);

        return Mark(mark);
    }

    private static UIElement CloseGlyph() =>
        new Grid()
            .Width(GlyphBox)
            .Height(GlyphBox)
            .Children(
                new Line { X1 = 0, Y1 = 0, X2 = GlyphBox, Y2 = GlyphBox, StrokeThickness = 1 },
                new Line { X1 = GlyphBox, Y1 = 0, X2 = 0, Y2 = GlyphBox, StrokeThickness = 1 });

    private enum CaptionGlyph
    {
        Minimize,
        Maximize,
        Restore,
        Close,
    }

    /// <summary>
    /// A window button's surface, together with the two states something outside it may need to drive.
    /// </summary>
    /// <remarks>
    /// Handed back rather than kept private because the maximize button has a second owner on Windows: the
    /// system takes the pointer over it so it can offer the layout flyout, and the button's look then has to
    /// be driven from the non-client messages instead of the usual enter/leave pair. See
    /// <see cref="IslandsWindowSnapLayouts"/>.
    /// </remarks>
    private readonly record struct CaptionFace(Border Surface, Action<bool> SetHot, Action<bool> SetPressed);

    /// <summary>
    /// Puts a mark in the middle of the surface it is drawn on.
    /// </summary>
    /// <remarks>
    /// Said rather than assumed, because the default is not the middle: an element that states a size of its
    /// own is placed at its slot's start corner unless it asks otherwise — <c>FrameworkElement</c>'s arrange
    /// only centres for an explicit <c>Center</c> — so a 10 DIP mark lands in the top-left of a 46 DIP button.
    /// That is exactly how the three window marks came out: the close cross sat over its button's corner
    /// rather than its middle, and the row read as unevenly spaced because the space was all on one side of
    /// each mark. A mark with no size of its own stretches instead, and centring costs it nothing.
    /// </remarks>
    private static void Centre(UIElement glyph)
    {
        if (glyph is FrameworkElement mark)
        {
            mark.HorizontalAlignment = HorizontalAlignment.Center;
            mark.VerticalAlignment = VerticalAlignment.Center;
        }
    }

    /// <summary>
    /// Brings the controls a strip hosts down to the height a title bar has room for.
    /// </summary>
    /// <remarks>
    /// The strip is as tall as the platform's own title bar — see <see cref="IslandsMetrics.TitleBarHeight"/> —
    /// which is less than a full control, and Islands states a second, tighter control height for exactly this
    /// case (<c>Button/minimumSize.compact</c>). It is applied as a local value, so it wins over the control
    /// height the hosted control's own style would otherwise give it. The caller keeps passing ordinary
    /// buttons; fitting them to the bar is the bar's business.
    /// </remarks>
    private static void FitIntoStrip(Element element)
    {
        if (element is Control control && element is not Panel)
        {
            // Both ends are stated: a control height on the style side sets MinHeight, and a minimum alone
            // leaves the control free to grow to whatever its container offers — which is how a 28 DIP button
            // ends up filling a 28 DIP bar. A local maximum pins it.
            control.MinHeight(IslandsMetrics.TitleBarControlHeight);
            control.MaxHeight(IslandsMetrics.TitleBarControlHeight);
        }

        if (element is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                FitIntoStrip(child);
            }
        }
    }

    /// <summary>
    /// Starts a window drag from a press on the strip, or maximizes when that press is a double-click.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A control inside the strip handles its own press first, so anything already handled is left alone: that
    /// is what keeps a click on a toolbar button from dragging the window instead of pressing the button.
    /// </para>
    /// <para>
    /// The second press has to be read here, before the drag starts, and not from the framework's double-click
    /// event. Two reasons, both about what the drag does to the pointer: a press spent on
    /// <see cref="Window.DragMove"/> is handed to the platform's own move loop, so the pair this handler is
    /// supposed to recognise is no longer its own sequence; and the double-click event is raised with a fresh,
    /// unhandled argument after the press, so it reaches the strip even when both presses landed on a window or
    /// toolbar button. Reading the count off the press keeps the strip's answer where its press already is, and
    /// keeps two clicks on a button as two clicks on a button.
    /// </para>
    /// </remarks>
    private void OnStripMouseDown(MouseEventArgs args)
    {
        if (args.Handled || args.Button != MouseButton.Left)
        {
            return;
        }

        _pendingStripToggle = false;
        if (args.ClickCount >= 2)
        {
            args.Handled = true;
            if (OperatingSystem.IsMacOS())
            {
                // AppKit owns the first press's native drag session. Let the second press finish
                // before changing the frame, so that session cannot move the newly maximized frame.
                _pendingStripToggle = true;
            }
            else
            {
                CompleteStripToggle();
            }
            return;
        }

        _window.DragMove();
    }

    private void OnStripMouseUp(MouseEventArgs args)
    {
        if (!_pendingStripToggle || args.Button != MouseButton.Left)
        {
            return;
        }

        _pendingStripToggle = false;
        if (args.Handled) return;
        args.Handled = true;
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher is not null)
        {
            dispatcher.BeginInvoke(DispatcherPriority.Background, CompleteStripToggle);
        }
        else
        {
            CompleteStripToggle();
        }
    }

    private void CompleteStripToggle()
    {
        ToggleMaximize();
        _placement.CorrectMaximizedPosition();
    }

    /// <summary>
    /// Brings the strip up to date once the window is on screen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Centered dialogs keep symmetric insets. A left-aligned workspace heading reserves native controls at
    /// the start; native controls at the end are reserved for actions in either alignment. Custom Windows
    /// captions keep their full strip height and meet the right window edge.
    /// </para>
    /// </remarks>
    internal void ApplyInsets()
    {
        // The window buttons, now that the platform can say whether it draws its own. macOS keeps its traffic
        // lights and reports NativeChromeButtons, so none of this happens there.
        bool drawCaptions = !_window.HasNativeChromeButtons;

        foreach (var captions in _captionRows)
        {
            captions.IsVisible(drawCaptions);
        }

        foreach (var (button, allowed) in _captionButtons)
        {
            button.IsVisible(drawCaptions && allowed());
        }

        // Rebuilding a title bar replaces its maximize button. Keep the one native hook, but always
        // point it at the current control, including while that control is waiting to join the visual tree.
        if (drawCaptions && _restore is { } restore)
        {
            _snapLayouts.UpdateTarget(restore.Surface, restore.SetHot, restore.SetPressed);
            _snapLayouts.TryInstall();
        }

        // The system's window buttons run to the window's own corner and span the whole strip, so a strip
        // that draws them drops its inset on those sides too. The vertical inset matters as much as the
        // horizontal one: a window button is as tall as the strip, and a strip whose content box is shorter
        // than itself makes such a button overflow. What that looks like is a hover fill a DIP or two short
        // of the strip's own edge, feathering into the band the island below carries on its top — which
        // reads as a fill that was not drawn cleanly rather than as a layout decision.
        foreach (var strip in _strips)
        {
            strip.Padding(TitlePadding(drawCaptions, _window.NativeChromeButtonInset,
                _leftAlignedStrips.Contains(strip)));
        }

        // The system backdrop, where the system has one. Mica is drawn behind the window and shows only where
        // the window paints nothing, so asking for it and keeping an opaque background would be asking for
        // nothing at all: the window and its strip step aside, and the islands — which are opaque — stay.
        // Stepping aside is a wash rather than a hole, so the window keeps its own colour over the material.
        if (!IslandsWindowBackdrop.Apply(_window.Handle))
        {
            return;
        }

        const byte tint = IslandsMetrics.BackdropTintAlpha;

        _window.WithTheme((theme, element) =>
            element.Background(IslandsTheme.Colors(theme).PanelBackground.WithAlpha(tint)));

        foreach (var strip in _strips)
        {
            strip.WithTheme((theme, element) =>
                element.Background(IslandsTheme.Colors(theme).ToolBarBackground.WithAlpha(tint)));
        }
    }

    /// <summary>Reserves native caption buttons when the workspace title occupies their side.</summary>
    internal static Thickness TitlePadding(bool drawCaptions, Thickness nativeInset, bool leftAligned)
    {
        if (drawCaptions)
        {
            return new Thickness(leftAligned ? IslandsMetrics.TitleBarPadding.Left : 0, 0, 0, 0);
        }

        var padding = IslandsMetrics.TitleBarPadding;
        if (!leftAligned)
        {
            // Reserve native controls on the trailing side symmetrically, keeping the title on the
            // window's center even on platforms that supply native buttons on the right.
            var inset = nativeInset.Right > 0 ? nativeInset.Right + IslandsMetrics.TitleBarGap : padding.Right;
            return new Thickness(inset, padding.Top, inset, padding.Bottom);
        }

        return new Thickness(
            nativeInset.Left > 0 ? nativeInset.Left + IslandsMetrics.TitleBarGap : padding.Left,
            padding.Top,
            nativeInset.Right > 0 ? nativeInset.Right + IslandsMetrics.TitleBarGap : padding.Right,
            padding.Bottom);
    }

    /// <summary>
    /// Lands the window in the middle of the region it asked to be centred on.
    /// </summary>
    internal void ApplyPlacement() => _placement.Place();
}

/// <summary>
/// Entry point for <see cref="IslandsWindowChrome"/>.
/// </summary>
public static class IslandsWindowChromeExtensions
{
    /// <summary>
    /// Creates chrome for a modal dialog with only a close button on every platform.
    /// Edge resizing remains governed by the window's size specification.
    /// </summary>
    /// <typeparam name="TWindow">Window type.</typeparam>
    /// <param name="window">The dialog to reshape, before showing it.</param>
    /// <returns>The chrome whose title strip can be added to the dialog layout.</returns>
    public static IslandsWindowChrome IslandsDialogChrome<TWindow>(this TWindow window)
        where TWindow : Window
    {
        ArgumentNullException.ThrowIfNull(window);

        // Native modal windows suppress these actions, but custom caption controls read the managed
        // capabilities. Set both explicitly so the controls and native window agree before realization.
        window.CanMinimize(false);
        window.CanMaximize(false);
        return window.IslandsFramelessChrome();
    }

    /// <summary>
    /// Gives up the platform's title bar: the client area takes it over, and the window's own strip becomes
    /// the title bar, the drag handle and the toolbar at once.
    /// </summary>
    /// <typeparam name="TWindow">Window type.</typeparam>
    /// <param name="window">The window to reshape.</param>
    /// <returns>
    /// The chrome, whose <see cref="IslandsWindowChrome.TitleBar(string, UIElement?)"/> builds the strip to dock at the top.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The window keeps its platform chrome buttons where the platform offers them — macOS keeps its traffic
    /// lights and hides the native title text — and takes its own elsewhere. The strip leaves room for them;
    /// see <see cref="IslandsWindowChrome"/> for why that is not knowable at build time.
    /// </para>
    /// <para>
    /// The top padding is dropped so the strip reaches the window's edge. The window's other insets are the
    /// theme's own container padding, so the window keeps exactly the breathing room it had.
    /// </para>
    /// </remarks>
    public static IslandsWindowChrome IslandsFramelessChrome<TWindow>(this TWindow window)
        where TWindow : Window
    {
        ArgumentNullException.ThrowIfNull(window);

        window.ExtendClientAreaTitleBarHeight(IslandsMetrics.TitleBarHeight);

        // Top flush, the rest of the frame keeps the theme's container inset. Resolved from the theme rather
        // than repeated as a number, so a custom metrics record moves the window with it.
        window.WithTheme((theme, element) => element.Padding(new Thickness(
            theme.Metrics.ContainerPadding.Left,
            0,
            theme.Metrics.ContainerPadding.Right,
            theme.Metrics.ContainerPadding.Bottom)));

        var chrome = new IslandsWindowChrome(window);

        // The traffic light width is a property of the realised window, so the inset is refreshed on load.
        window.Loaded += chrome.ApplyInsets;

        // So is the middle of the space the platform was asked to centre in, which is only knowable once the
        // window has a screen and a size — and which the platform answers with a biased one before that.
        window.Loaded += chrome.ApplyPlacement;

        return chrome;
    }
}

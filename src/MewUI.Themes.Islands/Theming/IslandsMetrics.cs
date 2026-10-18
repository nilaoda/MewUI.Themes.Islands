using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// Islands's New UI density translated into <see cref="ThemeMetrics"/>, plus the insets and radii
/// that have no slot in MewUI's metrics record and therefore have to be applied per style.
/// </summary>
public static class IslandsMetrics
{
    /// <summary><c>Button.margin</c> = <c>2,14,2,14</c> in Java insets order.</summary>
    public static Thickness ButtonPadding => Density.ButtonPadding;

    /// <summary><c>@componentMargin</c> / <c>TextField.margin</c> = <c>2,6,2,6</c>.</summary>
    public static Thickness ComponentPadding => Density.ComponentPadding;

    /// <summary>
    /// The air around a context-menu row: margin before the icon column, and the gap after the label.
    /// </summary>
    /// <remarks>
    /// Islands's own <c>MenuItem.margin</c> is <c>3,6,3,6</c> plus an icon column folded into the padding;
    /// that is right for an icon-less menu, but MewUI already reserves the icon column when items have
    /// icons, and this application's menus all do. The padding therefore stops folding the column in and
    /// leaves room for the menu's own inset instead.
    /// </remarks>
    public static Thickness MenuItemPadding { get; } = new(14, 5, 20, 5);

    /// <summary>
    /// The row height shared by the navigation tree and context menus, so a selected row and a selected
    /// menu item draw the same plate height.
    /// </summary>
    public const double NavigationRowHeight = 30;

    /// <summary><c>List.cellMargins</c> = <c>1,6,1,6</c>: rows are tighter than a standalone component.</summary>
    public static Thickness RowPadding => Density.RowPadding;

    /// <summary>
    /// <c>List|Tree/rowHeight</c> = 24 (compact 22): a list, tree, menu or drop-down row. Islands states no
    /// table row height, so a grid keeps the framework's own derivation (28).
    /// </summary>
    /// <remarks>
    /// MewUI derives a row from <c>BaseControlHeight</c> — <c>Max(18, height - 2)</c>, which is 26 against
    /// Islands's 28 DIP control — and the two numbers are separate keys in Islands, so a rule that owns a row
    /// control states this one itself. It matters most inside a drop-down, whose height is
    /// <c>rows × row height + chrome</c>.
    /// </remarks>
    public static double ListRowHeight => Density.ListRowHeight;

    /// <summary>
    /// The inset a drop-down's list keeps between its own edge and a row: <c>List.border</c> (<c>4,0,4,0</c>)
    /// for the vertical, <c>List.Button.leftRightInset</c> (8) for the horizontal, in MewUI's
    /// left/top/right/bottom order.
    /// </summary>
    /// <remarks>
    /// This is what makes a selected row a pill rather than a full-width band. MewUI has exactly one handle on
    /// it: a row's background plate is inflated back over the item padding (<c>ItemContainer.OnRender</c>), so
    /// <c>ItemPadding</c> cannot inset the plate — only the list's own <c>Padding</c> can, and it insets both
    /// axes. The vertical 4 is therefore the list border's and the horizontal 8 the row's own.
    /// </remarks>
    public static Thickness PopupContentPadding { get; } = new(8, 4, 8, 4);

    /// <summary><c>Tree.border</c> = <c>4,12,4,12</c>, in MewUI's left/top/right/bottom order.</summary>
    public static Thickness TreeContentPadding { get; } = new(12, 4, 12, 4);

    /// <summary>
    /// The inset of a window's bottom action strip. One value for every dialog: the button row should land
    /// at the same height whether the footer belongs to a small prompt or to the import wizard.
    /// </summary>
    public static Thickness DialogFooterPadding { get; } = new(20, 10, 20, 10);

    /// <summary>
    /// The inset a dialog's body keeps from the window edges and from the footer above it. One value for
    /// every dialog, so a prompt and the import wizard line their content up with the same margins.
    /// </summary>
    public static Thickness DialogContentPadding { get; } = new(20, 18, 20, 16);

    /// <summary><c>ToolBar.borderMargins</c> = <c>2,2,2,2</c>.</summary>
    public static Thickness ToolBarPadding { get; } = new(2, 2, 2, 2);

    /// <summary>
    /// The height of a window's own title strip, and the height the client area is extended by to make room
    /// for it. The two have to agree, because the platform treats the strip as the window's title bar for
    /// dragging and for the window buttons.
    /// </summary>
    /// <remarks>
    /// 32 DIP is Windows 11's own caption height, and that is the point rather than a coincidence: a control
    /// laid out in a shorter strip is a control with nothing around it. The traffic lights on macOS are
    /// positioned by the system, centred on the line a standard title bar would centre them on — 13.5 DIP
    /// from the top — so a taller strip centres its own content lower and leaves the lights slightly high.
    /// A shorter strip puts them level and leaves the controls crowded, and it was the crowding that read as
    /// wrong: the island below carries a border and a shadow, and in a strip only two DIP taller than its
    /// controls that band was most of the strip's breathing room. 32 gives both sides a share.
    /// </remarks>
    public const double TitleBarHeight = 32;

    /// <summary>
    /// The inset a title strip keeps at its own edges when the platform draws no window buttons in it. The
    /// vertical two leaves the strip's content box at <see cref="TitleBarControlHeight"/>.
    /// </summary>
    public static Thickness TitleBarPadding { get; } = new(12, 2, 12, 2);

    /// <summary>
    /// <c>Button/minimumSize.compact</c> height = 24: what a control takes when it has to live in a title
    /// strip, which is as tall as the platform's own title bar and therefore shorter than a full control.
    /// </summary>
    /// <remarks>
    /// Islands states this second, tighter control height for exactly this case; the strip applies it to the
    /// controls it hosts so that a caller passing ordinary buttons gets a bar that fits, rather than one whose
    /// buttons touch its edges.
    /// </remarks>
    public const double TitleBarControlHeight = 24;

    /// <summary>
    /// The width of one window button — minimize, maximize, close — on a platform that draws none.
    /// </summary>
    /// <remarks>
    /// 46 DIP is the system's own caption button width on Windows 11 at 100% (its three buttons span about 138),
    /// and a strip that draws them has to land on the same size to read as chrome rather than as a toolbar.
    /// </remarks>
    public const double CaptionButtonWidth = 46;

    /// <summary>How dark a window button goes while it is held down, on top of its hover fill.</summary>
    public const byte CaptionPressedAlpha = 0x29;

    /// <summary>
    /// How much of the panel's own colour is laid over the system backdrop, on a window that has one.
    /// </summary>
    /// <remarks>
    /// Deliberately not zero. A window that paints nothing at all over the backdrop shows the desktop's
    /// colours straight through, which reads as a hole in the interface rather than as a surface, and text on
    /// it changes contrast with whatever happens to be behind the window. A light wash of the panel colour —
    /// about an eighth — keeps the window recognisably this theme's while still letting the material show, and
    /// it is what the platform's own windows do. The islands stay opaque on top of it, so only the wall moves.
    /// </remarks>
    public const byte BackdropTintAlpha = 0x20;

    /// <summary>
    /// The air a title strip leaves next to the platform's window buttons. The inset itself comes from
    /// <see cref="Window.NativeChromeButtonInset"/>, which is the buttons' own extent; this is the gap on top
    /// of it, and it is used on both sides so the strip's ends look alike.
    /// </summary>
    public const double TitleBarGap = 12;

    /// <summary><c>Button.toolbar.margin</c> = <c>3,3,3,3</c> for a borderless toolbar button.</summary>
    public static Thickness ToolBarButtonPadding { get; } = new(3, 3, 3, 3);

    /// <summary>
    /// <c>Table.cellMargins</c>, with the horizontal widened: a code or identifier column otherwise touches
    /// the column line. See the deviation note in <c>docs/color-map.md</c>.
    /// </summary>
    public static Thickness GridCellPadding { get; } = new(8, 2, 8, 2);

    /// <summary><c>TabbedPane.tabInsets</c> = <c>4,12,4,12</c>.</summary>
    public static Thickness TabPadding => Density.TabPadding;

    /// <summary>
    /// Metrics to hand to <c>UseMetrics</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BaseControlHeight = 28</c> is Islands's control height (<c>Button/minimumSize = 72,28</c>), and it
    /// is what makes the density read like the New UI. MewUI derives a row height from it
    /// (<c>Max(18, BaseControlHeight - 2)</c> = 26), which is two more than Islands's own
    /// <c>List.rowHeight</c> (24) — the framework ties rows and controls to the one number, and Islands does
    /// not. Every rule that owns a row control therefore states <see cref="ListRowHeight"/> itself; the
    /// derivation is only what a control this theme does not cover falls back to.
    /// <c>ControlBorderThickness</c> goes 0.5 → 1 to match Islands's <c>Component.borderWidth</c>; it is
    /// an <c>AffectsLayout</c> property, so it only applies if installed before the first window shows.
    /// </para>
    /// <para>
    /// This sits below the insets it copies because static initializers run in declaration order: written
    /// above them, <c>ItemPadding</c> captured a <c>Thickness</c> that had not been built yet and every row
    /// the framework sizes from it came out flush against its own edge.
    /// </para>
    /// </remarks>
    public static ThemeMetrics Default { get; } = For(IslandsDensity.Regular);

    /// <summary>The metrics record for a density, ready to hand to <c>UseMetrics</c>.</summary>
    public static ThemeMetrics For(IslandsDensity density)
    {
        ArgumentNullException.ThrowIfNull(density);

        return ThemeMetrics.Default with
        {
            BaseControlHeight = density.BaseControlHeight,
            ControlCornerRadius = density.ComponentArc,
            ControlBorderThickness = 1,
            ContainerPadding = new Thickness(8),
            ItemPadding = density.RowPadding,
            FontFamily = ThemeMetrics.SystemFontFamily,
            FontSize = density.FontSize,
            FontSizeSmall = density.FontSizeSmall,
            FontSizeMedium = density.FontSizeMedium,
            ScrollBarThickness = 6,
            ScrollBarHitThickness = 10,
            ScrollBarMinThumbLength = 14,
            CommandIconSize = 16,
        };
    }

    /// <summary>
    /// The density the rules are reading. Islands states a compact set of the same tokens, and a style's
    /// own numbers are baked when the sheet builds it, so the density is installed before the rules are
    /// registered rather than looked up per frame. A theme is a single instance for the life of the
    /// application, which is what makes that safe.
    /// </summary>
    public static IslandsDensity Density { get; private set; } = IslandsDensity.Regular;

    /// <summary>Installs the density every rule reads from here on. Called once, by the theme's install path.</summary>
    internal static void UseDensity(IslandsDensity density)
    {
        ArgumentNullException.ThrowIfNull(density);

        Density = density;
    }

    /// <summary><c>Component/arc</c>: inputs, tabs, generic controls.</summary>
    public static double ComponentArc => Density.ComponentArc;


    /// <summary><c>Button/arc</c>.</summary>
    public static double ButtonArc => Density.ButtonArc;


    /// <summary><c>ProgressBar.arc</c> and the check / radio icon arc. Floating surfaces do not use it:
    /// their corners have to match <see cref="ThemeMetrics.ControlCornerRadius"/>, which is what the popup
    /// chrome blurs its shadow with.</summary>
    public const double SmallArc = 4;

    /// <summary>
    /// <c>TabbedPane.tabSelectionArc</c> = 4: the arc Islands gives a plate drawn <em>inside</em> a larger
    /// surface — a tab's selection, a list row's pill, a menu item's highlight.
    /// </summary>
    public const double SelectionArc = 4;

    /// <summary>
    /// <c>ToolTip.borderCornerRadius</c> = 6, the one popup radius Islands states outright. The theme files
    /// also carry <c>ToolTip.multilineBorderCornerRadius</c> = 8 for a tip that wraps onto several lines;
    /// MewUI gives a tool tip a single corner, so this is the one it takes.
    /// </summary>
    public const double ToolTipArc = 6;

    /// <summary>
    /// The arc of a drop-down's floating surface, one border width above <see cref="SelectionArc"/>.
    /// </summary>
    /// <remarks>
    /// Islands states no radius for a popup: the theme files give one only to a tool tip
    /// (<c>ToolTip.borderCornerRadius</c> = 6). MewUI hands a row the arc of the list it sits in, minus that
    /// list's border inset (<c>ItemsControl.UpdateItemCornerRadius</c>), so the popup's own corner is the only
    /// handle on the pill inside it, and this is that corner: the plate's arc plus the pixel MewUI subtracts.
    /// The chrome's blurred drop shadow keeps the metrics radius instead; see <c>docs/color-map.md</c>.
    /// </remarks>
    public static double PopupArc => SelectionArc + BorderThickness;

    /// <summary><c>ScrollBar.thumbArc = 999</c>: the thumb is a capsule.</summary>
    public const double CapsuleArc = 999;

    /// <summary><c>Component.borderWidth</c>.</summary>
    public const double BorderThickness = 1;

    /// <summary><c>Component.focusWidth</c>: the outer ring Islands switches on.</summary>
    public const double FocusRingThickness = 2;

    /// <summary><c>Button.minimumWidth</c>.</summary>
    public const double ButtonMinWidth = 72;

    /// <summary><c>TextField/minimumSize</c>: a field may be narrower than a button, but not below this.</summary>
    public const double ComponentMinWidth = 49;

    /// <summary><c>TabbedPane/tabHeight</c>.</summary>
    public static double TabHeight => Density.TabHeight;


    /// <summary><c>EditorTabs/underlineHeight</c>: the underline bar thickness.</summary>
    public static double TabUnderlineThickness => Density.TabUnderlineThickness;

    /// <summary><c>Island/arc</c>: the corner of a section panel.</summary>
    public static double IslandArc => Density.IslandArc;

    /// <summary><c>Island/borderWidth</c>: the ring of surrounding colour a section panel carries.</summary>
    public static double IslandBorderWidth => Density.IslandBorderWidth;

    /// <summary>
    /// The family a column of identifiers is set in; a list, so one value covers every platform.
    /// </summary>
    /// <remarks>
    /// Not part of <see cref="ThemeMetrics"/>: MewUI's metrics record carries the interface's own family and
    /// nothing else, and a theme is free to name a second one. See
    /// <see cref="IslandsDensity.MonospaceFontFamily"/> for why the value is a list.
    /// </remarks>
    public static string MonospaceFontFamily => Density.MonospaceFontFamily;


    /// <summary><c>TabbedPane.contentSeparatorHeight</c>.</summary>
    public const double TabContentSeparatorThickness = 1;

    /// <summary><c>TableHeader.height</c>.</summary>
    public const double GridHeaderHeight = 25;

    /// <summary><c>ProgressBar.horizontalSize</c> = <c>146,4</c>: the bar's height.</summary>
    public const double ProgressBarThickness = 4;

    /// <summary><c>Slider.thumbSize</c> = <c>12,12</c>.</summary>
    public const double SliderThumbSize = 12;

    /// <summary><c>MenuItem.minimumWidth</c> = <c>Button.minimumWidth</c>.</summary>
    public const double MenuMinWidth = 72;

    /// <summary><c>Separator.stripeWidth</c>.</summary>
    public const double SeparatorThickness = 1;
}

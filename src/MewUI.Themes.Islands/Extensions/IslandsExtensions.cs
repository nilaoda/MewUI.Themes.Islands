using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Ring = MewUI.Themes.Islands.Drawing.IslandsFocusRing;
using MewUI.Themes.Islands.Drawing;
using MewUI.Themes.Islands.Styles;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands;

/// <summary>
/// The semantic vocabulary an application writes its interface in, so that a screen names what a part of
/// it <em>is</em> rather than how many pixels and which hex it happens to use today.
/// </summary>
/// <remarks>
/// <para>
/// Every method returns the element it was called on, so these chain with MewUI's own fluent helpers:
/// <c>new TextBlock().Text(Ui.T(Msg.UiImportTitle)).IslandsTitle()</c>.
/// </para>
/// <para>
/// A method that only had to reach a type rule is deliberately absent: lists, buttons, fields and menus are
/// already Islands the moment <see cref="IslandsTheme"/> is installed, and an extension that set what the
/// style sets would only give a later editor two places to change.
/// </para>
/// </remarks>
public static class IslandsExtensions
{
    // ---- Buttons, by role ------------------------------------------------ -------------

    /// <summary>A borderless button: the chrome of a tool strip, a calendar header or a tab scroller.</summary>
    public static T IslandsButton<T>(this T button) where T : Button
    {
        button.StyleName = BuiltInStyles.FlatButton;
        return button;
    }

    /// <summary>The default button of a dialog, painted solid in the theme's accent.</summary>
    public static T IslandsPrimaryButton<T>(this T button) where T : Button
    {
        button.StyleName = BuiltInStyles.AccentButton;
        return button;
    }

    /// <summary>A button in a tool strip: the button's own colors with the toolbar's insets and no minimum width.</summary>
    /// <remarks>
    /// Deliberately no minimum width, so use it only for a fixed label or a glyph button. A label that can change
    /// at runtime — a search scope, a step, a counter — needs the standard button's <c>MinWidth</c> instead, or
    /// the control resizes with its text and everything beside it moves.
    /// </remarks>
    public static T IslandsCompactButton<T>(this T button) where T : Button
    {
        button.StyleName = IslandsStyleNames.CompactButton;
        return button;
    }

    /// <summary>The compact metric for a button with a menu, such as a list group's actions.</summary>
    public static DropDownButton IslandsCompactDropDownButton(this DropDownButton button)
    {
        button.StyleName = IslandsStyleNames.CompactDropDownButton;
        return button;
    }

    /// <summary>A flat drop-down for a panel header, matching the tool strip's buttons.</summary>
    public static DropDownButton IslandsToolbarDropDownButton(this DropDownButton button)
    {
        button.StyleName = IslandsStyleNames.ToolbarDropDownButton;
        return button;
    }

    /// <summary>An entry of a <see cref="ToolBar"/>, whose face belongs to the strip rather than to itself.</summary>
    public static T IslandsToolbarButton<T>(this T button) where T : Button
    {
        button.StyleName = BuiltInStyles.ToolBarButton;
        return button;
    }

    /// <summary>A toggle entry of a <see cref="ToolBar"/>, whose checked state tints the strip behind it.</summary>
    public static T IslandsToolbarToggleButton<T>(this T toggle) where T : ToggleButton
    {
        toggle.StyleName = BuiltInStyles.ToolBarToggleButton;
        return toggle;
    }

    // ---- Surfaces --------------------------------------------------------- -------------

    /// <summary>The panel surface, with no edge of its own: Islands's plain <c>Panel</c>.</summary>
    public static Border IslandsPanel(this Border border)
    {
        border.StyleName = IslandsSurfaceStyles.Panel;
        return border;
    }

    /// <summary>
    /// Islands' signature section panel: the layer-1 surface, edged, with the 20 DIP corner its name is
    /// about. Lay a window out of these and the application reads as islands on the background.
    /// </summary>
    public static Border IslandsSection(this Border border)
    {
        border.StyleName = IslandsSurfaceStyles.Island;
        return border;
    }

    /// <summary>
    /// A pane inside an island, on the island's own layer: use this rather than
    /// <see cref="IslandsPanel"/> for anything nested in a section, or it paints the window's layer into it.
    /// </summary>
    public static Border IslandsContent(this Border border)
    {
        border.StyleName = IslandsSurfaceStyles.Content;
        return border;
    }

    /// <summary>A raised plate: the component surface, edged and rounded like a text field.</summary>
    public static Border IslandsCard(this Border border)
    {
        border.StyleName = IslandsSurfaceStyles.Card;
        return border;
    }

    /// <summary>A strip of actions, laid on the panel with the toolbar's insets.</summary>
    public static Border IslandsToolbarSurface(this Border border)
    {
        border.StyleName = IslandsSurfaceStyles.Toolbar;
        return border;
    }

    /// <summary>The line under a window that reports what happened, laid on the panel.</summary>
    public static Border IslandsStatusBar(this Border border)
    {
        border.StyleName = IslandsSurfaceStyles.StatusBar;
        return border;
    }

    /// <summary>A window's bottom action strip: the same inset and hairline on every dialog.</summary>
    public static Border IslandsDialogFooter(this Border border)
    {
        border.StyleName = IslandsSurfaceStyles.DialogFooter;
        return border;
    }

    // ---- Text ------------------------------------------------------------- -------------

    /// <summary>A page title: the largest size the theme's type scale carries.</summary>
    public static TextBlock IslandsTitle(this TextBlock text) =>
        text.FontSize(ThemeFontSize.ExtraLarge);

    /// <summary>A heading over a group of fields: IntelliJ writes it in a bold body, not a larger body.</summary>
    public static TextBlock IslandsSectionTitle(this TextBlock text) =>
        text.FontWeight(FontWeight.Bold);

    /// <summary>
    /// An identifier — a code, an id, a timestamp — set so one character can be lined up against the one
    /// above it.
    /// </summary>
    /// <remarks>
    /// For a column of values that are compared character by character rather than read as words. Prose is
    /// deliberately left out of this: a monospaced family carries no CJK, so a title set in one hands its
    /// Chinese to whatever the platform happens to substitute, which is a different font on each machine.
    /// See <see cref="IslandsDensity.MonospaceFontFamily"/> for how one value covers Windows and macOS.
    /// </remarks>
    public static TextBlock IslandsMonospace(this TextBlock text) =>
        text.FontFamily(IslandsMetrics.MonospaceFontFamily);

    /// <summary>Text that explains rather than states, in Islands's secondary gray.</summary>
    /// <remarks>
    /// Secondary gray is a colour a reader is meant to read, which is why it is
    /// <see cref="IslandsVariantColors.SecondaryText"/> and not the disabled text this used to take. On the
    /// dark surfaces the disabled colour sits near 1.8:1 — correct for a control nobody can use, and invisible
    /// for a path under a heading.
    /// </remarks>
    public static TextBlock IslandsHint(this TextBlock text) =>
        text.WithTheme((theme, element) => element.Foreground(IslandsTheme.Colors(theme).SecondaryText));

    /// <summary>Text that navigates: Islands paints a link in the accent and leaves it undecorated.</summary>
    public static TextBlock IslandsLink(this TextBlock text) =>
        text.WithTheme((theme, element) => element.Foreground(IslandsTheme.Colors(theme).Link));

    /// <summary>Text that reports a failure, in the same red a field's validation border takes.</summary>
    public static TextBlock IslandsErrorText(this TextBlock text) =>
        text.WithTheme((theme, element) => element.Foreground(IslandsTheme.Colors(theme).Error));

    /// <summary>The label beside a field, at the size Islands uses for the text a control writes about itself.</summary>
    public static TextBlock IslandsFieldLabel(this TextBlock text) =>
        text.FontSize(ThemeFontSize.Small);

    // ---- Drawn behavior --------------------------------------------------- -------------

    /// <summary>Gives a grid's cells Islands's <c>Table.cellMargins</c>, so text does not sit on the column line.</summary>
    /// <remarks>
    /// A grid copies the theme's item padding into <c>CellPadding</c> while it is still being constructed,
    /// before it has a theme to copy, and — unlike a list, a tree or a menu — it never re-syncs that copy when
    /// the theme arrives. What it wrote is a local value, and a local value outranks a style, so no type rule
    /// can reach it: the insets have to be committed the same way the constructor committed its zeros.
    /// </remarks>
    public static GridView IslandsCellMargins(this GridView grid)
    {
        grid.CellPadding = IslandsMetrics.GridCellPadding;
        return grid;
    }

    /// <summary>Dress the tabs the way Islands does: each one a chip, not a bar under the label.</summary>
    public static TabControl IslandsTabs(this TabControl tabs) =>
        IslandsTabPlate.Apply(tabs);

    /// <summary>Draw Islands's outer focus halo while this control holds the keyboard focus.</summary>
    public static T IslandsFocusRing<T>(this T control) where T : Control
    {
        Ring.Apply(control);
        return control;
    }

    /// <summary>Draw Islands's triangle drop-down arrow in place of the framework's chevron.</summary>
    public static T IslandsDropDownArrow<T>(this T control) where T : Control
    {
        // Named in full: the method and the class it forwards to share a name.
        MewUI.Themes.Islands.Drawing.IslandsDropDownArrow.Apply(control);
        return control;
    }
}

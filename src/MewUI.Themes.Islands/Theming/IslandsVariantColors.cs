using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// One variant (light or dark) of the Islands color table.
/// Color roles used by the MewUI theme, mapped from the checked-in Islands reference JSON files.
/// Fields also include documented MewUI-specific values and platform caption colors.
/// </summary>
/// <remarks>
/// The record is <c>required</c>-initialized on purpose: a variant that forgets a color does not
/// compile, so a custom palette (Nord, Cobalt, …) cannot silently fall back to transparent black.
/// Derive a variant with <c>with</c> to reuse the whole style layer for another color table.
/// </remarks>
public sealed record IslandsVariantColors
{
    // ---- Surfaces ---------------------------------------------------------------------

    /// <summary><c>Panel.background</c>: the flat window/panel surface.</summary>
    public required Color PanelBackground { get; init; }

    /// <summary><c>TextField.background</c> / <c>@componentBackground</c>: inputs, lists, grids.</summary>
    public required Color ComponentBackground { get; init; }

    /// <summary><c>PopupMenu.background</c>: menus, popups, combo boxes dropdowns.</summary>
    public required Color MenuBackground { get; init; }

    /// <summary><c>ToolBar.background</c>: toolbars and status bars sit on the panel, not on a component.</summary>
    public required Color ToolBarBackground { get; init; }

    /// <summary><c>ScrollPane.background</c>: the area behind scroll tracks.</summary>
    public required Color ScrollTrackBackground { get; init; }

    /// <summary><c>ToolTip.background</c>.</summary>
    public required Color ToolTipBackground { get; init; }

    /// <summary><c>ToolTip.foreground</c>.</summary>
    public required Color ToolTipForeground { get; init; }

    // ---- Text -------------------------------------------------------------------------

    /// <summary><c>Label.foreground</c> / <c>@foreground</c>.</summary>
    public required Color Text { get; init; }

    /// <summary>
    /// <c>text-secondary</c>: the colour of text that explains rather than states.
    /// </summary>
    /// <remarks>
    /// A field of its own, and not <see cref="DisabledText"/>, because Islands states both and means
    /// different things by them. <c>text-secondary</c> is text the reader is supposed to read — a hint, a
    /// caption, an accelerator, the path under a heading — and it is chosen to stay legible on every surface
    /// the theme paints. <c>text-disabled</c> is chosen to look unreachable: on the dark panel it sits at
    /// roughly 1.8:1, which is right for a control nobody can use and useless for anything else. Taking the
    /// disabled colour for a hint is what makes a path disappear into the panel.
    /// </remarks>
    public required Color SecondaryText { get; init; }

    /// <summary><c>Label.disabledForeground</c> / <c>@disabledForeground</c>.</summary>
    public required Color DisabledText { get; init; }

    /// <summary><c>MenuItem.acceleratorForeground</c>.</summary>
    public required Color MenuAcceleratorText { get; init; }

    /// <summary><c>MenuItem.acceleratorSelectionForeground</c>.</summary>
    public required Color MenuAcceleratorSelectionText { get; init; }

    /// <summary><c>@icon</c> / <c>Table.sortIconColor</c>: monochrome icon tint.</summary>
    public required Color Icon { get; init; }

    /// <summary><c>@buttonArrowColor</c>: dropdown/disclosure arrows.</summary>
    public required Color Arrow { get; init; }

    /// <summary><c>ScrollBar.buttonDisabledArrowColor</c>.</summary>
    public required Color ArrowDisabled { get; init; }

    // ---- Borders and focus ------------------------------------------------------------

    /// <summary><c>Component.borderColor</c>.</summary>
    public required Color Border { get; init; }

    /// <summary><c>Component.disabledBorderColor</c>.</summary>
    public required Color BorderDisabled { get; init; }

    /// <summary><c>Component.focusedBorderColor</c>: the inner 1px focus edge.</summary>
    public required Color BorderFocused { get; init; }

    /// <summary><c>Component.focusColor</c>: the outer ring drawn by <c>focusWidth</c>.</summary>
    public required Color FocusRing { get; init; }

    /// <summary><c>PopupMenu.borderColor</c>.</summary>
    public required Color MenuBorder { get; init; }

    /// <summary>
    /// The fill a window's close button takes while it is lit.
    /// </summary>
    /// <remarks>
    /// Not an Islands key: Islands' IDE windows keep the platform's own window buttons, so its theme files never
    /// describe one. This is the system's own close colour on Windows 11 — the one place a window button leaves
    /// the theme's palette, because a close button that does not turn red is the one thing a Windows user reads
    /// as broken.
    /// </remarks>
    public required Color CaptionCloseHover { get; init; }

    /// <summary>
    /// <c>ComboPopup.border</c>: the edge of a drop-down's list, one step darker than a component's own.
    /// </summary>
    /// <remarks>
    /// Islands states this key as a border insets string with a literal hex — <c>1,1,1,1,#4C4F56</c> in the
    /// dark variant — rather than as a token, which is why it is the one color here that had to be read
    /// straight out of the theme files. The light variant states it only under <c>os.windows</c>
    /// (<c>#B5B7BD</c>); that is the value this port uses on every platform, so a light drop-down keeps the
    /// darker edge that makes it read as a floating surface.
    /// </remarks>
    public required Color ComboPopupBorder { get; init; }

    /// <summary><c>MenuItem.checkBackground</c>: the plate behind a checked menu item's icon.</summary>
    public required Color MenuCheckBackground { get; init; }

    /// <summary><c>Separator.foreground</c>.</summary>
    public required Color Separator { get; init; }

    // ---- Accent -----------------------------------------------------------------------

    /// <summary><c>Component.accentColor</c> / <c>@accentBaseColor</c>.</summary>
    public required Color Accent { get; init; }

    /// <summary>
    /// The text color fed to <see cref="ThemeSeed.WindowText"/>. It may differ from <see cref="Text"/>
    /// to balance MewUI's derived borders and row-label contrast. The default light variant uses
    /// <c>#3c3c3c</c>; control styles apply the explicit body-text color separately.
    /// See <c>docs/color-map.md</c> for accepted differences.
    /// </summary>
    public required Color SeedWindowText { get; init; }

    /// <summary><c>CheckBox.icon.checkmarkColor</c> (light variant of the accent family).</summary>
    public required Color AccentLight { get; init; }

    /// <summary><c>List.selectionForeground</c> on <c>@accentBaseColor</c>.</summary>
    public required Color AccentText { get; init; }

    /// <summary><c>Component.linkColor</c>.</summary>
    public required Color Link { get; init; }

    // ---- Selection --------------------------------------------------------------------

    /// <summary><c>List.selectionBackground</c> while the component owns the focus.</summary>
    public required Color Selection { get; init; }

    /// <summary><c>List.selectionForeground</c>.</summary>
    public required Color SelectionText { get; init; }

    /// <summary><c>List.selectionInactiveBackground</c>: cannot be derived from the accent.</summary>
    public required Color InactiveSelection { get; init; }

    /// <summary><c>List.selectionInactiveForeground</c>.</summary>
    public required Color InactiveSelectionText { get; init; }

    /// <summary><c>MenuBar.hoverBackground</c>: the row hover tint used by menus and toolbars.</summary>
    public required Color HoverBackground { get; init; }

    // ---- Buttons ----------------------------------------------------------------------

    /// <summary><c>Button.background</c> / <c>@buttonBackground</c>.</summary>
    public required Color ButtonBackground { get; init; }

    /// <summary><c>Button.foreground</c>.</summary>
    public required Color ButtonForeground { get; init; }

    /// <summary><c>Button.borderColor</c> (one step off <see cref="Border"/> in dark mode).</summary>
    public required Color ButtonBorder { get; init; }

    /// <summary><c>Button.hoverBackground</c>.</summary>
    public required Color ButtonHover { get; init; }

    /// <summary><c>Button.pressedBackground</c>.</summary>
    public required Color ButtonPressed { get; init; }

    /// <summary><c>Button.selectedBackground</c> / <c>ToggleButton.selectedBackground</c>.</summary>
    public required Color ButtonSelected { get; init; }

    /// <summary><c>Button.disabledBackground</c>.</summary>
    public required Color ButtonDisabledBackground { get; init; }

    /// <summary><c>Button.disabledForeground</c>.</summary>
    public required Color ButtonDisabledForeground { get; init; }

    /// <summary><c>ToggleButton.disabledSelectedBackground</c>: a checked toggle whose control is disabled.</summary>
    public required Color ToggleDisabledSelectedBackground { get; init; }

    /// <summary><c>Button.toolbar.hoverBackground</c>: borderless buttons that sit on a toolbar or tab strip.</summary>
    public required Color ToolbarHover { get; init; }

    /// <summary><c>Button.toolbar.pressedBackground</c>.</summary>
    public required Color ToolbarPressed { get; init; }

    /// <summary><c>Button.toolbar.selectedBackground</c> / <c>ToggleButton.toolbar.selectedBackground</c>.</summary>
    public required Color ToolbarSelected { get; init; }

    /// <summary><c>Button.default.background</c>: the default button background.</summary>
    public required Color PrimaryButtonBackground { get; init; }

    /// <summary><c>Button.default.foreground</c>.</summary>
    public required Color PrimaryButtonForeground { get; init; }

    /// <summary><c>Button.default.borderColor</c>.</summary>
    public required Color PrimaryButtonBorder { get; init; }

    /// <summary><c>Button.default.hoverBackground</c>.</summary>
    public required Color PrimaryButtonHover { get; init; }

    /// <summary><c>Button.default.pressedBackground</c>.</summary>
    public required Color PrimaryButtonPressed { get; init; }

    /// <summary><c>Button.default.focusedBorderColor</c>.</summary>
    public required Color PrimaryButtonFocusedBorder { get; init; }

    // ---- Grid -------------------------------------------------------------------------

    /// <summary><c>Table.gridColor</c>.</summary>
    public required Color GridLine { get; init; }

    /// <summary><c>TableHeader.separatorColor</c>.</summary>
    public required Color GridHeaderSeparator { get; init; }

    /// <summary><c>TableHeader.background</c>.</summary>
    public required Color GridHeaderBackground { get; init; }

    // ---- Tabs -------------------------------------------------------------------------

    /// <summary>
    /// <c>EditorTabs/underlinedTabBackground</c> = <c>tab-selected-bg-active</c>: the plate under the
    /// selected tab's label.
    /// </summary>
    /// <remarks>
    /// Islands claims a tab with a plate rather than with a bar: a rounded rectangle, filled and edged, that
    /// reads as a chip standing on the strip. The field is named for the token rather than for "selected"
    /// alone because the same plate is what the <c>underlinedTab*</c> family describes.
    /// </remarks>
    public required Color TabSelectedBackground { get; init; }

    /// <summary><c>EditorTabs/underlinedBorderColor</c> = <c>tab-selected-border-active</c>.</summary>
    public required Color TabSelectedBorder { get; init; }

    /// <summary>
    /// <c>EditorTabs/inactiveUnderlinedTabBackground</c> = <c>tab-selected-bg-inactive</c>: the same plate
    /// in a window that does not hold the focus.
    /// </summary>
    public required Color TabSelectedInactiveBackground { get; init; }

    /// <summary><c>EditorTabs/inactiveUnderlinedTabBorderColor</c> = <c>tab-selected-border-inactive</c>.</summary>
    public required Color TabSelectedInactiveBorder { get; init; }

    /// <summary><c>EditorTabs/underTabsBorderColor</c>: the 1px line under the tab strip.</summary>
    public required Color TabContentSeparator { get; init; }

    /// <summary><c>EditorTabs/hoverBackground</c> = <c>tab-bg-hovered</c>.</summary>
    public required Color TabHoverBackground { get; init; }

    /// <summary><c>TabbedPane.focusColor</c>: the tint behind a focused tab strip.</summary>
    public required Color TabFocusBackground { get; init; }

    // ---- Scroll bars ------------------------------------------------------------------

    /// <summary><c>ScrollBar.track</c>.</summary>
    public required Color ScrollTrack { get; init; }

    /// <summary><c>ScrollBar.thumb</c>.</summary>
    public required Color ScrollThumb { get; init; }

    /// <summary><c>ScrollBar.hoverThumbColor</c>.</summary>
    public required Color ScrollThumbHover { get; init; }

    /// <summary><c>ScrollBar.pressedThumbColor</c>.</summary>
    public required Color ScrollThumbPressed { get; init; }

    // ---- Progress ---------------------------------------------------------------------

    /// <summary><c>ProgressBar.background</c>.</summary>
    public required Color ProgressTrack { get; init; }

    /// <summary><c>ProgressBar.foreground</c>: the filled portion of the progress bar.</summary>
    public required Color ProgressFill { get; init; }

    /// <summary><c>ProgressBar.selectionBackground</c>: the label drawn over the unfilled track.</summary>
    public required Color ProgressText { get; init; }

    /// <summary><c>ProgressBar.selectionForeground</c>: the label drawn over the filled amount.</summary>
    public required Color ProgressTextOnFill { get; init; }

    // ---- Sliders ----------------------------------------------------------------------

    /// <summary><c>Slider.thumbColor</c> / <c>Slider.trackValueColor</c>.</summary>
    public required Color SliderFill { get; init; }

    /// <summary><c>Slider.hoverThumbColor</c>.</summary>
    public required Color SliderFillHover { get; init; }

    /// <summary><c>Slider.trackColor</c>: the unfilled remainder of the groove.</summary>
    public required Color SliderTrack { get; init; }

    // ---- Check boxes ------------------------------------------------------------------

    /// <summary><c>CheckBox.icon.borderColor</c>.</summary>
    public required Color CheckBorder { get; init; }

    /// <summary><c>CheckBox.icon.hoverBorderColor</c> / <c>CheckBox.icon.focusedBorderColor</c>.</summary>
    public required Color CheckBorderActive { get; init; }

    /// <summary><c>CheckBox.icon.disabledBorderColor</c>.</summary>
    public required Color CheckDisabledBorder { get; init; }

    /// <summary><c>CheckBox.icon.selectedBorderColor</c>: the edge of a checked box.</summary>
    public required Color CheckSelectedBorder { get; init; }

    /// <summary><c>CheckBox.icon.selectedBackground</c>: the plate of a checked box.</summary>
    public required Color CheckFilledBackground { get; init; }

    /// <summary><c>CheckBox.icon.checkmarkColor</c>: the check mark, or the radio's dot.</summary>
    public required Color CheckFilledForeground { get; init; }

    /// <summary><c>CheckBox.icon.disabledCheckmarkColor</c>.</summary>
    public required Color CheckDisabledMark { get; init; }

    // ---- Semantic ---------------------------------------------------------------------

    /// <summary><c>Component.error.focusedBorderColor</c>.</summary>
    public required Color Error { get; init; }

    /// <summary><c>Component.error.borderColor</c>.</summary>
    public required Color ErrorBorder { get; init; }

    /// <summary><c>Component.warning.focusedBorderColor</c>.</summary>
    public required Color Warning { get; init; }

    /// <summary><c>Component.warning.borderColor</c>.</summary>
    public required Color WarningBorder { get; init; }

    /// <summary><c>Component.success.focusedBorderColor</c>.</summary>
    public required Color Success { get; init; }

    /// <summary><c>Component.success.borderColor</c>.</summary>
    public required Color SuccessBorder { get; init; }
}

# Islands color map

How each color in `IslandsColors` is named by JetBrains, and which rule hands it to a control. The
authority is checked in at [`docs/islands/`](islands): `ManyIslandsLight.theme.json` and
`ManyIslandsDark.theme.json`, the theme files of the Islands family.

## The token model

Islands is not a table of component colors, it is a vocabulary. Three things follow from that, and they
are what make the port different from a one-to-one key translation.

**The ramps are variant-independent.** `gray-10…160`, `blue-10…160`, `red`, `green`, `yellow` — the same
hex in both themes. Only the semantic tokens resolve per variant, so a field's *light* and *dark* values
usually name the same token twice with different letters behind it.

**A surface is picked by layer, not by component.** `layer-0` is the application background, `layer-1` is
a panel or dialog, `layer-2` is content — the field you type into, the list you read. Against the previous
theme this inverts the dark variant: the *window* is the darkest layer and panels sit above it.

| Field | Token | Light | Dark |
| --- | --- | --- | --- |
| `PanelBackground` | `layer-1-bg` | `#f7f8f9` | `#191a1c` (that variant's `layer-0-bg`) |
| `ComponentBackground` | `dialog-bg-inline` / `layer-1-bg` | `#ffffff` (`layer-2-bg`) | `#26282c` |
| `MenuBackground` | `layer-2-bg` / `layer-1-bg` | `#ffffff` | `#26282c` |
| `ToolBarBackground` | `MainToolbar/background` | `#f7f8f9` (`layer-1-bg`) | `#191a1c` (`layer-0-bg`) |

**Hover and press are overlays, not colors.** `core-bg-transparent-hovered` is 7% black in light and 9%
white in dark; `core-bg-transparent-pressed` is 12.5% / 16%. They are kept as alpha here, so what they
composite over is the surface they land on rather than a guess baked in at build time. The scroll bar's
thumb is the same idea: a translucent grey capsule over nothing, with a fully transparent track.

## Text

| Field | Token | Light | Dark |
| --- | --- | --- | --- |
| `Text` | `text-default` | `#000000` | `#d1d3d9` |
| `SecondaryText` | `text-secondary` | `#73767c` | `#73767c` |
| `DisabledText` | `text-disabled` | `#9fa2a8` | `#4c4f56` |
| `MenuAcceleratorText`, `Icon`, `Arrow` | `text-secondary` / `gray-80` | `#73767c` | `#9fa2a8` |
| `Link` | `Link/*Foreground` | `#2f5eb9` (`blue-70`) | `#71a1fe` (`blue-100`) |
| `AccentText` | `text-over-accent` | `#ffffff` | `#ffffff` |
| `CaptionCloseHover` | not an Islands key — the system's own close red | `#C42B1C` | `#C42B1C` |

## Accent, selection, validation

| Field | Token | Light | Dark |
| --- | --- | --- | --- |
| `Accent`, `PrimaryButtonBackground`, `ProgressFill`, `SliderFill` | `accent-brand-bg` | `#3871e1` | `#3871e1` |
| `Selection`, `ButtonSelected`, `MenuCheckBackground`, `TabFocusBackground` | `blue-140` / `blue-50` | `#d0dffe` | `#2a4371` |
| `InactiveSelection` | `*/selectionInactiveBackground` (`gray-150` / `gray-40`) | `#e9eaee` | `#33353b` |
| `TabSelectedBackground` | `EditorTabs/underlinedTabBackground` = `tab-selected-bg-active` | `#e3ebfe` | `#233558` |
| `TabSelectedBorder` | `EditorTabs/underlinedBorderColor` = `tab-selected-border-active` | `#a7c5ff` | `#2e4d89` |
| `TabSelectedInactiveBackground` | `EditorTabs/inactiveUnderlinedTabBackground` = `tab-selected-bg-inactive` | `#e9eaee` | `#26282c` |
| `TabSelectedInactiveBorder` | `EditorTabs/inactiveUnderlinedTabBorderColor` = `tab-selected-border-inactive` | `#d1d3d9` | `#40434a` |
| `Error`, `Warning`, `Success` | `accent-*-bg` | `#e4656e` / `#c28013` / `#4e9d6c` | `#c54e58` / `#a56906` / `#338555` |
| `ErrorBorder`, `WarningBorder`, `SuccessBorder` | `accent-*-border-secondary` | `#ffc4c5` / `#f4cd9a` / `#bbdbc2` | `#80383e` / `#694820` / `#29583c` |

## Controls

| Area | Field ← token | Note |
| --- | --- | --- |
| Field and button edge | `Border` ← `Button/startBorderColor` (`gray-130` / `gray-50`) | `BorderFocused` and `FocusRing` are `accent-brand-border` |
| Button face | `ButtonBackground` ← `dialog-bg-inline` / the transparent `Button/endBackground` | In dark a button has no fill of its own and shows the layer under it |
| Button states | `ButtonHover`, `ButtonPressed`, `ToolbarHover`, `ToolbarPressed` ← `core-bg-transparent-*` | Overlays, alpha kept |
| Primary button | `PrimaryButton*` ← `Button/default/*` | `accent-brand-bg` with `text-over-accent` on it |
| Input | `ComponentBackground` ← `dialog-bg-inline` / `layer-1-bg` | Islands also raises `ComboBox/nonEditableBackground` one step (`gray-30`) |
| Tabs | `TabSelectedBackground` ← `tab-selected-bg-active`, `TabSelectedBorder` ← `tab-selected-border-active`, `TabHoverBackground` ← `tab-bg-hovered`; the plate is rounded by `TabbedPane.tabSelectionArc` (4) | `TabSelectedInactiveBackground` / `TabSelectedInactiveBorder` ← the `inactive*` pair, for a strip in a window without the focus |
| Rows | `HoverBackground` ← `*/hoverBackground`; `Selection` ← `*/selectionBackground` | `IslandsRowList` applies both directly; framework lists still derive their selection from the palette |
| Toolbar / menu icons | `Icon` ← `text-secondary`; `AccentText` over primary actions | Shared `IslandsIcon` geometry, colored at paint time |
| Grid | `GridLine` ← `Table/gridColor` | `layer-1-border-inline` / `layer-0-border` |
| Menu / popup | `MenuBackground` ← `Menu/background`, `MenuBorder` ← `Popup/borderColor` | A drop-down's list takes `ComboPopupBorder` instead; see below |
| Drop-down list | `ComboPopupBorder` ← `ComboPopup/border` | The one color the theme files state as a literal hex rather than as a token: `1,1,1,1,#4C4F56` dark, `1,1,1,1,#B5B7BD` light (light states it under `os.windows` only) |
| Tool tip | `ToolTipBackground` ← the dark chip Islands uses in both variants | |
| Check / radio | `CheckFilledBackground` and `CheckSelectedBorder` ← the accent; `CheckBorder` ← the icon's own stroke (`gray-120` / `gray-70`) | The mark's geometry comes from the icon, see `IslandsToggleIndicator` |
| Scroll bar | `ScrollThumb*` ← `ScrollBar/thumbColor` (35% / 55% / 75% grey); `ScrollTrack` ← `ScrollBar/trackColor`, fully transparent | |

## Deviations

- **The seed text is chosen for legibility, not for the border.** MewUI's private formulas derive
  `ControlBorder` as `WindowBackground.Lerp(WindowText, 0.23 dark / 0.21 light).Lerp(accent, 0.035 /
  0.05)`, and the same seed text is what a realized row label shows. The value that would land the border
  exactly on `gray-130` / `gray-50` is a mid grey (`#68605f` / `#c4c5cc`), which reads as washed-out
  labels, so the theme uses `#3c3c3c` / `#dddddd` and accepts a border nine (light) and six (dark)
  channels off. `SeedDerivationTests.BorderBudget` is that number.
- **Derived disabled text is a backstop only.** The framework derives `DisabledText` from the seeds; every
  control this theme styles restates it, so the derived value is allowed to sit 48 channels off
  (`Palette_only_derivations_stay_inside_the_documented_budget`).
- **A row is 24 DIP, and so is a grid's.** MewUI derives a row from `BaseControlHeight`
  (`Max(18, height - 2)` = 26) because it ties rows and controls to the one number and Islands does not, and
  it derives a *grid* row as `Max(BaseControlHeight, 24)` = 28. Every rule that owns a row control states
  `List|Tree/rowHeight` itself, so a list, a tree, a menu, a grid and a drop-down's list all land on 24 (22
  under `IslandsDensity.Compact`). Islands states no `Table.rowHeight`: the SDK's table reference images
  measure the old New UI's 22, and the table takes Islands's own row rhythm instead, which is the one it is
  read against beside the tree.
- **A table cell keeps more air than `Table.cellMargins` states.** The token is `2,3,2,3`, and a faithful
  reading leaves two or three DIP on every side; at that inset an identifier or a code sits against the column
  line. `IslandsMetrics.GridCellPadding` keeps the token's 2 vertical and widens the horizontal to 8, which
  is what the SDK's own table screenshots read as once their cell inset is counted. `GridCellMarginTests`
  pins whichever value the metric holds into the grid.
- **An overlay is composited onto the face it lands on.** Islands states hover and press as translucent
  overlays, and MewUI animates a colour change with a straight, unpremultiplied RGBA interpolation
  (`Color.Lerp`). Ramping an *opaque* face into a translucent overlay therefore passes through a
  half-transparent mid-tone: a white button dipped to `#c5c5c5` on its way into hover and back, which read as
  a flicker at the framework's 200 ms. Where the face is opaque the overlay is composited onto it first
  (`IslandsStyles.Overlay`), which leaves the settled fill exactly as Islands draws it and makes the ramp
  monotone. Where the face is already transparent — a toolbar button, a list row, a segment — both ends share
  a hue and the ramp is monotone on its own.
- **A button has no hover edge.** Islands states `Button.focusedBorderColor` and no `Button.hoverBorderColor`;
  `ActionButton.hoverBorderColor` exists, so the absence is deliberate rather than an omission. Only the fill
  moves on hover, and the edge is restated at its idle colour so the framework's own hover tint cannot show
  through this theme's state. The SDK's own `button_hovered.png` agrees: the hovered button there wears a
  plain grey edge.
- **A focused control wears the accent on its own 1px edge.** Islands takes `Component.focusedBorderColor` and
  `Button.focusedBorderColor` to the panel colour — the inner edge disappears — and expresses the focus with
  the 2 DIP `Component.focusWidth` ring in `accent-brand-border`. MewUI gives a control a single
  `BorderBrush` and draws that ring from an opt-in adorner (`IslandsFocusRing`), so a control that has not
  asked for the adorner would show no cue at all. The theme therefore keeps the accent on the edge: the same
  colour the ring uses, and always visible. That is why focus — and only focus — lights a button's edge.
- **A drop-down's list is rounded three DIP tighter than its own drop shadow.** MewUI gives a row plate the
  arc of the list it sits in, minus that list's border inset, so the list's corner is the only handle on the
  pill inside it — and Islands rounds a plate drawn *inside* a surface by `TabbedPane.tabSelectionArc` (4),
  not by `Component/arc`. The list therefore takes `Component/arc` less 3, while the chrome's blurred shadow,
  which reads its radius from the metrics record and nothing else, keeps 8. The disagreement sits on a blurred
  edge. The same coupling is why a menu's rows stay rounder than a drop-down's: a menu is read by its own
  silhouette, so it keeps the shadow's radius and its rows take 7.
- **A tool tip is rounded by Islands's own key, not by its shadow.** `ToolTip.borderCornerRadius` is 6 (8 for
  a tip that wraps); MewUI gives a tool tip a single corner and blurs its shadow with the metrics radius, so
  the tip sits two DIP off it. Its edge is still `Popup/borderColor`; Islands paints it in the chip's own
  color (`ToolTip.borderColor` = `feedback-bg`), which is to say with no visible edge at all.
- **The arrow column is 22 in the framework and 28 in Islands** (`Component/arrowAreaWidth`), and the
  framework strokes the chevron in the label's foreground where Islands uses its arrow token:
  `FlatDropDownArrow`-style overdrawing, see `IslandsDropDownArrow`.
- **A menu and a date picker round by `Component/arc` (8), not by a popup key.** Islands states no
  `Popup.borderCornerRadius` in the theme files, so those two surfaces take the value MewUI's metrics record
  holds — the same one the chrome blurs their shadow with — rather than a boxed constant that a custom
  `UseMetrics` would leave behind. A drop-down's list and a tool tip are the exceptions above.
- **Only a dark icon set ships.** `themes/expUI/icons/` has one `dark/` directory, so the light variant
  recolors the same shapes: the geometry is shared, and the light colors come from the light tokens.
- **No style handle at all** for the popup chrome's shadow, the scroll bar's track surface, the grid's
  header separators, a context menu's own separator and item plate, and the splitter thumb — the same list as
  before, with the same reasons. The menu's plate happens to land on Islands's light selection colour
  (`#cfddf7` against `#d0dffe`), which is luck rather than a wired token.
- **A `Separator` is not reachable either**, and for a blunter reason than the rest of that list: it is a
  `FrameworkElement`, not a `Control`, and `Style` refuses a target that is not a `Control`. It does draw
  Islands-adjacent colour — `Theme.Palette.ControlBorder`, which the seeds back-solve — but that is the
  border a control's edge takes, not `ToolBar.separatorColor` (`#00000020`) or
  `TableHeader.bottomSeparatorColor` (`dialog-border`), so a rule laid out with `new Separator()` reads a few
  steps darker than the theme's own hairlines. An application that wants the token uses a one-pixel `Border`
  the theme can reach.
- **`Menu` is not a control and not a gap.** It is the model handed to `DropDownMenu`/`ContextMenu`; the
  surface that presents it is `ContextMenu`, which the theme registers a rule for.
- **An island's border is its air, not a line on it.** `Island.borderWidth` is 6 and `Island.borderColor` is
  `tool-window-bg` — the surface the island stands on, not a stroke colour. The six DIP the border takes are
  therefore painted in the colour of whatever is behind the island and read as a gap: two islands side by
  side come out twelve apart and an island against a window edge keeps the same margin, neither of them
  knowing about the other. Drawing that border in the *border* colour instead — a one-pixel outline, which is
  what the theme did — gets the arc right and the spacing wrong, and two panes sharing a splitter meet edge
  to edge. `Island.arc` (20) stays the island's own corner; the plate inside the ring ends up rounder than
  the ring, which is what `Island.borderArcLength` (14) is measuring.
- **Islands states two grays below the body text, and they are not interchangeable.** `text-secondary` is
  text the reader is meant to read — a hint, a caption, an accelerator, the path under a heading — and it
  holds roughly 4.3:1 on the light panel and 3.2:1 on the dark one. `text-disabled` is chosen to look
  unreachable: the same panel puts it at **1.8:1**, which is right for a control nobody can use and
  invisible for anything else. `IslandsHint` took the disabled colour for a while, on the reasoning that a
  hint is "less important" than body text; the effect was that every path under a heading disappeared into
  the dark panel. `IslandsHint` now takes `SecondaryText`, and `TextContrastTests` holds every foreground
  the theme paints to a floor against the surface it lands on, so the pair cannot be swapped back by
  accident. Note that the light variant was over the line too, at 2.4:1 — the dark one was simply where it
  became impossible to ignore.
- **Reachable is not the same as faithful.** Everything above is about what a style cannot touch. The
  converse fault is quieter and was found only by comparing shapes: `TabControl` had a rule from the start
  and still drew the older New UI's underline rather than Islands's chip, because both are legitimate
  renderings of "the tab is selected" and only one of them is Islands. A rule's existence says nothing about
  the shape it paints; the tokens decide that, and a token like `tab-selected-bg-active` describes a *plate*
  rather than a bar.

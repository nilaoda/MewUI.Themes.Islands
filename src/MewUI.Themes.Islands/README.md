# MewUI.Themes.Islands

JetBrains' [Islands](https://plugins.jetbrains.com/docs/intellij/ui-overview.html) look for
[MewUI](https://github.com/aprillz/MewUI): its light and dark variants, carried over as a theme, a color
table, a set of metrics, per-control style rules and the few shapes MewUI draws itself. The authoritative
definition of both variants — every token, every component key — is checked in at
[`docs/islands/`](docs/islands), and how each of them is used is in
[`docs/color-map.md`](docs/color-map.md).

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/nilaoda/MewUI.Themes.Islands/main/docs/screenshots/overview-dark.png">
    <source media="(prefers-color-scheme: light)" srcset="https://raw.githubusercontent.com/nilaoda/MewUI.Themes.Islands/main/docs/screenshots/overview-light.png">
    <img alt="Islands overview" src="https://raw.githubusercontent.com/nilaoda/MewUI.Themes.Islands/main/docs/screenshots/overview-light.png" width="880">
  </picture>
</p>

Three things are worth knowing before the rest of this file:

- **Surfaces are picked by layer.** `layer-0` is the application background, `layer-1` a panel or dialog,
  `layer-2` content. `Border.IslandsSection()` is the signature *island* itself: a layer-1 panel with
  Islands' 20 DIP corner and Islands' 6 DIP border, which is painted in the colour of the surface behind it
  and therefore reads as air. Two islands side by side come out twelve apart, and an island against a window
  edge keeps the same margin, without the application arranging either.
- **Hover and press are translucent overlays**, not colors of their own, so they composite over whatever
  surface they land on.
- **There are two densities.** `IslandsDensity.Regular` carries the themes' own numbers (28 DIP controls,
  8 DIP arcs, 24 DIP rows); `IslandsDensity.Compact` carries their `.compact` keys (24, 6, 22). Pass one
  through `IslandsOptions.Density`.

The library is standalone. It references only `Aprillz.MewUI.Core` — never a platform backend, never an
application — so it can be packed, published and consumed on its own.

`IslandsDensity.MonospaceFontFamily` is a **list**, not a name:
`Cascadia Mono, Consolas, Menlo, Monaco, Courier New`. MewUI draws with the first family installed and takes
the characters it lacks from the ones after it, so one value covers every platform — Windows resolves
`Cascadia Mono` where the Terminal has installed it and `Consolas` everywhere else, and macOS falls through
both to `Menlo`, which is always there. Set prose in it at your peril: a monospaced family carries no CJK, so
a title set in one hands its Chinese to whatever the platform substitutes. It is for the columns that hold
codes, ids and timestamps.

## Install

```csharp
Application.Create()
    .UseIslands()          // before BuildMainWindow(): the style sheet freezes on first use
    .BuildMainWindow(() => new MainWindow())
    .Run();
```

`UseIslands()` seeds both variants, installs the metrics, registers every style rule into the application
style sheet, and keeps `Application.Accent` pointed at the accent of whichever variant is active. Switching
theme mode repaints everything, including the accent:

```csharp
application.SetThemeMode(ThemeMode.Dark);   // Darcula
application.SetThemeMode(ThemeMode.Light);  // IntelliJ Light
```

## The colors are the point

Islands resolves thousands of `UIDefaults` keys down to a few hundred colors at startup. This library does
not approximate that algebra: every value in [`IslandsColors.cs`](IslandsColors.cs) is read verbatim out of the
upstream resolved dumps for `FlatLightLaf`, `FlatDarkLaf` and the `FlatIntelliJLaf` / `FlatDarculaLaf`
deltas. [`IslandsVariantColors`](IslandsVariantColors.cs) gives each one a name that carries its Islands key, so a
custom palette (Nord, Cobalt, a brand) is one record to fill in:

```csharp
var nord = IslandsColors.Dark with
{
    Accent = Color.FromHex("#5e81ac"),
    // …
};

Application.Create().UseIslands(new IslandsOptions { Palette = new IslandsPalette(IslandsColors.Light, nord) });
```

[`docs/color-map.md`](docs/color-map.md) lists which Islands key became which MewUI property, and — in
detail — which Islands behaviors a style layer cannot reach.

## Writing an interface in it

Most controls need nothing: a `Button`, `TextBox`, `ListBox`, `GridView`, `TreeView`, `TabControl`,
`ContextMenu` or `Slider` is Islands the moment the theme is installed, because the theme registers a type
rule for each of them. What is left is the vocabulary that a rule cannot express:

```csharp
new Border().IslandsCard().Child(
    new StackPanel()
        .Vertical()
        .Spacing(6)
        .Children(
            new TextBlock().Text("Create project").IslandsTitle(),
            new TextBlock().Text("Everything is saved locally.").IslandsHint(),
            new Separator(),
            new Button().Content("Run").IslandsPrimaryButton()));
```

| Method | Meaning |
| --- | --- |
| `.IslandsButton()` | borderless — the chrome of a tool strip or a tab scroller |
| `.IslandsCompactButton()` | button colors, toolbar insets, no minimum width |
| `.IslandsPrimaryButton()` | the primary button appearance, solid in the accent |
| `.IslandsToolbarButton()`, `.IslandsToolbarToggleButton()` | an entry of a `ToolBar` |
| `.IslandsPanel()` | the plain panel surface, no edge |
| `.IslandsCard()` | component surface, edged and rounded like a text field |
| `.IslandsToolbarSurface()`, `.IslandsStatusBar()` | the strip surfaces, on the panel |
| `.IslandsDialogFooter()` | the bottom action strip of a dialog, with one inset and one hairline everywhere |
| `.IslandsTitle()`, `.IslandsSectionTitle()`, `.IslandsFieldLabel()`, `.IslandsHint()` | the type scale |
| `.IslandsMonospace()` | a column of identifiers, set in the theme's cross-platform monospaced stack |
| `.IslandsLink()`, `.IslandsErrorText()` | accent-colored navigation, validation red |
| `.IslandsTabs()` | claim the selected tab with Islands's chip — a filled, edged rounded plate |
| `.IslandsFocusRing()` | the outer 2 DIP focus halo of `Component.focusWidth` |

## Workspace components

The package also supplies the parts of an Islands workspace that do not belong to an application:

```csharp
var workspace = new IslandsWorkspace(
    navigation: navigationIsland,
    content: contentIsland,
    toolRail: IslandsToolbar.Rail(topTools, bottomTools),
    navigationWidth: 224);

var header = IslandsToolbar.Header(context, actions);
var copy = IslandsToolbar.IconButton(IslandsIconKind.Copy, localizedCopyLabel);
var create = IslandsToolbar.PrimaryActionButton(IslandsIconKind.Plus, localizedCreateLabel);
```

`IslandsWorkspace.Element` contains a draggable `SplitPanel`; `Panes` exposes its position for host-owned
persistence. Sizes are device-independent and the layout uses the same MewUI controls on Windows, macOS
and Linux. The tool rail has independent top and bottom groups. Pane headers keep flexible context beside
content-sized actions. The host supplies every label, tooltip, command and pane.

`IslandsRowList` mixes non-selectable headers with selectable row objects. It supports Ctrl/Cmd toggles,
Shift ranges, arrow/Home/End navigation, context-menu requests and selection restoration through `SelectMany`.
Pass `allowMultipleSelection: false` for an activity browser with one current item. `Items` and
`SelectedItems` are returned in display order; `Clear` clears both rows and selection. Use it for moderate
mixed-content lists; for large flat datasets use MewUI's virtualized `GridView` instead. Row objects must be
unique within the list. Row selection and hover colors follow theme changes without host rebuilding.

`IslandsActivity.Row` places a summary beside a small timestamp and a shared vector icon.
`IslandsActivity.Property` aligns an inspector label with host-supplied value content, including wrapped
text or a before/after comparison. These layouts contain no activity model, field names or application logic.

`IslandsToolbar.DisclosureButton` supplies the header action for host-owned collapsible groups. The host
owns expanded state and which rows are displayed. `IslandsIcon` contains shared monochrome geometry used
by toolbars and menus; primary actions use the contrasting icon color over the accent.

`IslandsWindowChrome.TitleBar(title, actions)` centers the title on the window independently of trailing
actions. Symmetric title insets preserve that position with native or self-drawn caption buttons. On
Windows, caption buttons retain their full height and right edge, including maximize/snap layout behavior.
The explicit `HorizontalAlignment.Left` overload remains available for workspaces that need it; on macOS,
its heading starts after the native traffic lights.

Use `window.IslandsDialogChrome()` for modal windows. It disables minimize and maximize before
the window is shown, keeping native macOS controls and custom Windows captions consistent. The close
button remains available, and a resizable dialog can still be resized from its edges.

Use `window.IslandsDefaultAction(Confirm, () => confirmButton.IsEffectivelyEnabled)` to opt into
Enter confirmation, including the macOS numeric keypad Enter key. Reuse the button's validation callback.
Handled keys, modifier chords, repeated presses and active IME composition are ignored; focused buttons and multiline editors keep their
own Enter behavior. `IslandsPrimaryButton()` only sets the visual style.

## Requirements

`net8.0` and `net10.0` assets. The library is `IsAotCompatible` and trims without warnings.

## License

MIT — see [`LICENSE`](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/LICENSE). Islands is by FormDev Software GmbH and is used here only as the reference
for its published color values.

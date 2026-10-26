# MewUI.Themes.Islands

An independent Islands-inspired theme for [MewUI](https://github.com/aprillz/MewUI),
with light and dark palettes, regular and compact density, per-control styles,
and reusable workspace, menu and window components.

<p align="center">
  <img alt="Islands theme overview" src="https://raw.githubusercontent.com/nilaoda/MewUI.Themes.Islands/main/docs/screenshots/overview-light.png" width="880">
</p>

## Preview status and compatibility

Version `0.1.0-preview.4` fixes unintended Windows window movement when clicking the title bar
with a dropdown open. It also vertically centers title-bar controls, giving dropdowns and toolbar
buttons balanced space above and below.
Public APIs may change between preview releases.
It is built and tested against MewUI `0.22.1`; later MewUI versions need separate verification,
especially for window integration and dialog keyboard routing.

The package targets **`net8.0` and `net10.0`**, matching the assets in
`Aprillz.MewUI.Core` and `Aprillz.MewUI` 0.22.1. It depends only on `Aprillz.MewUI.Core`.
The host application selects its platform and rendering backend.

To build the library from source or run the demo and tests, see the
[repository build instructions](https://github.com/nilaoda/MewUI.Themes.Islands#build-test-and-pack).

## Install

In a MewUI application:

```sh
dotnet add package Aprillz.MewUI --version 0.22.1
dotnet add package MewUI.Themes.Islands --version 0.1.0-preview.4
```

If the application already uses MewUI's individual platform/backend packages, keep that setup
and add only the theme package. The theme does not choose a backend or include one itself.

## Quick start

This complete macOS example uses MewUI's macOS platform and Metal renderer:

```csharp
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.Create()
            .UseMacOS()
            .UseMewVGMetal()
            .UseIslands()
            .BuildMainWindow(() => new Window()
                .Title("Islands preview")
                .Resizable(640, 420)
                .Content(new StackPanel().Vertical().Spacing(12).Children(
                    new TextBlock().Text("Hello Islands").IslandsTitle(),
                    new Button().Content("Close").IslandsPrimaryButton()
                        .OnClick(Application.Shutdown))))
            .Run();
    }
}
```

For Windows, replace `.UseMacOS().UseMewVGMetal()` with `.UseWin32().UseDirect2D()`.
For Linux/X11, use `.UseX11().UseMewVGX11()`. These are backend registration examples;
see the [demo startup](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/src/MewUI.Themes.Islands.Demo/Program.cs)
for platform selection that keeps backend registration in separate methods for trimming.

Call `UseIslands()` before the first window is built. Switch the active variant with:

```csharp
application.SetThemeMode(ThemeMode.Dark);   // Islands Dark
application.SetThemeMode(ThemeMode.Light);  // Islands Light
```

### Custom startup callbacks

In MewUI 0.22.1, `OnStartup` replaces the previous callback. If your application needs one,
install the theme inside that callback, before constructing windows or resolving styles:

```csharp
builder
    .UseIslands(options)
    .OnStartup(() =>
    {
        IslandsTheme.Install(options);
        // Additional application startup work.
    })
    .BuildMainWindow(() => new MainWindow())
    .Run();
```

Use the same `IslandsOptions` instance for both calls. `UseIslands` configures palette seeds
and metrics; `Install` registers the styles and accent synchronization after the application starts.

## Namespaces

```csharp
using MewUI.Themes.Islands;           // Theme entry points, options and all fluent extensions
using MewUI.Themes.Islands.Controls;  // Workspace, toolbar, row list, activity and icons
using MewUI.Themes.Islands.Menus;     // Context menus and menu items
using MewUI.Themes.Islands.Theming;   // Palettes, color tables, density and metrics
using MewUI.Themes.Islands.Windowing; // Window chrome
```

`Drawing` contains optional drawing helpers. `Styles` contains internal style registration.

## Palettes and density

The default palettes use the checked-in `ManyIslandsLight.theme.json` and
`ManyIslandsDark.theme.json` reference files. Colors are mapped to MewUI styles and palette seeds;
this is an adaptation, not a complete implementation of the JetBrains UI toolkit.
The [color map](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/docs/color-map.md)
documents mappings and known differences.

```csharp
var palette = IslandsPalette.Default;
var customDark = palette.Dark with { Accent = Color.FromHex("#5e81ac") };
var options = new IslandsOptions
{
    Density = IslandsDensity.Compact,
    Palette = new IslandsPalette(palette.Light, customDark),
};

builder.UseIslands(options);
```

`IslandsDensity.Regular` uses 28 DIP controls and 24 DIP list/tree rows;
`Compact` uses 24 DIP controls and 22 DIP rows. All sizes are device-independent.
By default the accent follows the active palette variant; set `SyncAccentWithVariant = false`
to manage it yourself.

`IslandsMonospace()` uses a cross-platform font fallback list for code-like values.
It is intended for identifiers, timestamps and code, rather than general interface text.

## Controls and fluent styling

Installing the theme registers styles for MewUI controls. Extensions select semantic roles:

```csharp
new Border().IslandsCard().Child(
    new StackPanel().Vertical().Spacing(8).Children(
        new TextBlock().Text("Create project").IslandsTitle(),
        new TextBlock().Text("Choose a folder to continue.").IslandsHint(),
        new Button().Content("Continue").IslandsPrimaryButton()));
```

| API | Purpose |
| --- | --- |
| `IslandsPanel`, `IslandsSection`, `IslandsContent`, `IslandsCard` | Surface hierarchy |
| `IslandsPrimaryButton`, `IslandsToolbarButton`, `IslandsCompactButton` | Button roles |
| `IslandsTitle`, `IslandsSectionTitle`, `IslandsFieldLabel`, `IslandsHint` | Text roles |
| `IslandsLink`, `IslandsErrorText`, `IslandsMonospace` | Specialized text |
| `IslandsTabs`, `IslandsFocusRing`, `IslandsDropDownArrow`, `IslandsCellMargins` | Additional control styling |
| `IslandsWorkspace`, `IslandsToolbar` | Split workspace, tool rail and pane headers |
| `IslandsSearchField`, `IslandsSearchToolbar` | Search icon, optional inline scope control and responsive toolbar layout |
| `IslandsRowList` | Headers mixed with selectable rows, keyboard navigation and selection restoration |
| `IslandsActivity`, `IslandsProductInfo`, `IslandsIcon` | Reusable presentation components |
| `IslandsContextMenu`, `IslandsMenuItem` | Menus hosted in the owner window's overlay |

The host supplies application data, labels, commands and persistence. `IslandsWorkspace.Panes`
exposes the splitter for host-owned position persistence. `IslandsRowList` is intended for moderate
mixed-content lists; use MewUI's virtualized `GridView` for large flat datasets.

### Search and toolbar composition

`IslandsSearchField` wraps a standard `TextBox` with a decorative search icon and an optional
scope control inside its outline. The caller supplies bindings, localization and search behavior.
`IslandsSearchToolbar` places the field beside its actions when they fit, or above them otherwise.
Use a `WrapPanel` for actions that should also wrap in narrow panes.

```csharp
var input = new TextBox().Placeholder("Search records");
var filterLabel = new TextBlock().Text("Filter");
var filter = IslandsToolbar.MenuButton(IslandsIconKind.Filter, filterLabel);
var actions = new WrapPanel { Spacing = 4 };
actions.Add(filter);
var toolbar = new IslandsSearchToolbar(new IslandsSearchField(input), actions);

// Update the label without replacing the icon and chevron.
filterLabel.Text = "Filter · 1";
```

`IslandsIconKind` also includes `Search`, `Sort`, `Layers` and `ArrowLeft` for workspace navigation.

## Window and dialog integration

`window.IslandsFramelessChrome()` returns a chrome object whose `TitleBar(title, actions)` builds
a draggable title strip. Titles default to centered alignment. macOS keeps native traffic-light
buttons; Windows uses custom caption buttons with maximize/snap integration.

`window.IslandsDialogChrome()` disables minimize and maximize before showing a modal dialog.
Closing remains available, and resizing follows the window's size settings.

`window.IslandsDefaultAction(Confirm, () => confirmButton.IsEffectivelyEnabled)` opts into Enter
confirmation, including macOS keypad Enter. Handled events, modifier chords, repeated key presses
and active IME composition are ignored. Focused buttons and multiline editors retain their own
Enter behavior. `IslandsPrimaryButton()` changes appearance only.

## Framework workarounds

The theme includes compatibility fixes for MewUI 0.22.1: low-DPI Windows Direct2D text rendering,
initial Windows centering of custom-titlebar windows, geometric macOS centering, macOS title-strip
maximization, inactive-owner hover cleanup, and macOS keypad Enter routing. The Windows placement correction is new in preview.3 and still needs
manual validation on Windows. Implementations, affected paths and removal conditions are listed in the
[repository workaround inventory](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/docs/framework-workarounds.md).

Starting with preview.4, a press on the draggable title area on Windows dismisses the open dropdown
and is consumed;
the next press can drag the window normally. Caption buttons and toolbar controls keep their normal behavior.

### Windows text rendering

The NuGet package automatically patches a build-output copy of `Aprillz.MewUI.Backend.Direct2D`
for Windows targets. At 100% scaling it uses DirectWrite system rendering parameters; above 100%
it preserves MewUI's original smoothing. The choice follows each render target's DPI, including
moving a window between monitors. NuGet cache assemblies and font settings are not modified.

Both the backend NuGet version (when available) and assembly version must match **0.22.1**.
An unknown version fails the build with an upgrade instruction. To disable this workaround:

```xml
<PropertyGroup>
  <IslandsWindowsTextRenderingWorkaround>false</IslandsWindowsTextRenderingWorkaround>
</PropertyGroup>
```

The default value is `auto`: Windows RIDs (including cross-publishing), or a Windows build host
when no RID is specified. `true` forces the patch on any host for headless integration checks.
Core-only projects without a copy-local Direct2D backend are skipped. The package does not choose
the application's backend; automatic detection is based on the Direct2D assembly being present.

The bundled build tool runs on .NET 8 or a later installed major runtime. Consumers do not build
the tool or restore Mono.Cecil separately. These build-only assets are excluded from application
output; the patched backend supports ordinary builds, single-file publishing, trimming and NativeAOT.
Building through a source `ProjectReference` does not import NuGet `buildTransitive` assets.

## Validation scope

- Both target-framework assets are built and checked through an independent NuGet consumer.
- Automated tests run on .NET 10 and cover palettes, style rules, components and window/input helpers.
- Windows and macOS integration have been exercised in a consuming application. Linux/X11 has not
  yet received the same manual validation.
- `IsAotCompatible` enables trimming and AOT analyzers. It is not a guarantee for every platform/RID;
  validate the host application's NativeAOT publish and runtime behavior on each intended target.

Reference data and the demo are not included in the NuGet package.

## AI-assisted development

This project is developed through collaboration across multiple AI models and tools
for implementation, refactoring, documentation and test development. The maintainer
is responsible for project direction and release decisions.

## References and license

The MewUI theme implementation is MIT licensed; see
[LICENSE](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/LICENSE).
The reference theme data and adapted palette values come from JetBrains under Apache 2.0;
their role and exact upstream snapshot are recorded in
[reference notes](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/docs/islands/README.md).
The package declares `MIT AND Apache-2.0` and includes both license texts and
[third-party notices](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/THIRD-PARTY-NOTICES.md).
This is an independent community adaptation, not an official JetBrains or MewUI package.
It is not affiliated with or endorsed by JetBrains or the MewUI maintainers.

# MewUI.Themes.Islands

[![NuGet version](https://img.shields.io/nuget/vpre/MewUI.Themes.Islands?label=NuGet&color=2563eb&logo=nuget&logoColor=white)](https://www.nuget.org/packages/MewUI.Themes.Islands)
[![NuGet downloads](https://img.shields.io/nuget/dt/MewUI.Themes.Islands?color=2563eb&logo=nuget&logoColor=white)](https://www.nuget.org/packages/MewUI.Themes.Islands)
[![Status: Preview](https://img.shields.io/badge/status-preview-d97706)](#preview-status)
[![.NET 8 and .NET 10](https://img.shields.io/badge/.NET-8%20%7C%2010-512bd4?logo=dotnet&logoColor=white)](src/MewUI.Themes.Islands/MewUI.Themes.Islands.csproj)

An independent Islands-inspired theme for [MewUI](https://github.com/aprillz/MewUI).
It provides light and dark palettes, regular and compact density, control styles,
and reusable workspace, menu and window components.

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/screenshots/overview-dark.png">
    <source media="(prefers-color-scheme: light)" srcset="docs/screenshots/overview-light.png">
    <img alt="Islands theme overview" src="docs/screenshots/overview-light.png" width="880">
  </picture>
</p>

## Preview status

The current source version is `0.1.0-preview.3`, fixing initial Windows placement and low-DPI Direct2D text rendering.
See the NuGet badge above for the latest published version. Public APIs may change between preview releases.
The current dependency and integration baseline is MewUI `0.22.1`.

The library targets `net8.0` and `net10.0`, matching both `Aprillz.MewUI.Core` and
`Aprillz.MewUI` 0.22.1.

See the [package README](src/MewUI.Themes.Islands/README.md) for installation,
startup examples, namespaces, components and validation scope.

## Layout

- `docs/` — color mapping, reference theme data and screenshots
- `src/MewUI.Themes.Islands/` — the packable library
- `src/MewUI.Themes.Islands.Tests/` — automated tests
- `src/MewUI.Themes.Islands.Demo/` — the component gallery
- `tools/` — the packaged Windows text patch tool and its integration checks

The solution and shared build configuration remain under `src/`.
The library groups palette and metrics in `Theming/`, reusable components in `Controls/`,
menus in `Menus/`, window behavior in `Windowing/`, style rules in `Styles/`, and drawing helpers
in `Drawing/`. Fluent entry points live in `Extensions/` and retain the root namespace.

## Build, test and pack

Building from source requires the .NET 10 SDK configured in [src/global.json](src/global.json).
The demo and automated tests target .NET 10; the library builds for both .NET 8 and .NET 10.

```sh
cd src
dotnet build MewUI.Themes.Islands.slnx -c Release
dotnet run --project MewUI.Themes.Islands.Tests -c Release
dotnet pack MewUI.Themes.Islands/MewUI.Themes.Islands.csproj -c Release
```

The NuGet package contains assemblies and XML API documentation for both target frameworks,
the package README, LICENSE, third-party notices and license texts. It also ships
`buildTransitive` targets and a .NET 8 build tool (with Mono.Cecil); these are build-only assets,
not application runtime dependencies.
A separate `.snupkg` contains debugging symbols.
Reference JSON files and the demo are not included in the package.

## Demo

```sh
cd src
dotnet run --project MewUI.Themes.Islands.Demo -- \
  --theme dark --density compact --page controls
```

`--theme` accepts `system`, `light` or `dark`; `--page` accepts `overview`, `controls`,
`lists & trees`, `dialogs & menus`, `window chrome` or `colors & metrics`.

## Framework workarounds

The current baseline is **MewUI 0.22.1**. Window fixes are enabled by
`IslandsFramelessChrome()` / `IslandsDialogChrome()`, and keypad handling is opt-in through
`IslandsDefaultAction()`. They use static interop compatible with trimming and NativeAOT.
The Direct2D text fix runs at build time automatically for Windows consumers of the NuGet package;
it needs no theme startup call. The library retains only a Core runtime dependency.

| Behavior | Implementation | When and how to remove |
|---|---|---|
| Small Chinese text loses thin strokes at 100% scaling; using system rendering parameters globally reduces smoothing at high DPI. | [Text patch tool](tools/MewUITextRenderingPatch/README.md) and [packaged targets](src/MewUI.Themes.Islands/buildTransitive/MewUI.Themes.Islands.targets). Patches an `obj` copy of Direct2D 0.22.1: system parameters at 96 DPI, original smoothing above 96 DPI, selected per render target. NuGet cache files are unchanged. | Rejects unverified backend versions at build time. After upstream fixes both low/high DPI, set `IslandsWindowsTextRenderingWorkaround=false`, verify ordinary, single-file and NativeAOT output, then remove the targets, tool and bundled Mono.Cecil assets/notices in a new theme release. |
| Windows startup appears above and left of center because the backend includes a non-client frame for a custom-titlebar window. | [IslandsWindowPlacement.Windows.cs](src/MewUI.Themes.Islands/Windowing/IslandsWindowPlacement.Windows.cs), reached through [IslandsWindowNativeHook.cs](src/MewUI.Themes.Islands/Windowing/IslandsWindowNativeHook.cs). Corrects only the first post-load move using actual pixel dimensions; centers on the work area or owner and clamps to the monitor. | After the backend uses actual outer dimensions for extended-titlebar startup placement, remove the Windows placement partial, the Windows branch in `Place()`, and the placement callback plus its `IslandsWindowChrome` wiring. Keep the hook's Snap Layouts behavior. |
| macOS native centering places custom-titlebar windows above the geometric center. | [IslandsWindowPlacement.cs](src/MewUI.Themes.Islands/Windowing/IslandsWindowPlacement.cs): constructor, `Place()` and `MoveToCentre()`. Claims startup placement before showing, then centers before the first paint. | Remove the macOS takeover only when the backend provides the same requested centering behavior. Preserve Windows placement and maximization fixes independently. |
| macOS title-strip double-click can maximize at a stale drag origin. | `IslandsWindowPlacement.CorrectMaximizedPosition()` and the title-strip gesture in [IslandsWindowChrome.cs](src/MewUI.Themes.Islands/Windowing/IslandsWindowChrome.cs). | Once maximization consistently fills the intended work area, remove the correction and its gesture call; preserve the maximize/restore gesture itself. |
| A tooltip can remain or reappear on an inactive owner after opening a dialog. | [IslandsWindowHover.cs](src/MewUI.Themes.Islands/Windowing/IslandsWindowHover.cs). Clears owner-chain hover on activation and again after queued input completes. | After the framework retires inactive-window hover reliably, remove this helper and its chrome attachment. |
| macOS keypad Enter (`0x4C`) is delivered as `Key.None`. | [IslandsDialogKeyboard.cs](src/MewUI.Themes.Islands/Windowing/IslandsDialogKeyboard.cs): `RouteKeypadEnter()` and its `UnsafeAccessor` bubbling entry points. | Once the backend maps keypad Enter to `Key.Enter`, remove only the rerouting code and accessors. Keep the public default-action feature and its IME/repeat/control guards. |

When upgrading MewUI, test one workaround at a time with its correction disabled. Check Windows at
100% and higher DPI, secondary monitors with negative coordinates, taskbar positions, screen/owner/manual
startup modes, and maximize/restore. Check macOS centering, title-strip double-click, dialog hover,
keypad Enter, multiline editors and IME composition. Verify NativeAOT on each target OS before removing
the corresponding workaround. Automated geometry tests do not replace a real Windows placement check;
the new preview.3 Windows correction still needs that validation.

## References and license

The MewUI theme implementation is MIT licensed; see [LICENSE](LICENSE).
The reference theme data and adapted palette values come from JetBrains under Apache 2.0;
see [reference notes](docs/islands/README.md) and [third-party notices](THIRD-PARTY-NOTICES.md).
The NuGet package declares `MIT AND Apache-2.0` and includes both license texts.
This project is not an official JetBrains or MewUI theme package and is not affiliated with
or endorsed by JetBrains or the MewUI maintainers.

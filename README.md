# MewUI.Themes.Islands

[![NuGet version](https://img.shields.io/nuget/vpre/MewUI.Themes.Islands?label=NuGet&color=2563eb&logo=nuget&logoColor=white)](https://www.nuget.org/packages/MewUI.Themes.Islands)
[![NuGet downloads](https://img.shields.io/nuget/dt/MewUI.Themes.Islands?color=2563eb&logo=nuget&logoColor=white)](https://www.nuget.org/packages/MewUI.Themes.Islands)
[![Status: Preview](https://img.shields.io/badge/status-preview-d97706)](#preview-status)
[![AI-assisted development](https://img.shields.io/badge/AI-assisted-0f766e)](#ai-assisted-development)
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

## Get started

Add the theme to a MewUI application:

```sh
dotnet add package Aprillz.MewUI --version 0.22.1
dotnet add package MewUI.Themes.Islands --version 0.1.0-preview.4
```

If your application already references MewUI's individual platform and rendering packages,
keep that setup and add only the theme package. The theme depends on `Aprillz.MewUI.Core`;
the host selects the platform and rendering backend.

The [package README](src/MewUI.Themes.Islands/README.md) covers startup examples, theme options,
component APIs and window integration.

## Preview status

The current source version is `0.1.0-preview.4`, fixing unintended Windows window movement when
clicking the title bar with a dropdown open, and vertically centering controls hosted in the title bar.
See the NuGet badge above for the latest published version. Public APIs may change between preview releases.
The current dependency and integration baseline is MewUI `0.22.1`.

The library targets `net8.0` and `net10.0`, matching both `Aprillz.MewUI.Core` and
`Aprillz.MewUI` 0.22.1.

## Demo

```sh
cd src
dotnet run --project MewUI.Themes.Islands.Demo -- \
  --theme dark --density compact --page controls
```

`--theme` accepts `system`, `light` or `dark`; `--page` accepts `overview`, `controls`,
`lists & trees`, `dialogs & menus`, `window chrome` or `colors & metrics`.

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

## Framework workarounds

The theme includes compatibility fixes for **MewUI 0.22.1** covering Windows text rendering
and startup placement, macOS centering and maximization, inactive-owner hover, and keypad Enter.
Window fixes are enabled through `IslandsFramelessChrome()` / `IslandsDialogChrome()`;
Enter confirmation is opt-in through `IslandsDefaultAction()`. The Windows Direct2D text fix
runs automatically at build time for NuGet consumers.

See the [workaround inventory](docs/framework-workarounds.md) for implementation links,
activation and opt-out behavior, validation requirements, and removal conditions when upgrading MewUI.
The preview.3 Windows placement correction still needs manual validation on Windows.

Starting with preview.4, clicking the draggable title area on Windows closes the open dropdown
and consumes that press.
The next press can drag the window normally.

## AI-assisted development

This project is developed through collaboration across multiple AI models and tools
for implementation, refactoring, documentation and test development. The maintainer
is responsible for project direction and release decisions.

## References and license

The MewUI theme implementation is MIT licensed; see [LICENSE](LICENSE).
The reference theme data and adapted palette values come from JetBrains under Apache 2.0;
see [reference notes](docs/islands/README.md) and [third-party notices](THIRD-PARTY-NOTICES.md).
The NuGet package declares `MIT AND Apache-2.0` and includes both license texts.
This project is not an official JetBrains or MewUI theme package and is not affiliated with
or endorsed by JetBrains or the MewUI maintainers.

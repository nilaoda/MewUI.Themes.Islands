# MewUI.Themes.Islands

Islands (JetBrains' New UI) theme for [MewUI](https://github.com/aprillz/MewUI): its light and dark
color tables, metrics, per-control style rules, and the few shapes MewUI draws itself.

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/screenshots/overview-dark.png">
    <source media="(prefers-color-scheme: light)" srcset="docs/screenshots/overview-light.png">
    <img alt="Islands overview" src="docs/screenshots/overview-light.png" width="880">
  </picture>
</p>

## Layout

- `docs/` — the color map, upstream reference data and screenshots
- `src/MewUI.Themes.Islands/` — the library
- `src/MewUI.Themes.Islands.Tests/` — its tests
- `src/MewUI.Themes.Islands.Demo/` — the component gallery

The full documentation, including the token map and the deviation notes, is in
[`src/MewUI.Themes.Islands/README.md`](src/MewUI.Themes.Islands/README.md).

The library groups palette and metrics in `Theming/`, reusable components in `Controls/`,
menus in `Menus/`, window behavior in `Windowing/`, style rules in `Styles/`, and drawing helpers
in `Drawing/`. Fluent entry points live in `Extensions/` and retain the root namespace.

## Build and test

```bash
cd src
dotnet build MewUI.Themes.Islands.slnx
dotnet run --project MewUI.Themes.Islands.Tests
```

## Demo

The gallery lives in `src/MewUI.Themes.Islands.Demo`:

```bash
cd src
dotnet run --project MewUI.Themes.Islands.Demo
```

Startup options make screenshots reproducible:

```bash
dotnet run --project MewUI.Themes.Islands.Demo -- \
  --theme dark --density compact --page controls
```

`--theme` accepts `system`, `light` or `dark`; `--page` accepts `overview`, `controls`,
`lists & trees`, `dialogs & menus`, `window chrome` or `colors & metrics`.

## License

MIT — see [`LICENSE`](LICENSE).

# Framework workarounds

Compatibility fixes included in MewUI.Themes.Islands, with implementation links and removal conditions.
For installation and project information, see the [main README](../README.md).

The current baseline is **MewUI 0.22.1**. Window fixes are enabled by
`IslandsFramelessChrome()` / `IslandsDialogChrome()`, and keypad handling is opt-in through
`IslandsDefaultAction()`. They use static interop compatible with trimming and NativeAOT.
The Direct2D text fix runs at build time automatically for Windows consumers of the NuGet package;
it needs no theme startup call. The library retains only a Core runtime dependency.

| Behavior | Implementation | When and how to remove |
|---|---|---|
| Small Chinese text loses thin strokes at 100% scaling; using system rendering parameters globally reduces smoothing at high DPI. | [Text patch tool](../tools/MewUITextRenderingPatch/README.md) and [packaged targets](../src/MewUI.Themes.Islands/buildTransitive/MewUI.Themes.Islands.targets). Patches an `obj` copy of Direct2D 0.22.1: system parameters at 96 DPI, original smoothing above 96 DPI, selected per render target. NuGet cache files are unchanged. | Rejects unverified backend versions at build time. After upstream fixes both low/high DPI, set `IslandsWindowsTextRenderingWorkaround=false`, verify ordinary, single-file and NativeAOT output, then remove the targets, tool and bundled Mono.Cecil assets/notices in a new theme release. |
| Windows startup appears above and left of center because the backend includes a non-client frame for a custom-titlebar window. | [IslandsWindowPlacement.Windows.cs](../src/MewUI.Themes.Islands/Windowing/IslandsWindowPlacement.Windows.cs), reached through [IslandsWindowNativeHook.cs](../src/MewUI.Themes.Islands/Windowing/IslandsWindowNativeHook.cs). Corrects only the first post-load move using actual pixel dimensions; centers on the work area or owner and clamps to the monitor. | After the backend uses actual outer dimensions for extended-titlebar startup placement, remove the Windows placement partial, the Windows branch in `Place()`, and the placement callback plus its `IslandsWindowChrome` wiring. Keep the hook's Snap Layouts behavior. |
| macOS native centering places custom-titlebar windows above the geometric center. | [IslandsWindowPlacement.cs](../src/MewUI.Themes.Islands/Windowing/IslandsWindowPlacement.cs): constructor, `Place()` and `MoveToCentre()`. Claims startup placement before showing, then centers before the first paint. | Remove the macOS takeover only when the backend provides the same requested centering behavior. Preserve Windows placement and maximization fixes independently. |
| macOS title-strip double-click can maximize at a stale drag origin. | `IslandsWindowPlacement.CorrectMaximizedPosition()` and the title-strip gesture in [IslandsWindowChrome.cs](../src/MewUI.Themes.Islands/Windowing/IslandsWindowChrome.cs). | Once maximization consistently fills the intended work area, remove the correction and its gesture call; preserve the maximize/restore gesture itself. |
| A tooltip can remain or reappear on an inactive owner after opening a dialog. | [IslandsWindowHover.cs](../src/MewUI.Themes.Islands/Windowing/IslandsWindowHover.cs). Clears owner-chain hover on activation and again after queued input completes. | After the framework retires inactive-window hover reliably, remove this helper and its chrome attachment. |
| macOS keypad Enter (`0x4C`) is delivered as `Key.None`. | [IslandsDialogKeyboard.cs](../src/MewUI.Themes.Islands/Windowing/IslandsDialogKeyboard.cs): `RouteKeypadEnter()` and its `UnsafeAccessor` bubbling entry points. | Once the backend maps keypad Enter to `Key.Enter`, remove only the rerouting code and accessors. Keep the public default-action feature and its IME/repeat/control guards. |

When upgrading MewUI, test one workaround at a time with its correction disabled. Check Windows at
100% and higher DPI, secondary monitors with negative coordinates, taskbar positions, screen/owner/manual
startup modes, and maximize/restore. Check macOS centering, title-strip double-click, dialog hover,
keypad Enter, multiline editors and IME composition. Verify NativeAOT on each target OS before removing
the corresponding workaround. Automated geometry tests do not replace a real Windows placement check;
the new preview.3 Windows correction still needs that validation.

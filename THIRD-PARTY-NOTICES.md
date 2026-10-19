# Third-party notices

## IntelliJ IDEA Islands theme data

Source: [JetBrains/intellij-community, commit `d8074d411dcaa249dc393948d37f29ff50abfd5d`](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d/platform/platform-resources/src/themes/islands).

The unmodified reference snapshots are in `docs/islands/ManyIslandsLight.theme.json`,
`ManyIslandsDark.theme.json` and `ManyIslandsDarcula.theme.json`.
The MewUI palette and metrics adapt selected values from these files. The adaptation maps them
to MewUI controls, adjusts palette seeds, and uses framework-specific drawing and layout code.
See [the color map](https://github.com/nilaoda/MewUI.Themes.Islands/blob/main/docs/color-map.md) for differences.
The JSON snapshots are excluded from the NuGet package; the adapted values are included.

Upstream attribution, reproduced from [NOTICE.txt](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d/NOTICE.txt):

```text
This software includes code from IntelliJ IDEA
Copyright (C) JetBrains s.r.o.
https://www.jetbrains.com/idea/
```

The upstream source is subject to the Apache License, Version 2.0.
A copy is included in [licenses/Apache-2.0.txt](licenses/Apache-2.0.txt).
The original MewUI theme implementation is licensed under [MIT](LICENSE).
The NuGet license expression combines both as `MIT AND Apache-2.0`.

This project is not affiliated with or endorsed by JetBrains.

## Mono.Cecil

The NuGet package bundles Mono.Cecil 0.11.6 and its helper assemblies for the build-time
Windows text rendering patch tool. They are not dependencies of the application's runtime.
Source: [jbevain/cecil, tag 0.11.6](https://github.com/jbevain/cecil/tree/0.11.6).
The MIT copyright notice and license are included in [licenses/Mono.Cecil-MIT.txt](licenses/Mono.Cecil-MIT.txt).

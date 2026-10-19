# Reference theme data

These JSON files are design references from [JetBrains/intellij-community](https://github.com/JetBrains/intellij-community).

Verified snapshot: [`d8074d411dcaa249dc393948d37f29ff50abfd5d`](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d), dated 2026-10-16.
Each local file is byte-for-byte identical to its upstream file at that commit.
This identifies a matching snapshot, not the last change to each file or a released IDE version.

Upstream directory: `platform/platform-resources/src/themes/islands/`.

| Upstream file | Role |
| --- | --- |
| [ManyIslandsLight.theme.json](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d/platform/platform-resources/src/themes/islands/ManyIslandsLight.theme.json) | Default light palette and metrics reference |
| [ManyIslandsDark.theme.json](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d/platform/platform-resources/src/themes/islands/ManyIslandsDark.theme.json) | Default dark palette and metrics reference |
| [ManyIslandsDarcula.theme.json](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d/platform/platform-resources/src/themes/islands/ManyIslandsDarcula.theme.json) | Additional reference; not the default dark palette |

The snapshots are unchanged. The MewUI implementation adapts selected colors and metrics;
see the [color map](../color-map.md) for mappings and differences.
The default palettes use Light and Dark. Darcula is retained as an additional reference.
The reference files are not included in the NuGet package.

## License and attribution

The files declare JetBrains as their author. The upstream [LICENSE.txt](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d/LICENSE.txt)
states that the open-source software is subject to Apache 2.0. Its build terms are distinct
from the Apache license text. The upstream [NOTICE.txt](https://github.com/JetBrains/intellij-community/blob/d8074d411dcaa249dc393948d37f29ff50abfd5d/NOTICE.txt) attribution is
retained in [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md), together with the scope
of the adaptation. A copy of [Apache 2.0](../../licenses/Apache-2.0.txt) is also included.
These reference files are not covered by the implementation's MIT license.

See [JetBrains' UI documentation](https://plugins.jetbrains.com/docs/intellij/ui-overview.html)
for the design context. FormDev FlatLaf property dumps are not the default palette source.

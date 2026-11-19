# Windows Direct2D text rendering workaround

Baseline: `Aprillz.MewUI.Backend.Direct2D` **0.22.1**, both `net8.0` and `net10.0`.
MewUI's tuned `NATURAL_SYMMETRIC` parameters can weaken thin Chinese strokes at 100% scaling.
Using DirectWrite system parameters improves low-DPI text, but is less smooth than the original
settings at high DPI. The patch therefore selects system parameters at 96 DPI and the original
parameters above 96 DPI, per render target. The COM references are owned and released by the factory.

## Packaging and activation

The theme NuGet ships this .NET 8 tool, Mono.Cecil, its license, and
`buildTransitive/MewUI.Themes.Islands.targets`. No separate package or manual Import is required.
`dotnet pack` builds the tool; consumers only execute the bundled copy. Runtime major roll-forward
allows .NET 10-only build environments too. The tool never becomes an application runtime dependency.

`IslandsWindowsTextRenderingWorkaround` defaults to `auto`: Windows RID, or Windows host without
a RID. `false` disables it; `true` forces it on any host for the headless tests below. Projects
without a copy-local Direct2D backend are skipped. Publish-profile RIDs are evaluated after import.
The backend NuGet version (when available) and assembly version must be 0.22.1; upgrades fail
explicitly until reviewed. No runtime reflection or dynamic code generation is introduced.

The targets patch an `obj/islands-workarounds` copy and replace build and publish runtime assets
before single-file and NativeAOT input collection. The original NuGet cache is never modified.
The tool rejects in-place and already-patched input. Existing IL macros are expanded before edits
and optimized afterward to keep branch offsets valid.

## Verification

Pack to a local NuGet source with a local-only version and use a fresh package cache for each run.
The following commands run from the repository root:

```sh
islands_test_dir=$(mktemp -d "${TMPDIR:-/tmp}/islands-test.XXXXXXXX")
islands_test_version=0.1.0-local
(cd src && dotnet pack MewUI.Themes.Islands -c Release \
  -p:PackageVersion="$islands_test_version" -o "$islands_test_dir/feed")
dotnet restore tools/MewUITextRenderingPatch.Tests -p:IslandsPackageVersion="$islands_test_version" \
  --packages "$islands_test_dir/packages" --source "$islands_test_dir/feed" --source https://api.nuget.org/v3/index.json
dotnet run --project tools/MewUITextRenderingPatch.Tests -c Release -f net8.0 --no-restore \
  -p:IslandsPackageVersion="$islands_test_version"
dotnet run --project tools/MewUITextRenderingPatch.Tests -c Release -f net10.0 --no-restore \
  -p:IslandsPackageVersion="$islands_test_version"
dotnet publish tools/MewUITextRenderingPatch.Tests -c Release -f net10.0 -r osx-arm64 \
  -p:PublishAot=true -p:IslandsPackageVersion="$islands_test_version" \
  --packages "$islands_test_dir/packages" --source "$islands_test_dir/feed" --source https://api.nuget.org/v3/index.json \
  -o "$islands_test_dir/net10-aot"
"$islands_test_dir/net10-aot/MewUITextRenderingPatch.Tests"

# Run with the .NET 11 SDK to verify that ILC includes the patched backend.
dotnet publish tools/MewUITextRenderingPatch.Tests -c Release -p:TargetFrameworks=net11.0 -f net11.0 -r osx-arm64 \
  -p:PublishAot=true -p:IslandsPackageVersion="$islands_test_version" \
  --packages "$islands_test_dir/packages" --source "$islands_test_dir/feed" --source https://api.nuget.org/v3/index.json \
  -o "$islands_test_dir/net11-aot"
"$islands_test_dir/net11-aot/MewUITextRenderingPatch.Tests"
```

The checks invoke the actual patched selector with sentinel pointers, including 100%, 125%,
150%, 200%, moving back to 100%, invalid DPI, parameter-creation failure and repeated disposal.
On macOS the AOT binary tests patched managed code only; actual Windows rendering, monitor changes
and Windows NativeAOT must also be validated on Windows. Check ordinary and single-file Windows
publish output and ensure build-tool/Cecil DLLs are absent from the application.

When upstream fixes the rendering behavior, disable the workaround and verify both low and high
DPI. Then remove these tools/tests, packaged targets and the pack target, plus bundled Cecil assets
and their notice. Preserve unrelated window/input workarounds independently.

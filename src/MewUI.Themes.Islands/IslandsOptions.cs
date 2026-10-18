using Aprillz.MewUI;

namespace MewUI.Themes.Islands;

/// <summary>
/// Optional overrides for <see cref="IslandsBuilderExtensions.UseIslands"/>.
/// </summary>
public sealed record IslandsOptions
{
    /// <summary>Color table to install. Defaults to <see cref="IslandsPalette.Default"/> (Islands light + dark).</summary>
    public IslandsPalette? Palette { get; init; }

    /// <summary>Metrics to install. Defaults to the record built from <see cref="Density"/>.</summary>
    public ThemeMetrics? Metrics { get; init; }

    /// <summary>
    /// Which of Islands' two densities to install: <see cref="IslandsDensity.Regular"/> (the themes' own
    /// numbers) or <see cref="IslandsDensity.Compact"/> (their <c>.compact</c> keys).
    /// </summary>
    public IslandsDensity Density { get; init; } = IslandsDensity.Regular;

    /// <summary>
    /// Keep MewUI's accent pinned to the active variant's Islands accent (<c>accent-brand-bg</c>,
    /// <c>#3871e1</c> in both variants, so this is a no-op today and stays for a palette that varies it).
    /// </summary>
    /// <remarks>
    /// A raw <see cref="Color"/> accent in MewUI is not re-resolved when the variant changes, so the
    /// library subscribes to theme changes and re-applies it. Turn this off to drive the accent from
    /// the OS or from another theme manager; the Islands styles themselves do not depend on it.
    /// </remarks>
    public bool SyncAccentWithVariant { get; init; } = true;
}

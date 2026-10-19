using Aprillz.MewUI;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands;

/// <summary>
/// Builder entry point that installs the Islands theme.
/// </summary>
public static class IslandsBuilderExtensions
{
    /// <summary>
    /// Switches the application to Islands: its light variant in light mode, its dark one in dark mode.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="options">Palette, metrics and accent-sync overrides.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call this before <c>BuildMainWindow()</c>. The seeds, metrics and start accent are applied
    /// immediately (they must be set before the application runs); the style layer is deferred to the
    /// startup callback, which MewUI invokes after the application object exists and before the first
    /// window is shown.
    /// </para>
    /// <para>
    /// MewUI's <c>OnStartup</c> replaces any previously registered startup callback rather than
    /// composing them. If the application needs its own startup work, register that callback after
    /// this call, and invoke <see cref="IslandsTheme.Install"/> with the same options inside it.
    /// </para>
    /// </remarks>
    public static ApplicationBuilder UseIslands(this ApplicationBuilder builder, IslandsOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var palette = options?.Palette ?? IslandsPalette.Default;
        var density = options?.Density ?? IslandsDensity.Regular;

        // The density has to be in place before any rule is registered: a style bakes its own numbers when
        // the sheet builds it, and that happens on startup.
        IslandsMetrics.UseDensity(density);

        builder
            .UseSeed(IslandsSeeds.From(palette.Light), IslandsSeeds.From(palette.Dark))
            .UseMetrics(options?.Metrics ?? IslandsMetrics.For(density))
            .UseAccent(palette.Light.Accent);

        builder.OnStartup(() => IslandsTheme.Install(options));

        return builder;
    }
}

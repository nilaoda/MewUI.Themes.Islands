using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// Islands's accent (<c>Component.accentColor</c>) per variant.
/// </summary>
/// <remarks>
/// MewUI derives several colors it does not let a theme override — check marks, radio dots, progress
/// fill, the filled slider track, text selection and the accent hover overlays — straight from this
/// accent, so it must be set per variant rather than once for the process.
/// </remarks>
public static class IslandsAccent
{
    /// <summary>The accent for the requested variant, read from <see cref="IslandsPalette.Default"/>.</summary>
    public static Color For(bool isDark) => For(IslandsPalette.Default, isDark);

    /// <summary>The accent for the requested variant of a custom palette.</summary>
    public static Color For(IslandsPalette palette, bool isDark)
    {
        ArgumentNullException.ThrowIfNull(palette);
        return palette.Of(isDark).Accent;
    }
}

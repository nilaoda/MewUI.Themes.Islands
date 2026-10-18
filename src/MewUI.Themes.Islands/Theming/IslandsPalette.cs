using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// A pair of <see cref="IslandsVariantColors"/> (light + dark) that a MewUI application installs.
/// </summary>
public sealed class IslandsPalette
{
    /// <summary>The Islands light + dark palette, read from the theme files in <c>docs/islands</c>.</summary>
    public static IslandsPalette Default { get; } = new(IslandsColors.Light, IslandsColors.Dark);

    /// <summary>Creates a palette from a custom light/dark color table pair.</summary>
    public IslandsPalette(IslandsVariantColors light, IslandsVariantColors dark)
    {
        ArgumentNullException.ThrowIfNull(light);
        ArgumentNullException.ThrowIfNull(dark);

        Light = light;
        Dark = dark;
    }

    /// <summary>The light variant (<c>TabbedPane.tabType = underlined</c>, IntelliJ default button).</summary>
    public IslandsVariantColors Light { get; }

    /// <summary>The dark variant.</summary>
    public IslandsVariantColors Dark { get; }

    /// <summary>Selects the variant matching a "is the window background dark" flag.</summary>
    public IslandsVariantColors Of(bool isDark) => isDark ? Dark : Light;

    /// <summary>Selects the variant matching a live theme, using MewUI's own darkness test.</summary>
    public IslandsVariantColors Of(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        return Of(theme.IsDark);
    }
}

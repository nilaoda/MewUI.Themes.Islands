using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// MewUI seed colors, chosen so that what MewUI <em>derives</em> from them stays readable.
/// </summary>
/// <remarks>
/// MewUI derives control borders, text selection and other values from its palette seeds.
/// These seeds map the checked-in Islands light and dark color tables to those inputs.
/// <see cref="IslandsVariantColors.SeedWindowText"/> can differ from the visible body text
/// to keep framework-derived borders and realized row labels readable. Control style rules
/// apply the explicit theme colors where MewUI allows them; remaining differences are
/// documented in <c>docs/color-map.md</c> and covered by the seed derivation tests.
/// </remarks>
public static class IslandsSeeds
{
    /// <summary>Seed for Islands Light.</summary>
    public static ThemeSeed Light { get; } = From(IslandsColors.Light);

    /// <summary>Seed for Islands Dark.</summary>
    public static ThemeSeed Dark { get; } = From(IslandsColors.Dark);

    /// <summary>
    /// Maps an Islands color table onto MewUI's five seed slots. Use this for custom palettes so their
    /// derived borders, disabled text and selection colors follow the same rules as the default light and dark variants.
    /// </summary>
    public static ThemeSeed From(IslandsVariantColors colors)
    {
        ArgumentNullException.ThrowIfNull(colors);

        return new ThemeSeed
        {
            WindowBackground = colors.PanelBackground,
            WindowText = colors.SeedWindowText,
            ControlBackground = colors.ComponentBackground,
            ButtonFace = colors.ButtonBackground,
            ButtonDisabledBackground = colors.ButtonDisabledBackground,
            Error = colors.Error,
        };
    }
}

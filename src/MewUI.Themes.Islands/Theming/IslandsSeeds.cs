using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// MewUI seed colors, chosen so that what MewUI <em>derives</em> from them stays readable.
/// </summary>
/// <remarks>
/// <para>
/// MewUI computes most of its palette from five seed colors plus the accent (see
/// <c>Aprillz.MewUI.Palette</c>, whose formulas are private). Islands's own colors cannot be injected
/// directly, so the seeds are back-solved against those formulas:
/// </para>
/// <list type="bullet">
/// <item>
/// <description><c>ControlBorder = WindowBackground.Lerp(WindowText, 0.23 dark / 0.21 light).Lerp(accent,
/// 0.035 / 0.05)</c>. The dark pair derives <c>#606366</c>, one step off Islands's <c>#616365</c>. Light
/// mode wants <c>#c2c2c2</c>: with Islands's own black text the formula lands on <c>#b7bbbf</c>, eleven
/// steps off, so IntelliJ's <see cref="IslandsVariantColors.SeedWindowText"/> is <c>#232323</c> and the
/// result is <c>#bec1c5</c> — at most four. That is the one intentional deviation from Islands's table,
/// and the test project asserts both halves of it. Body text is restated to black by the <c>Window</c>
/// rule, and its contrast against <c>#f2f2f2</c> stays at ~15.9:1.</description>
/// </item>
/// <item>
/// <description><c>ControlBackground</c> is Islands's component background, which makes the derived
/// selection wash a pale accent tint in light mode and a muted one in the dark variant. It cannot be traded for
/// Islands's solid <c>List.selectionBackground</c>: a list, a tree and a grid color an item's label
/// themselves, from the palette's own selection text, as a local value — which outranks every setter a
/// style can issue — and they do it once, when the row is realized, because a selection change re-runs
/// only the row's visual states. The label therefore keeps whichever of that color and the window text
/// was right at that moment, so the two must be the same weight for a row to stay legible after it is
/// clicked. The derived wash keeps them both dark in light mode and both light in dark mode; the row rules
/// paint that wash rather than Islands's solid plate, and the deviation is
/// recorded in <c>docs/color-map.md</c>.</description>
/// </item>
/// <item>
/// <description>Light <c>ControlBackground</c> equals <c>ButtonFace</c> (<c>#ffffff</c>), which makes
/// MewUI's zebra stripe collapse to nothing — Islands has no alternating rows — and the list, grid and
/// drop-down rules switch it off so a custom palette cannot bring it back.</description>
/// </item>
/// </list>
/// <para>
/// What the palette derives and a style cannot override (grid lines, zebra base, check marks, text
/// selection, placeholder, item text) is either steered through these seeds or listed as an accepted
/// deviation in <c>docs/color-map.md</c>.
/// </para>
/// </remarks>
public static class IslandsSeeds
{
    /// <summary>Seed for IntelliJ Light.</summary>
    public static ThemeSeed Light { get; } = From(IslandsColors.Light);

    /// <summary>Seed for Darcula.</summary>
    public static ThemeSeed Dark { get; } = From(IslandsColors.Dark);

    /// <summary>
    /// Maps a Islands color table onto MewUI's five seed slots. Use this for custom palettes so their
    /// derived borders, disabled text and selection colors follow the same rules as IntelliJ and Darcula.
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

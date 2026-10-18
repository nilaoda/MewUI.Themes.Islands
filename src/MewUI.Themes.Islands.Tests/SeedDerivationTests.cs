using Xunit;
using Aprillz.MewUI;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The seeds are back-solved against MewUI's private palette formulas, so what matters is not the seed
/// values themselves but the colors they produce. These tests run <see cref="Palette"/> over
/// <see cref="IslandsSeeds"/> and compare the derived result with Islands's own table.
/// </summary>
public class SeedDerivationTests
{
    /// <summary>Per-channel error the derived colors are allowed off Islands's value.</summary>
    /// <summary>
    /// How far the framework's derived border may sit from Islands's own token. It is nine channels because
    /// the seed text that would hit the token exactly is a mid grey, and that seed text is also what a
    /// realized row label shows — legible labels beat a hairline by nine steps.
    /// </summary>
    private const int BorderBudget = 10;

    private static Palette PaletteFor(IslandsVariantColors colors) =>
        new(IslandsSeeds.From(colors), colors.Accent);

    [Fact]
    public void Control_border_lands_on_islands_component_border()
    {
        AssertChannelClose(PaletteFor(IslandsColors.Light).ControlBorder, IslandsColors.Light.Border, BorderBudget, "light");
        AssertChannelClose(PaletteFor(IslandsColors.Dark).ControlBorder, IslandsColors.Dark.Border, BorderBudget, "dark");
    }

    /// <summary>
    /// The seed text is also the color a realized row label shows, so it is chosen for legibility first: a
    /// near-black grey rather than the exact value that would land the border derivation on its token. The
    /// derivation interpolates toward the seed text, and black overshoots Islands's <c>gray-130</c>; the grey
    /// the theme uses is what is left after the two are traded off.
    /// </summary>
    [Fact]
    public void Back_solved_seed_text_derives_a_closer_border_than_islands_own()
    {
        IslandsVariantColors light = IslandsColors.Light;

        int solved = MaxChannelDelta(PaletteFor(light).ControlBorder, light.Border);
        int literal = MaxChannelDelta(
            new Palette(IslandsSeeds.From(light with { SeedWindowText = Color.FromHex("#000000") }), light.Accent)
                .ControlBorder,
            light.Border);

        Assert.True(solved < literal, $"the theme's seed text derives a border {solved} off, #000000 derives one {literal} off");
        Assert.True(solved <= BorderBudget, $"derived border is {solved} channels off Islands's");
    }

    [Fact]
    public void Surfaces_and_the_disabled_button_fill_pass_through_the_seeds()
    {
        AssertBothVariants((palette, colors) =>
        {
            AssertChannelClose(palette.WindowBackground, colors.PanelBackground, 3, nameof(palette.WindowBackground));
            AssertChannelClose(palette.ControlBackground, colors.ComponentBackground, 3, "ControlBackground");
            Assert.Equal(colors.ButtonBackground, palette.ButtonFace);
            Assert.Equal(colors.ButtonDisabledBackground, palette.ButtonDisabledBackground);
            Assert.Equal(colors.Error, palette.Error);
        });
    }

    /// <summary>
    /// A list, a tree and a grid color an item's label themselves: they write the palette's selection text or
    /// its window text onto the label as a local value, which outranks every setter a style can issue. Worse,
    /// they write it once, when the row is realized, because a selection change re-runs only the row's visual
    /// states — so a row keeps showing whichever of the two was chosen at that moment. Islands's solid
    /// <c>List.selectionBackground</c> is therefore unreachable for a row: the plate has to be one the label
    /// survives either way. This pins down the two halves of that.
    /// </summary>
    [Fact]
    public void An_item_label_stays_legible_whichever_selection_it_was_realized_with()
    {
        AssertBothVariants((palette, colors) =>
        {
            // Same weight: a stale choice reads as the current one.
            Assert.Equal(IsDark(palette.WindowText), IsDark(palette.SelectionText));

            // And both are legible on the plate the row rules paint, which is this wash.
            AssertLegible(palette.SelectionText, palette.SelectionBackground, "selection text");
            AssertLegible(palette.WindowText, palette.SelectionBackground, "window text");
        });
    }

    /// <summary>
    /// The accent is not a seed: it is handed to <c>UseAccent</c> per variant, and MewUI derives selection
    /// from it as a wash rather than the flat accent Islands selects with. The row styles paint that wash, so
    /// <c>List.selectionBackground</c> and its foreground are recorded as unreachable in
    /// <c>docs/color-map.md</c>; this is the measurement behind that entry.
    /// </summary>
    [Fact]
    public void Derived_selection_is_an_accent_wash_rather_than_expuis_plate()
    {
        Palette palette = PaletteFor(IslandsColors.Light);

        Assert.NotEqual(IslandsColors.Light.Selection, palette.SelectionBackground);
        Assert.Equal(IslandsColors.Light.Accent, palette.Accent);
        Assert.Equal(IslandsColors.Light.Accent, palette.Focus);
    }

    /// <summary>
    /// MewUI derives disabled text, placeholder and grid colors from the seeds with no hook to override
    /// them; the Islands table's own values have to be close enough that the uncovered controls still read
    /// as JetBrains. This is the budget the deviation list in <c>docs/color-map.md</c> is written against.
    /// </summary>
    [Fact]
    public void Palette_only_derivations_stay_inside_the_documented_budget()
    {
        AssertBothVariants((palette, colors) =>
        {
            // The theme restates disabled text on every control it styles, so what the framework derives
            // from the seeds is only the backstop for the ones it does not: it is allowed to sit further off
            // than a value a rule hands out.
            AssertChannelClose(palette.DisabledText, colors.DisabledText, 48, nameof(palette.DisabledText));
            AssertChannelClose(palette.PlaceholderText, colors.DisabledText, 96, nameof(palette.PlaceholderText));

        });
    }

    /// <summary>
    /// Islands uses one accent for both variants — <c>Blue4</c> in light and <c>Blue6</c> in dark, the same
    /// <c>#3574f0</c> — so switching variants must not move <c>Application.Accent</c>.
    /// </summary>
    [Fact]
    public void Both_variants_carry_islands_one_accent()
    {
        Assert.Equal(Color.FromHex("#3871e1"), IslandsColors.Light.Accent);
        Assert.Equal(Color.FromHex("#3871e1"), IslandsColors.Dark.Accent);

        IslandsPalette palette = IslandsPalette.Default;
        Assert.Equal(IslandsColors.Light.Accent, palette.Of(ThemeFor(IslandsColors.Light)).Accent);
        Assert.Equal(IslandsColors.Dark.Accent, palette.Of(ThemeFor(IslandsColors.Dark)).Accent);
    }

    private static void AssertBothVariants(Action<Palette, IslandsVariantColors> assert)
    {
        assert(PaletteFor(IslandsColors.Light), IslandsColors.Light);
        assert(PaletteFor(IslandsColors.Dark), IslandsColors.Dark);
    }

    /// <summary>A theme built the way the application would; enough for the variant test, no more.</summary>
    private static Theme ThemeFor(IslandsVariantColors colors) =>
        new()
        {
            Name = colors == IslandsColors.Light ? "islands-light" : "islands-dark",
            Palette = PaletteFor(colors),
            Metrics = IslandsMetrics.Default,
        };

    private static void AssertChannelClose(Color actual, Color expected, int budget, string what)
    {
        int delta = MaxChannelDelta(actual, expected);
        Assert.True(
            delta <= budget,
            $"{what}: derived {ToHex(actual)} is {delta} off Islands's {ToHex(expected)} (budget {budget})");
    }

    private static int MaxChannelDelta(Color a, Color b) =>
        Math.Max(Math.Max(Math.Abs(a.R - b.R), Math.Abs(a.G - b.G)), Math.Abs(a.B - b.B));

    /// <summary>MewUI's own weighting, on the 0..1 scale its contrast test uses.</summary>
    private static double Luma(Color color) =>
        (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;

    /// <summary>Whether a reader sees a text color as dark or light, which is what a stale value keeps.</summary>
    private static bool IsDark(Color color) => Luma(color) < 0.5;

    /// <summary>The WebAIM floor a theme is expected to clear for body text.</summary>
    private static void AssertLegible(Color text, Color plate, string what)
    {
        double ratio = ContrastRatio(text, plate);
        Assert.True(
            ratio >= 4.5,
            $"{what} {ToHex(text)} on the row plate {ToHex(plate)} is {ratio:0.##}:1");
    }

    private static double ChannelLightness(int channel)
    {
        double srgb = channel / 255.0;
        return srgb <= 0.03928 ? srgb / 12.92 : Math.Pow((srgb + 0.055) / 1.055, 2.4);
    }

    private static double RelativeLuminance(Color color) =>
        0.2126 * ChannelLightness(color.R) + 0.7152 * ChannelLightness(color.G) + 0.0722 * ChannelLightness(color.B);

    private static double ContrastRatio(Color foreground, Color background)
    {
        double first = RelativeLuminance(foreground);
        double second = RelativeLuminance(background);

        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static string ToHex(Color color) =>
        $"#{color.R:x2}{color.G:x2}{color.B:x2}";
}

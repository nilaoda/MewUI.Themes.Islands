using Xunit;
using System.Reflection;
using Aprillz.MewUI;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// Text has to be readable on the surface it lands on, in both variants.
/// </summary>
/// <remarks>
/// <para>
/// This suite exists because the failure is invisible in the place it is introduced. A colour table reads as
/// a list of plausible grays, and each value can be right — read out of the theme faithfully, asserted
/// against its hex — while a *pair* of them is unusable together. Only a contrast ratio says whether one can
/// be seen against the other, and nothing in the table shows it.
/// </para>
/// <para>
/// Islands states both <c>text-secondary</c> and <c>text-disabled</c> and means different things by them.
/// The disabled colour is chosen to look unreachable — around 1.8:1 on the dark panel — which is right for a
/// control nobody can use and useless for a hint. Taking it for a hint is what made a path under a heading
/// disappear into the panel; that shipped, and this is the check that would have caught it.
/// </para>
/// </remarks>
public class TextContrastTests
{
    /// <summary>WCAG AA for body text.</summary>
    private const double BodyFloor = 4.5;

    /// <summary>WCAG AA for large text, and for glyphs the reader recognises rather than reads.</summary>
    private const double GlyphFloor = 3.0;

    /// <summary>
    /// The combinations the theme actually paints: foreground, the surface it lands on, and the floor it has
    /// to clear there.
    /// </summary>
    /// <remarks>
    /// Stated as pairs rather than as "everything against everything", because not every combination occurs
    /// and a tool tip's own pair would fail against the panel it does not sit on. Disabled text is absent on
    /// purpose, and that is the whole point of the suite: it is <em>meant</em> to be faint, and its floor is
    /// low enough that listing it here would suggest otherwise.
    /// </remarks>
    private static readonly (string Foreground, string Surface, double Floor)[] Readable =
    [
        // Body text, on every surface the theme paints for content.
        (nameof(IslandsVariantColors.Text), nameof(IslandsVariantColors.PanelBackground), BodyFloor),
        (nameof(IslandsVariantColors.Text), nameof(IslandsVariantColors.ComponentBackground), BodyFloor),
        (nameof(IslandsVariantColors.Text), nameof(IslandsVariantColors.MenuBackground), BodyFloor),
        (nameof(IslandsVariantColors.Text), nameof(IslandsVariantColors.ToolBarBackground), BodyFloor),

        // Text that explains rather than states. Read, just not in the main voice.
        (nameof(IslandsVariantColors.SecondaryText), nameof(IslandsVariantColors.PanelBackground), GlyphFloor),
        (nameof(IslandsVariantColors.SecondaryText), nameof(IslandsVariantColors.ComponentBackground), GlyphFloor),
        (nameof(IslandsVariantColors.SecondaryText), nameof(IslandsVariantColors.MenuBackground), GlyphFloor),
        (nameof(IslandsVariantColors.SecondaryText), nameof(IslandsVariantColors.ToolBarBackground), GlyphFloor),

        // A tool tip carries its own pair, and it is the one surface the body colour does not reach.
        (nameof(IslandsVariantColors.ToolTipForeground), nameof(IslandsVariantColors.ToolTipBackground), BodyFloor),

        // Glyphs, on the surface they are drawn on.
        (nameof(IslandsVariantColors.Icon), nameof(IslandsVariantColors.ComponentBackground), GlyphFloor),
        (nameof(IslandsVariantColors.Arrow), nameof(IslandsVariantColors.ComponentBackground), GlyphFloor),
        (nameof(IslandsVariantColors.MenuAcceleratorText), nameof(IslandsVariantColors.MenuBackground), GlyphFloor),

        // Text on a selection plate, which is a surface of its own.
        (nameof(IslandsVariantColors.SelectionText), nameof(IslandsVariantColors.Selection), BodyFloor),
        (nameof(IslandsVariantColors.InactiveSelectionText), nameof(IslandsVariantColors.InactiveSelection), BodyFloor),
        (nameof(IslandsVariantColors.MenuAcceleratorSelectionText), nameof(IslandsVariantColors.Selection), BodyFloor),
    ];

    public static TheoryData<string, string, string, double> ReadableData()
    {
        var data = new TheoryData<string, string, string, double>();
        foreach ((IslandsVariantColors colors, string variant) in Variants())
        {
            foreach ((string foreground, string surface, double floor) in Readable)
            {
                data.Add(variant, foreground, surface, floor);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ReadableData))]
    public void Text_clears_its_floor_against_the_surface_it_lands_on(
        string variant, string foreground, string surface, double floor)
    {
        IslandsVariantColors colors = Variant(variant);
        Color ink = Read(colors, foreground);
        Color plate = Read(colors, surface);
        double ratio = Contrast(ink, plate);

        Assert.True(
            ratio >= floor,
            $"{variant}: {foreground} ({Hex(ink)}) on {surface} ({Hex(plate)}) is {ratio:F2}:1, "
            + $"under the {floor:F1}:1 this pair has to clear.");
    }

    private static string Hex(Color color) => $"#{color.R:x2}{color.G:x2}{color.B:x2}";

    private static IEnumerable<(IslandsVariantColors Colors, string Variant)> Variants()
    {
        yield return (IslandsColors.Light, "light");
        yield return (IslandsColors.Dark, "dark");
    }

    private static IslandsVariantColors Variant(string variant) =>
        variant == "light" ? IslandsColors.Light : IslandsColors.Dark;

    private static Color Read(IslandsVariantColors colors, string property) =>
        (Color)typeof(IslandsVariantColors).GetProperty(property)!.GetValue(colors)!;

    /// <summary>WCAG relative luminance.</summary>
    private static double Luminance(Color color) =>
        (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));

    private static double Channel(byte value)
    {
        double channel = value / 255.0;
        return channel <= 0.04045 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static double Contrast(Color a, Color b)
    {
        double high = Math.Max(Luminance(a), Luminance(b));
        double low = Math.Min(Luminance(a), Luminance(b));
        return (high + 0.05) / (low + 0.05);
    }
}

using Xunit;
using System.Reflection;
using System.Text.RegularExpressions;
using Aprillz.MewUI;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The color table is the package's public contract: every field comes from a named Islands
/// <c>UIDefaults</c> key, and a custom palette has to fill all of them.
/// </summary>
public class ColorTableTests
{
    /// <summary>
    /// Fields the style layer cannot reach, because MewUI paints them from a palette color of its own or
    /// has no visual state for them. Each one is explained in <c>docs/color-map.md</c>.
    /// </summary>
    private static readonly string[] DeclaredButUnreachable =
    [
        "AccentLight", "ErrorBorder", "GridHeaderBackground",
        "GridHeaderSeparator", "GridLine", "InactiveSelection", "InactiveSelectionText",
        "MenuAcceleratorSelectionText", "MenuAcceleratorText", "MenuCheckBackground", "ProgressFill",
        "ProgressTextOnFill", "ScrollTrack", "ScrollTrackBackground", "SelectionText",
        "Success", "SuccessBorder", "TabFocusBackground", "Warning", "WarningBorder",
    ];

    /// <summary>Fields Islands paints as a translucent overlay rather than as a surface of their own.</summary>
    private static readonly string[] Overlays =
    [
        nameof(IslandsVariantColors.HoverBackground), nameof(IslandsVariantColors.ButtonHover),
        nameof(IslandsVariantColors.ButtonPressed), nameof(IslandsVariantColors.ToolbarHover),
        nameof(IslandsVariantColors.ToolbarPressed), nameof(IslandsVariantColors.TabHoverBackground),
        nameof(IslandsVariantColors.ScrollThumb), nameof(IslandsVariantColors.ScrollThumbHover),
        nameof(IslandsVariantColors.ScrollThumbPressed),
    ];

    /// <summary>Fields that are a thumb over nothing, so they carry no color of their own at all.</summary>
    private static readonly string[] TransparentByDesign =
    [
        nameof(IslandsVariantColors.ScrollTrack), nameof(IslandsVariantColors.ScrollTrackBackground),
    ];

    /// <summary>Values resolved from the Islands theme JSON, so a retyped hex shows up here.</summary>
    private static readonly (IslandsVariantColors Colors, string Property, string Hex)[] Provenance =
    [
        (IslandsColors.Light, nameof(IslandsVariantColors.PanelBackground), "#f7f8f9"),
        (IslandsColors.Light, nameof(IslandsVariantColors.ComponentBackground), "#ffffff"),
        (IslandsColors.Light, nameof(IslandsVariantColors.Border), "#d1d3d9"),
        (IslandsColors.Light, nameof(IslandsVariantColors.BorderFocused), "#3871e1"),
        (IslandsColors.Light, nameof(IslandsVariantColors.FocusRing), "#3871e1"),
        (IslandsColors.Light, nameof(IslandsVariantColors.Accent), "#3871e1"),
        (IslandsColors.Light, nameof(IslandsVariantColors.Selection), "#d0dffe"),
        (IslandsColors.Light, nameof(IslandsVariantColors.InactiveSelection), "#e9eaee"),
        (IslandsColors.Light, nameof(IslandsVariantColors.PrimaryButtonBackground), "#3871e1"),
        (IslandsColors.Light, nameof(IslandsVariantColors.TabSelectedBackground), "#e3ebfe"),
        (IslandsColors.Light, nameof(IslandsVariantColors.TabSelectedBorder), "#a7c5ff"),
        (IslandsColors.Light, nameof(IslandsVariantColors.TabSelectedInactiveBackground), "#e9eaee"),
        (IslandsColors.Light, nameof(IslandsVariantColors.TabSelectedInactiveBorder), "#d1d3d9"),
        (IslandsColors.Light, nameof(IslandsVariantColors.CheckFilledBackground), "#3871e1"),
        (IslandsColors.Light, nameof(IslandsVariantColors.SliderFill), "#3871e1"),
        (IslandsColors.Light, nameof(IslandsVariantColors.ComboPopupBorder), "#b5b7bd"),

        (IslandsColors.Dark, nameof(IslandsVariantColors.PanelBackground), "#191a1c"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.ComponentBackground), "#26282c"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.Border), "#40434a"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.Accent), "#3871e1"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.InactiveSelection), "#33353b"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.ToolTipForeground), "#d1d3d9"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.ProgressFill), "#3871e1"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.TabSelectedBackground), "#233558"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.TabSelectedBorder), "#2e4d89"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.TabSelectedInactiveBackground), "#26282c"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.TabSelectedInactiveBorder), "#40434a"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.CheckFilledForeground), "#ffffff"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.Error), "#c54e58"),
        (IslandsColors.Dark, nameof(IslandsVariantColors.ComboPopupBorder), "#4c4f56"),
    ];

    [Theory]
    [MemberData(nameof(ProvenanceData))]
    public void Field_holds_the_hex_from_the_upstream_dump(IslandsVariantColors colors, string property, string hex)
    {
        Color actual = Read(colors, property);

        Assert.Equal(Color.FromHex(hex), actual);
    }

    public static TheoryData<IslandsVariantColors, string, string> ProvenanceData()
    {
        var data = new TheoryData<IslandsVariantColors, string, string>();
        foreach ((IslandsVariantColors colors, string property, string hex) in Provenance)
        {
            data.Add(colors, property, hex);
        }

        return data;
    }

    [Fact]
    public void Both_variants_fill_the_whole_table()
    {
        foreach ((IslandsVariantColors colors, string variant) in
                 new[] { (IslandsColors.Light, "light"), (IslandsColors.Dark, "dark") })
        {
            foreach (PropertyInfo field in Fields())
            {
                Color value = Read(colors, field.Name);
                byte alpha = value.A;

                // Islands paints hover and press as a translucent overlay over whatever surface they land
                // on, and its scroll bar as a thumb over nothing; every other field is a surface of its own
                // and has to be opaque, or a hand-written palette has forgotten a key.
                bool overlay = Overlays.Contains(field.Name);
                bool transparentByDesign = TransparentByDesign.Contains(field.Name);

                if (transparentByDesign)
                {
                    Assert.True(alpha == 0, $"{variant}.{field.Name} is drawn over nothing, but carries alpha {alpha}");
                }
                else if (overlay)
                {
                    Assert.True(alpha is > 0 and < 255, $"{variant}.{field.Name} is an overlay, but carries alpha {alpha}");
                }
                else
                {
                    Assert.True(alpha == 255, $"{variant}.{field.Name} is not opaque (alpha {alpha})");
                }
            }
        }
    }

    [Fact]
    public void Every_field_is_a_required_init_only_member()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string source = LibrarySource.Read("Theming/IslandsVariantColors.cs");
        int declared = Regex.Matches(source, @"public required Color \w+ \{ get; init; \}").Count;

        // required + init is what makes a custom palette that forgets a key fail to compile, and what keeps
        // an installed palette from being rewritten underneath a running theme.
        Assert.Equal(Fields().Length, declared);
    }

    /// <summary>
    /// A color that is neither applied by a rule nor on the deviation list is either a key someone forgot
    /// to wire up or one that got wired up and left behind in the documentation.
    /// </summary>
    [Fact]
    public void Every_field_is_applied_or_recorded_as_unreachable()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string source = string.Concat(
            LibrarySource.Files("*.cs")
                .Where(path => !Path.GetFileName(path).Equals("IslandsVariantColors.cs", StringComparison.Ordinal)
                            && !Path.GetFileName(path).Equals("IslandsColors.cs", StringComparison.Ordinal))
                .Select(File.ReadAllText));

        foreach (PropertyInfo field in Fields())
        {
            // A "Palette." receiver is MewUI's own derived entry, not this table's field of the same name.
            bool applied = Regex.IsMatch(
                source,
                @"(?<!Palette)\." + Regex.Escape(field.Name) + @"\b");
            bool documented = DeclaredButUnreachable.Contains(field.Name);

            Assert.True(
                applied || documented,
                $"{field.Name} is applied by no rule and on no deviation list");
            Assert.False(
                applied && documented,
                $"{field.Name} is now applied by a rule; drop it from the deviation list and from docs/color-map.md");
        }
    }

    [Fact]
    public void A_derived_palette_keeps_the_style_layer_and_overrides_only_what_it_names()
    {
        IslandsVariantColors derived = IslandsColors.Dark with
        {
            Accent = Color.FromHex("#5e81ac"),
            PanelBackground = Color.FromHex("#2e3440"),
        };

        ThemeSeed seed = IslandsSeeds.From(derived);

        Assert.Equal(derived.PanelBackground, seed.WindowBackground);
        Assert.Equal(Color.FromHex("#5e81ac"), derived.Accent);
        Assert.Equal(IslandsColors.Dark.ComponentBackground, seed.ControlBackground);
        Assert.Equal(IslandsColors.Dark.Border, derived.Border);
    }

    private static Color Read(IslandsVariantColors colors, string property) =>
        (Color)typeof(IslandsVariantColors).GetProperty(property, BindingFlags.Public | BindingFlags.Instance)!
            .GetValue(colors)!;

    private static PropertyInfo[] Fields() =>
        typeof(IslandsVariantColors).GetProperties(BindingFlags.Public | BindingFlags.Instance);
}

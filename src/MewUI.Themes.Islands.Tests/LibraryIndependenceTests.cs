using Xunit;
using System.Text.RegularExpressions;
using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The library ships as a package, so nothing in it may lean on an application. These guards fail the moment
/// it starts to.
/// </summary>
public class LibraryIndependenceTests
{
    /// <summary>Any CJK code point, in any file the package carries.</summary>
    private static readonly Regex Cjk = new(@"[\u3040-\u30ff\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff\uff00-\uffef]", RegexOptions.Compiled);

    [Fact]
    public void The_library_does_not_reference_an_application()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string project = LibrarySource.Read("MewUI.Themes.Islands.csproj");

        Assert.DoesNotContain("<ProjectReference", project, StringComparison.Ordinal);

        // The meta-package would drag every platform backend onto a consumer of the theme.
        Assert.Contains("Aprillz.MewUI.Core", project, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Aprillz.MewUI\"", project, StringComparison.Ordinal);
    }

    [Fact]
    public void Internals_reach_only_the_librarys_own_tests()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string project = LibrarySource.Read("MewUI.Themes.Islands.csproj");
        MatchCollection visibleTo = Regex.Matches(project, @"<InternalsVisibleTo Include=""([^""]+)""");

        Assert.Single(visibleTo);
        Assert.Equal("MewUI.Themes.Islands.Tests", visibleTo[0].Groups[1].Value);
    }

    /// <summary>
    /// The package's documentation and API surface is English; a non-English string that reaches a
    /// control's text would ship to every consumer of the theme.
    /// </summary>
    [Fact]
    public void No_shipped_file_carries_a_non_english_literal()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        IEnumerable<string> shipped = LibrarySource.Files("*.cs")
            .Concat(LibrarySource.Files("*.csproj"))
            .Concat(LibrarySource.Files("*.props"))
            .Concat(LibrarySource.Files("*.md"));

        foreach (string path in shipped)
        {
            Match first = Cjk.Match(File.ReadAllText(path));

            Assert.True(
                !first.Success,
                $"{Path.GetRelativePath(LibrarySource.Root!, path)} carries non-ASCII CJK text near \"{Context(path, first)}\"");
        }
    }

    [Fact]
    public void The_color_map_that_the_readme_links_exists()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string map = LibrarySource.Read(Path.Combine("docs", "color-map.md"));

        // A token, a key Islands states for a control, and the deviations section: what makes the file a
        // map rather than prose.
        Assert.Contains("layer-1-bg", map, StringComparison.Ordinal);
        Assert.Contains("*/selectionInactiveBackground", map, StringComparison.Ordinal);
        Assert.Contains("Deviation", map, StringComparison.Ordinal);
    }

    /// <summary>
    /// A palette is chosen per theme at paint time, so a custom pair drives the style layer with no rebuild.
    /// </summary>
    [Fact]
    public void A_custom_palette_still_drives_the_style_layer()
    {
        IslandsVariantColors custom = IslandsColors.Dark with { PanelBackground = Color.FromHex("#2e3440") };
        var palette = new IslandsPalette(IslandsColors.Light, custom);

        Theme darkTheme = new()
        {
            Name = "custom-dark",
            Palette = new Palette(IslandsSeeds.From(custom), custom.Accent),
            Metrics = IslandsMetrics.Default,
        };

        Assert.Same(custom, palette.Of(darkTheme));
        Assert.Equal(Color.FromHex("#2e3440"), palette.Of(darkTheme).PanelBackground);
        Assert.Equal(custom.PanelBackground, IslandsSeeds.From(custom).WindowBackground);
    }

    private static string Context(string path, Match match)
    {
        if (!match.Success)
        {
            return string.Empty;
        }

        string text = File.ReadAllText(path);
        int start = Math.Max(0, match.Index - 40);
        int length = Math.Min(80, text.Length - start);

        return text.Substring(start, length).ReplaceLineEndings(" ");
    }
}

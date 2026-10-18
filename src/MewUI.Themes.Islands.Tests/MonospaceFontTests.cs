using Xunit;
using Aprillz.MewUI;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The family a column of identifiers is set in has to resolve on every platform the theme ships on.
/// </summary>
/// <remarks>
/// A single family name cannot do that, and the failure is invisible on the machine it is written on: the
/// developer's own platform has the font they typed, and everyone else's silently falls back to something
/// proportional, which is the one outcome the monospaced column exists to avoid. MewUI takes a
/// comma-separated list and draws with the first one installed, so the guard is that the list stays a list
/// and keeps naming a family each platform guarantees.
/// </remarks>
public class MonospaceFontTests
{
    /// <summary>Families that ship with the platform, so a device without either would be unusual.</summary>
    private static readonly string[] Guaranteed = ["Consolas", "Menlo"];

    private static string[] Families() =>
        IslandsMetrics.MonospaceFontFamily
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    [Fact]
    public void The_family_is_a_list_rather_than_one_name()
    {
        string[] families = Families();

        Assert.True(
            families.Length > 1,
            $"'{IslandsMetrics.MonospaceFontFamily}' is a single family; a monospaced column then only "
            + "resolves on the platform that family came from.");
    }

    [Fact]
    public void The_list_names_a_family_that_is_guaranteed_on_each_platform()
    {
        string[] families = Families();
        var comparer = StringComparer.OrdinalIgnoreCase;

        foreach (string family in Guaranteed)
        {
            Assert.True(
                families.Contains(family, comparer),
                $"'{IslandsMetrics.MonospaceFontFamily}' does not name '{family}'.");
        }
    }

    [Fact]
    public void No_entry_is_empty_or_quoted()
    {
        // MewUI trims quotes as well as spaces, but an entry left as 'Menlo' in the source is a typo that
        // only shows up as a missing font on the machine that has it.
        foreach (string family in Families())
        {
            Assert.DoesNotContain('\'', family);
            Assert.DoesNotContain('"', family);
            Assert.False(string.IsNullOrWhiteSpace(family));
        }
    }
}

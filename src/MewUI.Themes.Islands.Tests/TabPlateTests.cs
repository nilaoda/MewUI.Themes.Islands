using Xunit;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Styles;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// A tab is claimed the instant it is clicked, and the plate it lands on has to arrive there without the
/// strip dimming on the way.
/// </summary>
/// <remarks>
/// Both facts that make that true are invisible in a screenshot of the strip at rest: one is about how two
/// colours interpolate, the other about how long the interpolation lasts. Neither can be seen once the
/// animation has finished, and both were wrong in the revision that flickered — so they are pinned here
/// rather than left to a recording.
/// </remarks>
public class TabPlateTests
{
    /// <summary>The two plates a strip can wear: the window with the focus, and the window without it.</summary>
    private static readonly bool[] Plates = [false, true];

    /// <summary>
    /// A fresh sheet per test: the lookup is the same one an application does, and the library only ever
    /// adds rules to it.
    /// </summary>
    private static StyleSheet Registered()
    {
        var sheet = new StyleSheet();
        IslandsStyles.Register(sheet);
        return sheet;
    }

    [Fact]
    public void The_resting_plate_differs_from_the_claimed_one_in_alpha_alone()
    {
        foreach (IslandsVariantColors colors in new[] { IslandsColors.Light, IslandsColors.Dark })
        {
            foreach (bool inactive in Plates)
            {
                Color restingFill = IslandsTabStyles.RestingFill(colors, inactive);
                Color claimedFill = IslandsTabStyles.ClaimedFill(colors, inactive);
                Color restingEdge = IslandsTabStyles.RestingEdge(colors, inactive);
                Color claimedEdge = IslandsTabStyles.ClaimedEdge(colors, inactive);

                Assert.Equal(0, restingFill.A);
                Assert.Equal(0, restingEdge.A);

                // The channels are what matter. A transition moves each one on its own, so a resting plate of
                // a different hue crosses a half-transparent dark middle: the chip dims before it brightens,
                // and dims again on the way back out. Sharing the channels leaves alpha as the only thing
                // that moves, and at alpha zero the resting plate paints nothing either way.
                Assert.Equal(
                    (claimedFill.R, claimedFill.G, claimedFill.B),
                    (restingFill.R, restingFill.G, restingFill.B));
                Assert.Equal(
                    (claimedEdge.R, claimedEdge.G, claimedEdge.B),
                    (restingEdge.R, restingEdge.G, restingEdge.B));
            }
        }
    }

    [Fact]
    public void The_plate_is_cut_rather_than_faded()
    {
        MewProperty[] properties = [Control.BackgroundProperty, Control.BorderBrushProperty];

        foreach (string name in new[] { IslandsStyleNames.TabHeader, IslandsStyleNames.TabHeaderInactive })
        {
            Style? style = Registered().Get(name);

            Assert.NotNull(style);

            foreach (MewProperty property in properties)
            {
                // Compared by reference because a property's identity is the instance the framework hands
                // out, and that is also how the lookup matches them.
                Transition? declared = style!.Transitions
                    .FirstOrDefault(transition => ReferenceEquals(transition.Property, property));

                Assert.NotNull(declared);
                Assert.True(
                    declared!.Duration <= TimeSpan.FromMilliseconds(IslandsTabStyles.SnappedMs),
                    $"{name} animates {property.Name} over {declared.Duration.TotalMilliseconds} ms; a strip "
                    + "with one plate arriving and another leaving reads a cross-fade as a flicker.");
            }
        }
    }
}

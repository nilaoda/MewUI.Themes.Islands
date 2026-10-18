using Xunit;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The macOS-only take-over of window placement, which is two moments: claim the answer, then place it.
/// </summary>
/// <remarks>
/// These read the source rather than drive the UI: placement needs a realised window on a real screen, and a
/// window moved to the wrong half of the display is not something a headless test can tell apart from one
/// moved to the right half. What is pinned is the ordering and the arithmetic — both of which come back wrong
/// silently if someone tidies the code into one step or copies a frame without flipping it.
/// </remarks>
public class WindowPlacementTests
{
    private const string Placement = "Windowing/IslandsWindowPlacement.cs";
    private const string Chrome = "Windowing/IslandsWindowChrome.cs";

    /// <summary>
    /// The platform's biased answer is only wrong on macOS, so the claim that replaces it must not be made
    /// anywhere else: a window handed <see cref="WindowStartupLocation.Manual"/> on Windows or X11 gets the
    /// framework's default instead of the placement it asked for.
    /// </summary>
    [Fact]
    public void The_claim_is_made_only_after_the_macos_check()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string body = Method("internal IslandsWindowPlacement(Window window)", Placement);

        int platform = body.IndexOf("if (!OperatingSystem.IsMacOS())", StringComparison.Ordinal);
        int read = body.IndexOf("_centreOn = window.StartupLocation switch", StringComparison.Ordinal);
        int write = body.IndexOf("window.StartupLocation = _centreOn is null", StringComparison.Ordinal);

        Assert.True(platform >= 0, "the placement no longer asks which platform it is on, so it corrects all of them");
        Assert.True(read >= 0 && write >= 0, "the claim is gone, so the platform answers for itself again");
        Assert.True(platform < read && read < write, "the claim is made before the platform check, so non-macOS windows lose their placement");
    }

    /// <summary>
    /// Claiming early only helps if the placement is actually asked for before the window is painted, and the
    /// framework re-applies its own answer after <c>Loaded</c>. Losing either half leaves a window that lands
    /// off-centre, or lands on-centre and then jumps back.
    /// </summary>
    [Fact]
    public void A_chrome_that_claims_also_hands_the_placement_to_a_later_moment()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string chrome = LibrarySource.Read(Chrome);

        // One chrome, built from the window, does the claiming.
        Assert.Equal(1, chrome.Split("new IslandsWindowPlacement(window)").Length - 1);
        Assert.Contains("internal void ApplyPlacement() => _placement.Place();", chrome, StringComparison.Ordinal);

        // And the entry point that builds it subscribes that placement to the same event as the insets.
        string entry = Method("    public static IslandsWindowChrome IslandsFramelessChrome<TWindow>(this TWindow window)", "Extensions/IslandsWindowChromeExtensions.cs");

        Assert.Contains("window.Loaded += chrome.ApplyInsets;", entry, StringComparison.Ordinal);
        Assert.Contains("window.Loaded += chrome.ApplyPlacement;", entry, StringComparison.Ordinal);
    }

    /// <summary>
    /// The framework's native centre is the biased one, so there is nothing to nudge: the position is computed
    /// from the region and the window's own size. A delta off the platform's answer would inherit the bias it
    /// is meant to remove.
    /// </summary>
    [Fact]
    public void The_centre_is_computed_once_and_only_for_a_requested_region()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string source = LibrarySource.Read(Placement);

        // Startup centering performs one move; maximizing has its own placement correction.
        string centre = Method("    private void MoveToCentre(Point regionTopLeft, Size region)", Placement);
        Assert.Equal(1, centre.Split("_window.MoveTo(").Length - 1);
        Assert.Contains("private void MoveToCentre(Point regionTopLeft, Size region)", source, StringComparison.Ordinal);

        string place = Method("    internal void Place()", Placement);

        // A window nobody asked to centre stays where it was put, and only the two real regions are handled.
        Assert.Contains("case null:", place, StringComparison.Ordinal);
        Assert.Contains("case WindowStartupLocation.CenterOwner when _window.Owner is { } owner && owner.Handle != 0:", place, StringComparison.Ordinal);
        Assert.Contains("case WindowStartupLocation.CenterScreen when TryGetWorkArea(out Rect area):", place, StringComparison.Ordinal);
        Assert.Equal(2, place.Split("MoveToCentre(").Length - 1);
    }

    /// <summary>
    /// AppKit measures from the bottom of the display and MewUI from the top, and a struct wider than 16 bytes
    /// is returned through a different entry point on Intel. Neither is visible in a signature, and both turn
    /// into a window that is off the screen rather than an error.
    /// </summary>
    [Fact]
    public void The_work_area_is_flipped_into_top_down_units_and_read_the_way_the_abi_wants()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string source = LibrarySource.Read(Placement);
        string area = Method("    private bool TryGetWorkArea(out Rect workArea)", Placement);

        int guard = area.IndexOf("visible.size.height <= 0", StringComparison.Ordinal);
        int flipped = area.IndexOf("(full.origin.y + full.size.height) - (visible.origin.y + visible.size.height)", StringComparison.Ordinal);

        Assert.True(guard >= 0, "an unreadable frame is no longer refused, so the window is moved to a half-read region");
        Assert.True(flipped > guard, "the frame is used as AppKit reports it, which is upside down relative to the window");

        Assert.Contains("RuntimeInformation.ProcessArchitecture == Architecture.X64", source, StringComparison.Ordinal);
        Assert.Contains("objc_msgSend_stret_rect", source, StringComparison.Ordinal);
    }

    private static string Method(string signature, string file)
    {
        string source = LibrarySource.Read(file);
        int start = source.IndexOf(signature, StringComparison.Ordinal);

        Assert.True(start >= 0, $"{signature} is no longer in {file}");

        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        Assert.True(end > start, $"{signature} does not close where the tests expect");

        return source[start..end];
    }
}

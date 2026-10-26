using Xunit;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The title strip's press handling, which decides between dragging the window and maximizing it.
/// </summary>
/// <remarks>
/// These read the source rather than drive the UI: a press cannot be synthesised here, because handing one to
/// the window needs accessibility permission the test machine does not grant. What is pinned is the ordering —
/// the thing that silently comes back if someone moves the drag up or the count down.
/// </remarks>
public class WindowChromeTests
{
    private const string Handler = "private void OnStripMouseDown(MouseEventArgs args)";

    /// <summary>
    /// A press that opens a drag is spent on the platform's move loop, so the second press of the pair only
    /// maximizes if the count is read before that loop starts.
    /// </summary>
    [Fact]
    public void The_strip_reads_the_click_count_before_it_hands_the_press_to_a_drag()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string body = StripHandler();

        int count = body.IndexOf("args.ClickCount", StringComparison.Ordinal);
        int drag = body.IndexOf("DragMove()", StringComparison.Ordinal);

        Assert.True(count >= 0, "the strip no longer looks at the click count, so a double-click only drags");
        Assert.True(drag >= 0, "the strip no longer starts a window drag at all");
        Assert.True(count < drag, "the strip starts the drag before reading the count, which eats the second press");
    }

    /// <summary>
    /// Double-click and the caption button are two ways of asking for the same thing, so they must not carry two
    /// answers: one of them would drift, and the glyph swap would keep following the button while the strip did
    /// something else.
    /// </summary>
    [Fact]
    public void The_strip_and_the_caption_button_share_one_maximize_toggle()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string source = LibrarySource.Read("Windowing/IslandsWindowChrome.cs");

        // The definition, the button's action, and the strip's call.
        Assert.Equal(3, source.Split("ToggleMaximize").Length - 1);

        // One place states what maximizing is.
        Assert.Equal(1, source.Split("_window.Maximize();").Length - 1);
        Assert.Equal(1, source.Split("_window.Restore();").Length - 1);

        Assert.Contains(Handler, source, StringComparison.Ordinal);
    }

    /// <summary>
    /// A dialog is not a document window: double-clicking its strip must not turn a small prompt into a
    /// full-screen surface. The guard has to sit before the maximize call, not after it.
    /// </summary>
    [Fact]
    public void A_window_that_cannot_maximize_ignores_the_strip_double_click()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        const string toggle = "private void ToggleMaximize()";
        string source = LibrarySource.Read("Windowing/IslandsWindowChrome.cs");
        int start = source.IndexOf(toggle, StringComparison.Ordinal);
        Assert.True(start >= 0, "ToggleMaximize is gone");
        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        string body = source[start..end];

        int guard = body.IndexOf("_window.CanMaximize", StringComparison.Ordinal);
        int maximize = body.IndexOf("_window.Maximize()", StringComparison.Ordinal);

        Assert.True(guard >= 0, "the maximize toggle no longer checks whether the window may maximize");
        Assert.True(maximize >= 0, "the maximize toggle no longer maximizes the window");
        Assert.True(guard < maximize, "the capability check must come before the maximize call");
    }

    /// <summary>
    /// The caption is the one part of a strip an app may have to change while the window is open, and rebuilding
    /// the strip to change it would leave the actions it hosts at their own height beside the fitted ones. A
    /// heading that never gets registered is a caption that silently stops following the window.
    /// </summary>
    [Fact]
    public void The_caption_of_every_strip_is_reachable_after_the_strip_is_built()
    {
        Assert.SkipWhen(LibrarySource.Root is null, "library sources are not next to the test output");

        string source = LibrarySource.Read("Windowing/IslandsWindowChrome.cs");

        Assert.Contains("public void SetTitle(string title)", source, StringComparison.Ordinal);

        // One heading is built per strip, and it is the one SetTitle writes to.
        Assert.Equal(1, source.Split("var heading = new TextBlock()").Length - 1);
        Assert.Equal(1, source.Split("_headings.Add(heading)").Length - 1);
        Assert.Equal(1, source.Split("foreach (TextBlock heading in _headings)").Length - 1);

        int register = source.IndexOf("_headings.Add(heading)", StringComparison.Ordinal);
        int returned = source.IndexOf("return new IslandsTitleBarLayout(", StringComparison.Ordinal);

        Assert.True(register < returned, "the heading is registered after its strip is handed back");
    }

    private static string StripHandler()
    {
        string source = LibrarySource.Read("Windowing/IslandsWindowChrome.cs");
        int start = source.IndexOf(Handler, StringComparison.Ordinal);
        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);

        return source[start..end];
    }
}

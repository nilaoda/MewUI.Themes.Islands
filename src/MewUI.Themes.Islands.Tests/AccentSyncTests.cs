using Xunit;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The variant accent hook, which is the one place the theme calls back into the application while the
/// application is telling it about a change.
/// </summary>
public class AccentSyncTests
{
    /// <summary>
    /// MewUI resolves a brand-new <c>Theme</c> on every <c>SetAccent</c> and decides whether anything
    /// changed by reference equality, so pushing an accent always re-raises <c>ThemeChanged</c>. A handler
    /// that calls <c>SetAccent</c> directly therefore calls itself until the process dies of a stack
    /// overflow, which is what it did before the push was put behind a guard.
    /// </summary>
    [Fact]
    public void The_accent_hook_pushes_only_through_the_reentrancy_guard()
    {
        string source = LibrarySource.Read("IslandsTheme.cs");

        Assert.Contains("if (_pushingAccent)", source);
        Assert.Contains("_accentHandler = (_, newTheme) => PushAccent(", source);

        // Exactly one call site, and it is the guarded helper's own.
        Assert.Equal(1, source.Split("application.SetAccent(").Length - 1);
        Assert.Contains("private static void PushAccent(Application application, Color accent)", source);
    }
}

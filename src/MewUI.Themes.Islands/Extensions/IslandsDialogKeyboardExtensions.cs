using Aprillz.MewUI;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands;

/// <summary>Opt-in keyboard actions for dialogs.</summary>
public static class IslandsDialogKeyboardExtensions
{
    /// <summary>Invokes the default action on an unhandled, unmodified Enter press.</summary>
    /// <remarks>
    /// Uses bubbling so focused buttons and controls that consume Enter keep their own behavior.
    /// IME composition and repeated key presses do not invoke the action. Supply the same validation
    /// callback used by the confirmation button, and an optional predicate for its enabled state.
    /// </remarks>
    public static TWindow IslandsDefaultAction<TWindow>(
        this TWindow window, Action action, Func<bool>? canExecute = null) where TWindow : Window
        => IslandsDialogKeyboard.Attach(window, action, canExecute);
}

using System.Runtime.CompilerServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Input;

namespace MewUI.Themes.Islands;

/// <summary>Opt-in keyboard actions for dialogs.</summary>
public static partial class IslandsDialogKeyboard
{
    /// <summary>Invokes the default action on an unhandled, unmodified Enter press.</summary>
    /// <remarks>
    /// Uses bubbling so focused buttons and controls that consume Enter keep their own behavior.
    /// IME composition and repeated key presses do not invoke the action. Supply the same validation
    /// callback used by the confirmation button, and an optional predicate for its enabled state.
    /// </remarks>
    public static TWindow IslandsDefaultAction<TWindow>(
        this TWindow window, Action action, Func<bool>? canExecute = null) where TWindow : Window
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(action);

        window.KeyDown += args =>
        {
            if (RouteKeypadEnter(window, args, keyUp: false, OperatingSystem.IsMacOS()))
            {
                return;
            }

            if (args.Handled || args.Key != Key.Enter || args.Modifiers != ModifierKeys.None || args.IsRepeat
                || window.FocusManager.FocusedElement is ITextCompositionClient { IsComposing: true }
                || canExecute?.Invoke() == false)
            {
                return;
            }

            args.Handled = true;
            action();
        };

        window.KeyUp += args => RouteKeypadEnter(window, args, keyUp: true, OperatingSystem.IsMacOS());
        return window;
    }

    // MewUI 0.22.1 maps macOS kVK_ANSI_KeypadEnter (0x4C) to Key.None. Re-route it as
    // Enter through the focused control first, so Cancel buttons and multiline editors retain
    // their normal behavior. The native backend has already processed IME before this event.
    internal static bool RouteKeypadEnter(Window window, KeyEventArgs args, bool keyUp, bool isMacOS)
    {
        if (!isMacOS || args.Handled || args.Key != Key.None || args.PlatformKey != 0x4C
            || window.FocusManager.FocusedElement is ITextCompositionClient { IsComposing: true })
        {
            return false;
        }

        var enter = new KeyEventArgs(Key.Enter, args.PlatformKey, args.Modifiers, args.IsRepeat);
        args.Handled = true;
        for (UIElement? current = window.FocusManager.FocusedElement ?? window;
             current is not null && !enter.Handled;
             current = current.Parent as UIElement)
        {
            if (keyUp)
            {
                RaiseKeyUp(current, enter);
            }
            else
            {
                RaiseKeyDown(current, enter);
            }
        }

        return true;
    }

    // Direct access to the framework's existing bubbling entry points remains compatible with
    // trimming and NativeAOT; no reflection or platform backend assembly is involved.
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "RaiseKeyDown")]
    private static extern void RaiseKeyDown(UIElement element, KeyEventArgs args);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "RaiseKeyUp")]
    private static extern void RaiseKeyUp(UIElement element, KeyEventArgs args);
}

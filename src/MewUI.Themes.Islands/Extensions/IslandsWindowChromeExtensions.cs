using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using MewUI.Themes.Islands.Theming;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands;

/// <summary>
/// Entry point for <see cref="IslandsWindowChrome"/>.
/// </summary>
public static class IslandsWindowChromeExtensions
{
    /// <summary>
    /// Creates chrome for a modal dialog with only a close button on every platform.
    /// Edge resizing remains governed by the window's size specification.
    /// </summary>
    /// <typeparam name="TWindow">Window type.</typeparam>
    /// <param name="window">The dialog to reshape, before showing it.</param>
    /// <returns>The chrome whose title strip can be added to the dialog layout.</returns>
    public static IslandsWindowChrome IslandsDialogChrome<TWindow>(this TWindow window)
        where TWindow : Window
    {
        ArgumentNullException.ThrowIfNull(window);

        // Native modal windows suppress these actions, but custom caption controls read the managed
        // capabilities. Set both explicitly so the controls and native window agree before realization.
        window.CanMinimize(false);
        window.CanMaximize(false);
        return window.IslandsFramelessChrome();
    }

    /// <summary>
    /// Gives up the platform's title bar: the client area takes it over, and the window's own strip becomes
    /// the title bar, the drag handle and the toolbar at once.
    /// </summary>
    /// <typeparam name="TWindow">Window type.</typeparam>
    /// <param name="window">The window to reshape.</param>
    /// <returns>
    /// The chrome, whose <see cref="IslandsWindowChrome.TitleBar(string, UIElement?)"/> builds the strip to dock at the top.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The window keeps its platform chrome buttons where the platform offers them — macOS keeps its traffic
    /// lights and hides the native title text — and takes its own elsewhere. The strip leaves room for them;
    /// see <see cref="IslandsWindowChrome"/> for why that is not knowable at build time.
    /// </para>
    /// <para>
    /// The top padding is dropped so the strip reaches the window's edge. The window's other insets are the
    /// theme's own container padding, so the window keeps exactly the breathing room it had.
    /// On Linux/X11 without extended-client-area support, the native title bar remains; the strip shows
    /// actions only and is hidden entirely when it has no actions, preserving native moving and resizing.
    /// </para>
    /// </remarks>
    public static IslandsWindowChrome IslandsFramelessChrome<TWindow>(this TWindow window)
        where TWindow : Window
    {
        ArgumentNullException.ThrowIfNull(window);

        window.ExtendClientAreaTitleBarHeight(IslandsMetrics.TitleBarHeight);

        // Top flush, the rest of the frame keeps the theme's container inset. Resolved from the theme rather
        // than repeated as a number, so a custom metrics record moves the window with it.
        window.WithTheme((theme, element) => element.Padding(new Thickness(
            theme.Metrics.ContainerPadding.Left,
            0,
            theme.Metrics.ContainerPadding.Right,
            theme.Metrics.ContainerPadding.Bottom)));

        var chrome = new IslandsWindowChrome(window);

        // The traffic light width is a property of the realised window, so the inset is refreshed on load.
        window.Loaded += chrome.ApplyInsets;

        // So is the middle of the space the platform was asked to centre in, which is only knowable once the
        // window has a screen and a size — and which the platform answers with a biased one before that.
        window.Loaded += chrome.ApplyPlacement;

        return chrome;
    }
}

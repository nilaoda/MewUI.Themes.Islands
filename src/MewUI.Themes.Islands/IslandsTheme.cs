using Aprillz.MewUI;

namespace MewUI.Themes.Islands;

/// <summary>
/// Applies the Islands style layer to a running MewUI application.
/// </summary>
/// <remarks>
/// Use <see cref="IslandsBuilderExtensions.UseIslands"/> from the application builder; it defers
/// <see cref="Install"/> to the startup callback, which runs after the application object exists but
/// before the main window is shown — the only window in which the application style sheet can still
/// be extended.
/// </remarks>
public static class IslandsTheme
{
    private static readonly object _gate = new();

    private static IslandsPalette _palette = IslandsPalette.Default;
    private static Application? _accentOwner;
    private static Action<Theme, Theme>? _accentHandler;
    private static bool _pushingAccent;

    /// <summary>Whether the Islands style layer has been installed into the running application.</summary>
    public static bool IsInstalled { get; private set; }

    /// <summary>The palette handed to <see cref="Install"/>; the default IntelliJ + Darcula pair otherwise.</summary>
    public static IslandsPalette Palette => _palette;

    /// <summary>The color table for a theme's variant. Read it inside drawing code, never cache it.</summary>
    public static IslandsVariantColors Colors(Theme theme) => _palette.Of(theme);

    /// <summary>The color table of the theme the application is currently rendering.</summary>
    public static IslandsVariantColors CurrentColors
    {
        get
        {
            if (!Application.IsRunning)
            {
                throw new InvalidOperationException("IslandsTheme.CurrentColors requires a running application.");
            }

            return _palette.Of(Application.Current.Theme);
        }
    }

    /// <summary>
    /// Registers every Islands type rule and named style into <see cref="Application.StyleSheet"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The application is not running yet, or its style sheet already froze because a window resolved
    /// styles before the theme was installed. Both mean styles would be dropped silently, so they fail
    /// loudly instead.
    /// </exception>
    public static void Install(IslandsOptions? options = null)
    {
        lock (_gate)
        {
            if (IsInstalled)
            {
                return;
            }

            if (!Application.IsRunning)
            {
                throw new InvalidOperationException(
                    "Islands must be installed after the application starts. Call Application.Create().UseIslands() instead.");
            }

            var application = Application.Current;
            var sheet = application.StyleSheet;
            if (sheet.IsFrozen)
            {
                throw new InvalidOperationException(
                    "The application style sheet is already frozen: a control resolved its style before Islands " +
                    "was installed. Call UseIslands() on the builder, before BuildMainWindow().");
            }

            _palette = options?.Palette ?? IslandsPalette.Default;

            // Only add rules to the default instance; never replace StyleSheet. The default instance carries
            // UsesFrameworkNamedStyles = true, and after a replacement the framework's built-in named styles
            // (combobox-popup and friends) would only be reachable through the fallback lookup.
            IslandsStyles.Register(sheet);

            IsInstalled = true;

            if (options?.SyncAccentWithVariant ?? true)
            {
                SubscribeAccent(application);
            }
        }
    }

    private static void SubscribeAccent(Application application)
    {
        if (ReferenceEquals(_accentOwner, application))
        {
            return;
        }

        // ThemeChanged covers both ways the variant flips: an explicit SetThemeMode and a system appearance
        // change (the latter never raises ThemeModeChanged).
        _accentHandler = (_, newTheme) => PushAccent(application, _palette.Of(newTheme).Accent);
        application.ThemeChanged += _accentHandler;
        _accentOwner = application;

        PushAccent(application, _palette.Of(application.Theme).Accent);
    }

    /// <summary>
    /// Pushes the accent of a variant. The nested <c>ThemeChanged</c> this raises is suppressed: MewUI
    /// resolves a brand-new <c>Theme</c> on every <c>SetAccent</c> and reports the change by reference
    /// equality, so the event always re-fires and the callback would otherwise call itself to a stack
    /// overflow. The re-broadcast already carries the new accent, so nothing is lost by not re-entering.
    /// </summary>
    private static void PushAccent(Application application, Color accent)
    {
        if (_pushingAccent)
        {
            return;
        }

        _pushingAccent = true;
        try
        {
            application.SetAccent(accent);
        }
        finally
        {
            _pushingAccent = false;
        }
    }
}

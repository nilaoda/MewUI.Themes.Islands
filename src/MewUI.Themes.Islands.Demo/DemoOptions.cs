using Aprillz.MewUI;
using MewUI.Themes.Islands;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Demo;

/// <summary>
/// Startup options. They exist so screenshots are reproducible:
/// <c>--theme dark --density compact --page controls</c>.
/// </summary>
internal sealed record DemoOptions(ThemeVariant Theme, IslandsDensity Density, string InitialPage)
{
    public static DemoOptions Parse(string[] args)
    {
        var theme = ThemeVariant.System;
        var density = IslandsDensity.Regular;
        var page = "Overview";

        for (var i = 0; i < args.Length; i++)
        {
            var value = i + 1 < args.Length ? args[i + 1] : string.Empty;

            switch (args[i])
            {
                case "--theme":
                    theme = value.ToLowerInvariant() switch
                    {
                        "light" => ThemeVariant.Light,
                        "dark" => ThemeVariant.Dark,
                        _ => ThemeVariant.System,
                    };
                    i++;
                    break;

                case "--density":
                    density = value.Equals("compact", StringComparison.OrdinalIgnoreCase)
                        ? IslandsDensity.Compact
                        : IslandsDensity.Regular;
                    i++;
                    break;

                case "--page":
                    page = value;
                    i++;
                    break;
            }
        }

        return new DemoOptions(theme, density, page);
    }
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace MewUI.Themes.Islands.Menus;

/// <summary>One row of an <see cref="IslandsContextMenu"/>. Separators carry no text or action.</summary>
public sealed class IslandsMenuItem
{
    public string Text { get; init; } = string.Empty;

    /// <summary>Builds the 16 DIP glyph for this row; called once per open.</summary>
    public Func<FrameworkElement>? Glyph { get; init; }

    /// <summary>The action run after the menu has closed.</summary>
    public Action? Invoke { get; init; }

    /// <summary>Evaluated when the menu opens.</summary>
    public Func<bool>? IsEnabled { get; init; }

    public bool IsSeparator { get; init; }

    public static IslandsMenuItem Separator { get; } = new() { IsSeparator = true };
}

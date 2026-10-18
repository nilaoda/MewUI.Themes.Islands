using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Controls;

/// <summary>A navigation island and a content island, separated by a draggable gutter.</summary>
/// <remarks>The host supplies the panes and optional tool rail. No application commands or text are owned here.</remarks>
public sealed class IslandsWorkspace
{
    public IslandsWorkspace(UIElement navigation, UIElement content, UIElement? toolRail = null,
        double navigationWidth = 224, double minNavigationWidth = 180, double minContentWidth = 560)
    {
        ArgumentNullException.ThrowIfNull(navigation);
        ArgumentNullException.ThrowIfNull(content);

        Panes = new SplitPanel()
            .Horizontal()
            .FirstLength(new GridLength(navigationWidth, GridUnitType.Pixel))
            .SecondLength(GridLength.Star)
            .MinFirst(minNavigationWidth)
            .MaxFirst(400)
            .MinSecond(minContentWidth)
            .SplitterThickness(4)
            .First(navigation)
            .Second(content);

        Element = toolRail is null ? Panes : new DockPanel().Spacing(0).Children(toolRail.DockLeft(), Panes);
    }

    /// <summary>The layout to place beneath the application's title bar.</summary>
    public UIElement Element { get; }

    /// <summary>The splitter, exposed so the host can persist its position.</summary>
    public SplitPanel Panes { get; }
}

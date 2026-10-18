using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Styles;

/// <summary>
/// The surfaces an application lays out of plain borders: a panel, a card, a toolbar strip, a status bar.
/// </summary>
/// <remarks>
/// These are named styles rather than a type rule on purpose: <see cref="Border"/> is the framework's
/// generic rectangle, and recoloring the type would restyle every hairline and clip box in the tree. An
/// application asks for one of them by name, through the extensions in <c>IslandsExtensions</c>.
/// </remarks>
internal static class IslandsSurfaceStyles
{

    internal const string Panel = "islands-panel";
    internal const string Card = "islands-card";
    internal const string Toolbar = "islands-toolbar";
    internal const string StatusBar = "islands-status-bar";
    internal const string DialogFooter = "islands-dialog-footer";

    /// <summary>Islands' signature surface: a rounded panel one layer above the application background.</summary>
    internal const string Island = "islands-island";

    /// <summary>The content layer, for a pane that sits *inside* an island and shares its surface.</summary>
    internal const string Content = "islands-content";

    internal static void Register(StyleSheet sheet)
    {
        sheet.Define(Panel, static () => Surface([
            IslandsStyles.Background(c => c.PanelBackground),
            IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
        ]));

        // The island: Islands' signature rounded panel — the surface one layer *above* the window, which is
        // what makes it read as an island rather than as part of the wall. In light the window is the
        // layer-1 grey and an island on it is layer-2 white; in dark the window is layer-0 and the island is
        // layer-1. Both are this theme's component surface, and its 20 DIP corner is the rest of the name.
        //
        // The border is the island's air, not a line on it: Island/borderColor is the tool window's own
        // background, so the six DIP it takes are painted in the colour of whatever the island is standing on
        // and read as a gap. Two islands side by side therefore come out twelve apart, and an island against
        // the window's edge keeps the same margin — without the application arranging either.
        sheet.Define(Island, static () => Surface([
            IslandsStyles.Background(c => c.ComponentBackground),
            IslandsStyles.BorderBrush(c => c.PanelBackground),
            IslandsStyles.Fixed(Control.BorderThicknessProperty, IslandsMetrics.IslandBorderWidth),
            IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.IslandArc),
            IslandsStyles.Metric(Control.PaddingProperty, t => t.Metrics.ContainerPadding),
        ]));

        // Content inside an island shares the island's layer: a pane that painted the window's own layer here
        // would read as a darker plate inset into the panel.
        sheet.Define(Content, static () => Surface([
            IslandsStyles.Background(c => c.ComponentBackground),
            IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
        ]));

        // A card is the component surface with a component's edge and arc: Islands's titled pane.
        sheet.Define(Card, static () => Surface([
            IslandsStyles.Background(c => c.ComponentBackground),
            IslandsStyles.BorderBrush(c => c.Border),
            IslandsStyles.Metric(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
            IslandsStyles.Fixed(Control.CornerRadiusProperty, IslandsMetrics.ComponentArc),
            IslandsStyles.Metric(Control.PaddingProperty, t => t.Metrics.ContainerPadding),
        ]));

        // Islands's ToolBar and its status bar share one surface: the panel, tighter than a component.
        sheet.Define(Toolbar, static () => Surface([
            IslandsStyles.Background(c => c.ToolBarBackground),
            IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
            IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ToolBarPadding),
        ]));

        sheet.Define(StatusBar, static () => Surface([
            IslandsStyles.Background(c => c.ToolBarBackground),
            IslandsStyles.Fixed(Control.BorderThicknessProperty, 0.0),
            IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.ComponentPadding),
        ]));

        // The strip under a window's content: the wall's colour, one hairline, and one inset for every
        // window. Dialogs are different sizes, but their button rows should end at the same distance from
        // the bottom edge; doing that per window is how the margins drifted apart in the first place.
        sheet.Define(DialogFooter, static () => Surface([
            IslandsStyles.Background(c => c.PanelBackground),
            IslandsStyles.BorderBrush(c => c.Border),
            IslandsStyles.Fixed(Border.NonUniformBorderThicknessProperty, new Thickness(0, 1, 0, 0)),
            IslandsStyles.Fixed(Control.PaddingProperty, IslandsMetrics.DialogFooterPadding),
        ]));
    }

    private static Style Surface(SetterBase[] setters) =>
        Style.DeriveFromDefault<Border>(setters);
}

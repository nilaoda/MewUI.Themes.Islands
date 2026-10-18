using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// A grid copies the theme's item padding into <c>CellPadding</c> while it is still being constructed, and a
/// local value outranks a style — so the cell insets are a metric the type rules cannot deliver. Islands's
/// table wants its own margins there, tighter than a list row's, and they have to be committed the same way
/// the constructor committed its copy.
/// </summary>
public class GridCellMarginTests
{
    [Fact]
    public void The_extension_commits_expuis_table_cell_margins()
    {
        GridView grid = new GridView().IslandsCellMargins();

        Assert.Equal(IslandsMetrics.GridCellPadding, grid.CellPadding);

        // Table.cellMargins = 2,3,2,3; the theme keeps the 2 vertical and widens the horizontal to 8 — at 3
        // an identifier or a code sat against the column line (see the deviation note in docs/color-map.md).
        Assert.Equal(8, grid.CellPadding.Left);
        Assert.Equal(8, grid.CellPadding.Right);
        Assert.Equal(2, grid.CellPadding.Top);
        Assert.Equal(2, grid.CellPadding.Bottom);
    }
}

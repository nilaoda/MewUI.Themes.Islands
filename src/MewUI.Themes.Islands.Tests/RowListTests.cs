using Aprillz.MewUI.Controls;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

public class RowListTests
{
    [Fact]
    public void Single_selection_restoration_retains_only_the_first_visible_row()
    {
        var list = new IslandsRowList(allowMultipleSelection: false);
        var first = new object();
        var second = new object();
        list.AddRow(first, new TextBlock());
        list.AddRow(second, new TextBlock());

        list.SelectMany([second, first]);

        Assert.Equal([first], list.SelectedItems);
        Assert.Same(first, list.SelectedItem);
    }

    [Fact]
    public void Restoring_a_selection_ignores_missing_rows_and_keeps_display_order()
    {
        var list = new IslandsRowList();
        var first = new object();
        var second = new object();
        list.AddHeader(new TextBlock());
        list.AddRow(first, new TextBlock());
        list.AddRow(second, new TextBlock());

        list.SelectMany([second, new object(), first, second]);

        Assert.Equal([first, second], list.SelectedItems);
        Assert.Same(first, list.SelectedItem);
        Assert.Equal(2, list.Items.Count);
    }

    [Fact]
    public void A_collapsed_or_filtered_selection_can_be_cleared_and_the_next_row_selected()
    {
        var list = new IslandsRowList();
        var old = new object();
        list.AddRow(old, new TextBlock());
        list.Select(old);
        list.Clear();
        var current = new object();
        list.AddRow(current, new TextBlock());

        var changes = 0;
        list.SelectionChanged += () => changes++;
        list.SelectMany([old]);
        Assert.Empty(list.SelectedItems);
        Assert.Null(list.SelectedItem);
        list.Select(current);
        Assert.Same(current, list.SelectedItem);
        Assert.Equal(2, changes);
    }

    [Fact]
    public void Duplicate_row_objects_are_rejected_before_mutating_the_list()
    {
        var list = new IslandsRowList();
        var item = new object();
        list.AddRow(item, new TextBlock());

        Assert.Throws<ArgumentException>(() => list.AddRow(item, new TextBlock()));
        Assert.Single(list.Items);
    }
}

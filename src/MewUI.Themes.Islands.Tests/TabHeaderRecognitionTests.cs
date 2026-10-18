using Xunit;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// The header button's plate is named onto an element the drawer finds under the tabs, because the element's
/// type is internal. These tests pin what that lookup is allowed to treat as a header: naming the plate onto
/// anything else hands it a hover fill that repaints whatever that element covers.
/// </summary>
public class TabHeaderRecognitionTests
{
    private const string HeaderText = "Settings";

    private static TabControl Tabs()
    {
        var tabs = new TabControl();
        tabs.AddTabs(new TabItem().Header(HeaderText).Content(new ScrollViewer()));
        return tabs;
    }

    /// <summary>The one header the tabs realize answers; the page they realize beside it must not.</summary>
    [Fact]
    public void Only_the_realized_header_answers()
    {
        TabControl tabs = Tabs();

        var headers = new List<ContentControl>();
        VisualTree.Visit(tabs, child =>
        {
            if (child is ContentControl control && IslandsTabPlate.IsHeader(tabs, control))
            {
                headers.Add(control);
            }
        });

        ContentControl header = Assert.Single(headers);
        Assert.NotNull(header.Content);

        ScrollViewer page = (ScrollViewer)tabs.Tabs[0].Content!;
        Assert.False(IslandsTabPlate.IsHeader(tabs, page));
    }
}

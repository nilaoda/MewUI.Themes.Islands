using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

public class DialogChromeTests
{
    [Fact]
    public void A_resizable_dialog_has_only_close_capability_and_keeps_edge_resizing()
    {
        var dialog = new Window().Resizable(960, 600, minWidth: 900, minHeight: 460);
        var size = dialog.WindowSize;

        dialog.IslandsDialogChrome();

        Assert.False(dialog.CanMinimize);
        Assert.False(dialog.CanMaximize);
        Assert.True(dialog.CanClose);
        Assert.Equal(size, dialog.WindowSize);
    }

    [Fact]
    public void Ordinary_window_chrome_keeps_minimize_and_maximize_available()
    {
        var window = new Window().Resizable(1180, 720);

        window.IslandsFramelessChrome();

        Assert.True(window.CanMinimize);
        Assert.True(window.CanMaximize);
    }
}

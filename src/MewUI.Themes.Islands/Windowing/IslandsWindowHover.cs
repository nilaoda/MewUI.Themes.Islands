using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace MewUI.Themes.Islands.Windowing;

/// <summary>Retires hover feedback when input moves to another window.</summary>
internal static class IslandsWindowHover
{
    internal static void Attach(Window window)
    {
        window.Deactivated += () =>
        {
            window.ClearMouseOver();
            AfterInput(() =>
            {
                if (!window.IsActive)
                {
                    window.ClearMouseOver();
                }
            });
        };

        window.Activated += () =>
        {
            ClearOwners(window);
            AfterInput(() =>
            {
                if (window.IsActive)
                {
                    ClearOwners(window);
                }
            });
        };
    }

    private static void ClearOwners(Window window)
    {
        // Include the whole chain: a nested dialog also supersedes the main window's hover feedback.
        for (var owner = window.Owner; owner != null; owner = owner.Owner)
        {
            owner.ClearMouseOver();
        }
    }

    private static void AfterInput(Action action)
    {
        if (Application.IsRunning)
        {
            // MewUI 0.22.1 re-evaluates hover after MouseUp and after releasing capture, even if the
            // click opened a modal window. Run after that input and its queued layout work so a
            // tooltip cannot reappear in the now inactive owner. ClearMouseOver raises MouseLeave,
            // closing the tooltip through the framework's public lifecycle without reflection.
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background, action);
        }
    }
}

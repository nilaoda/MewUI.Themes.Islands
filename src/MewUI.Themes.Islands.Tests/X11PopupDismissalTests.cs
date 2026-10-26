using System.Reflection;
using System.Runtime.InteropServices;
using MewUI.Themes.Islands.Styles;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Windowing;
using Xunit;

namespace MewUI.Themes.Islands.Tests;

public class X11PopupDismissalTests
{
    [Fact]
    public void The_optional_X11_event_contract_is_read_without_a_platform_reference_in_the_library()
    {
        var args = (X11NativeMessageEventArgs)Activator.CreateInstance(typeof(X11NativeMessageEventArgs),
            BindingFlags.Instance | BindingFlags.NonPublic, null, [10, (nint)123], null)!;

        Assert.True(IslandsX11PopupDismissal.X11Message.TryRead(args, out int type, out nint pointer));
        Assert.Equal(10, type);
        Assert.Equal((nint)123, pointer);
        Assert.False(IslandsX11PopupDismissal.X11Message.TryRead(new NativeMessageEventArgs(), out _, out _));
    }

    [Fact]
    public void Theme_installation_enables_the_workaround_only_on_Linux()
    {
        var sheet = new StyleSheet();
        IslandsStyles.Register(sheet);
        var setter = Assert.Single(sheet.GetByType(typeof(Window))!.Setters,
            setter => ReferenceEquals(setter.Property, IslandsX11PopupDismissal.EnabledProperty));
        Assert.Equal(OperatingSystem.IsLinux(), setter.Value);

        // Assigning a style sheet refreshes the window's type rule without a platform host.
        var window = new Window { StyleSheet = sheet };
        var handlers = typeof(Window).GetProperty("HasNativeMessageHandler", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.Equal(OperatingSystem.IsLinux(), handlers.GetValue(window));
    }

    [Theory]
    [InlineData(1, 0, true)]
    [InlineData(1, 2, false)]
    [InlineData(1, 5, false)]
    [InlineData(0, 0, false)]
    public void Only_a_WM_grab_needs_the_extra_focus_dismissal(int mode, int detail, bool expected)
    {
        // Marshal the backend's real structure to verify our prefix against the upstream ABI.
        var type = typeof(X11NativeMessageEventArgs).Assembly.GetType("Aprillz.MewUI.Native.XFocusChangeEvent", true)!;
        var focus = Activator.CreateInstance(type)!;
        type.GetField("mode")!.SetValue(focus, mode);
        type.GetField("detail")!.SetValue(focus, detail);
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf(type));
        try
        {
            Marshal.StructureToPtr(focus, pointer, false);
            Assert.Equal(expected, IslandsX11PopupDismissal.IsWindowManagerFocusGrab(pointer));
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void A_framework_transfer_to_a_submenu_does_not_look_like_a_stolen_native_grab()
    {
        ref var holder = ref IslandsX11PopupDismissal.CaptureHolder(null!);
        ref var watches = ref IslandsX11PopupDismissal.ArmedWatches(null!);
        var previousHolder = holder;
        var previousWatches = watches;
        try
        {
            var parent = new Window();
            var submenu = new Window();
            watches = [parent, submenu];
            holder = parent;
            Assert.True(IslandsX11PopupDismissal.HoldsPopupWatch(parent));

            holder = submenu;
            Assert.False(IslandsX11PopupDismissal.HoldsPopupWatch(parent));
            Assert.True(IslandsX11PopupDismissal.HoldsPopupWatch(submenu));

            watches.Remove(submenu);
            Assert.False(IslandsX11PopupDismissal.HoldsPopupWatch(submenu));
        }
        finally
        {
            holder = previousHolder;
            watches = previousWatches;
        }
    }

    [Fact]
    public void Dismissal_closes_only_the_owners_popup_chain_and_tolerates_registry_changes()
    {
        var owner = new Window();
        var other = new Window();
        var windows = new List<Window> { owner, other };
        int closed = 0;
        var parent = Popup(owner, () => closed++);
        var child = Popup(parent, () => { closed++; windows.Clear(); });
        var unrelated = Popup(other, () => throw new InvalidOperationException("wrong owner"));
        windows.AddRange([parent, child, unrelated]);

        IslandsX11PopupDismissal.DismissOwnedPopups(owner, windows);
        Assert.Equal(2, closed);
    }

    private static Window Popup(Window owner, Action dismiss)
    {
        var type = typeof(Window).Assembly.GetType("Aprillz.MewUI.PopupWindow", throwOnError: true)!;
        var popup = (Window)Activator.CreateInstance(type, [new Border(), new Size(100, 80), true])!;
        typeof(Window).GetProperty(nameof(Window.Owner))!.SetValue(popup, owner);
        type.GetProperty("DismissRequested", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(popup, dismiss);
        return popup;
    }
}

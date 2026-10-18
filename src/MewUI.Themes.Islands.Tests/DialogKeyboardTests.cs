using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Input;
using Xunit;
using MewUI.Themes.Islands.Windowing;

namespace MewUI.Themes.Islands.Tests;

public class DialogKeyboardTests
{
    [Fact]
    public void Enter_runs_the_confirmation_callback_and_observes_its_enabled_state()
    {
        var enabled = false;
        var calls = 0;
        var window = new KeyboardWindow().IslandsDefaultAction(() => calls++, () => enabled);

        var disabledPress = new KeyEventArgs(Key.Enter, 0);
        window.Send(disabledPress);
        Assert.Equal(0, calls);
        Assert.False(disabledPress.Handled);

        enabled = true;
        var press = new KeyEventArgs(Key.Enter, 0);
        window.Send(press);
        Assert.Equal(1, calls);
        Assert.True(press.Handled);
    }

    [Theory]
    [InlineData(Key.Enter, ModifierKeys.None, false, true)]
    [InlineData(Key.Enter, ModifierKeys.None, true, false)]
    [InlineData(Key.Enter, ModifierKeys.Shift, false, false)]
    [InlineData(Key.Enter, ModifierKeys.Control, false, false)]
    [InlineData(Key.Enter, ModifierKeys.Alt, false, false)]
    [InlineData(Key.Enter, ModifierKeys.Meta, false, false)]
    [InlineData(Key.Escape, ModifierKeys.None, false, false)]
    public void Handled_keys_repeats_and_other_shortcuts_do_not_confirm(
        Key key, ModifierKeys modifiers, bool repeat, bool handled)
    {
        var calls = 0;
        var window = new KeyboardWindow().IslandsDefaultAction(() => calls++);
        var press = new KeyEventArgs(key, 0, modifiers, repeat) { Handled = handled };

        window.Send(press);

        Assert.Equal(0, calls);
        Assert.Equal(handled, press.Handled);
    }

    [Fact]
    public void Enter_does_not_confirm_while_the_focused_editor_is_composing()
    {
        var calls = 0;
        var editor = new TextBox();
        var window = new KeyboardWindow().IslandsDefaultAction(() => calls++);
        window.Content(editor);
        Assert.True(window.FocusManager.SetFocus(editor));
        var client = (ITextCompositionClient)editor;
        client.HandleTextCompositionStart(new TextCompositionEventArgs());
        Assert.True(client.IsComposing);

        var composingPress = new KeyEventArgs(Key.Enter, 0);
        window.Send(composingPress);
        Assert.Equal(0, calls);
        Assert.False(composingPress.Handled);

        client.HandleTextCompositionEnd(new TextCompositionEventArgs());
        window.Send(new KeyEventArgs(Key.Enter, 0));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Mac_keypad_enter_confirms_from_a_text_field()
    {
        var calls = 0;
        var editor = new TextBox();
        var window = new KeyboardWindow().IslandsDefaultAction(() => calls++);
        window.Content(editor);
        Assert.True(window.FocusManager.SetFocus(editor));

        var press = new KeyEventArgs(Key.None, 0x4C);
        Assert.True(IslandsDialogKeyboard.RouteKeypadEnter(window, press, false, true));
        Assert.True(press.Handled);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Mac_keypad_enter_activates_the_focused_cancel_button_instead_of_confirming()
    {
        var confirmed = 0;
        var cancelled = 0;
        var cancel = new Button().OnClick(() => cancelled++);
        var window = new KeyboardWindow().IslandsDefaultAction(() => confirmed++);
        window.Content(cancel);
        Assert.True(window.FocusManager.SetFocus(cancel));

        IslandsDialogKeyboard.RouteKeypadEnter(window, new KeyEventArgs(Key.None, 0x4C), false, true);
        IslandsDialogKeyboard.RouteKeypadEnter(window, new KeyEventArgs(Key.None, 0x4C), true, true);

        Assert.Equal(1, cancelled);
        Assert.Equal(0, confirmed);
    }

    [Fact]
    public void Mac_keypad_enter_keeps_multiline_editing_behavior()
    {
        var calls = 0;
        var editor = new MultiLineTextBox();
        var window = new KeyboardWindow().IslandsDefaultAction(() => calls++);
        window.Content(editor);
        Assert.True(window.FocusManager.SetFocus(editor));

        IslandsDialogKeyboard.RouteKeypadEnter(window, new KeyEventArgs(Key.None, 0x4C), false, true);

        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false, 0x4C, ModifierKeys.None, false)]
    [InlineData(true, 0x4D, ModifierKeys.None, false)]
    [InlineData(true, 0x4C, ModifierKeys.Shift, false)]
    [InlineData(true, 0x4C, ModifierKeys.None, true)]
    public void Keypad_fallback_does_not_confirm_other_keys_shortcuts_or_repeats(
        bool macOS, int keyCode, ModifierKeys modifiers, bool repeat)
    {
        var calls = 0;
        var window = new KeyboardWindow().IslandsDefaultAction(() => calls++);
        IslandsDialogKeyboard.RouteKeypadEnter(window, new KeyEventArgs(Key.None, keyCode, modifiers, repeat), false, macOS);
        Assert.Equal(0, calls);
    }

    private sealed class KeyboardWindow : Window
    {
        public void Send(KeyEventArgs args) => OnKeyDown(args);
    }
}

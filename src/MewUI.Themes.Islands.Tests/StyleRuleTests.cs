using Xunit;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewUI.Themes.Islands.Drawing;
using MewUI.Themes.Islands.Styles;
using MewUI.Themes.Islands.Theming;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// A derived style's own base setter outranks a trigger of the style it is BasedOn to, so any state a
/// framework default reacts to in a property we paint stops reacting unless the rule restates it. These
/// tests are the list of what has to keep reacting.
/// </summary>
public class StyleRuleTests
{
    private readonly record struct State(VisualStateFlags Match, VisualStateFlags Exclude)
    {
        public static State On(VisualStateFlags flags) => new(flags, VisualStateFlags.None);

        /// <summary>A state that needs one flag present and another absent, like a checked-but-enabled toggle.</summary>
        public static State On(VisualStateFlags flags, VisualStateFlags except) => new(flags, except);

        /// <summary>A state that must be absent, which is how the theme spells "disabled".</summary>
        public static State Off(VisualStateFlags flags) => new(VisualStateFlags.None, flags);
    }

    private static readonly (Type Control, State[] States)[] RestatedStates =
    [
        (typeof(Button), [State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed), State.Off(VisualStateFlags.Enabled)]),
        (typeof(ToggleButton),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed),
            State.On(VisualStateFlags.Checked), State.On(VisualStateFlags.Checked | VisualStateFlags.Hot),
            State.On(VisualStateFlags.Checked | VisualStateFlags.Focused), State.On(VisualStateFlags.Checked | VisualStateFlags.Pressed),
            State.On(VisualStateFlags.Checked, VisualStateFlags.Enabled), State.Off(VisualStateFlags.Enabled),
         ]),
        (typeof(SplitButton), [State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed), State.Off(VisualStateFlags.Enabled)]),
        (typeof(DropDownButton), [State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed), State.Off(VisualStateFlags.Enabled)]),
        (typeof(DropDownBase),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Active),
            State.On(VisualStateFlags.Pressed), State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(ToolBar), []),
        (typeof(ComboBox),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Active),
            State.On(VisualStateFlags.Pressed), State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(ListBox), []),
        (typeof(TextBase),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed),
            State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(NumericUpDown),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed),
            State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(CheckBox),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed),
            State.On(VisualStateFlags.Checked), State.On(VisualStateFlags.Checked | VisualStateFlags.Hot),
            State.On(VisualStateFlags.Indeterminate), State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(RadioButton),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed),
            State.On(VisualStateFlags.Checked), State.On(VisualStateFlags.Checked | VisualStateFlags.Hot),
            State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(ToggleSwitch),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed),
            State.On(VisualStateFlags.Checked), State.On(VisualStateFlags.Checked | VisualStateFlags.Hot),
            State.On(VisualStateFlags.Checked | VisualStateFlags.Pressed),
            State.On(VisualStateFlags.Checked | VisualStateFlags.Focused),
            State.On(VisualStateFlags.Checked, VisualStateFlags.Enabled),
            State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(Expander), [State.On(VisualStateFlags.Focused), State.Off(VisualStateFlags.Enabled)]),
        (typeof(SegmentedControl),
         [State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid)]),
        (typeof(SegmentButton),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Pressed), State.On(VisualStateFlags.Selected),
            State.On(VisualStateFlags.Selected | VisualStateFlags.Hot), State.On(VisualStateFlags.Selected, VisualStateFlags.Enabled),
            State.Off(VisualStateFlags.Enabled),
         ]),
        (typeof(ScrollableItemsBase), [State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid)]),
        (typeof(ItemContainer), [State.On(VisualStateFlags.Selected), State.Off(VisualStateFlags.Enabled)]),
        (typeof(GridView), [State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid)]),
        (typeof(GridViewRow), [State.On(VisualStateFlags.Selected), State.Off(VisualStateFlags.Enabled)]),
        (typeof(TreeView), [State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid)]),
        (typeof(MenuBar), [State.Off(VisualStateFlags.Enabled)]),
        (typeof(ContextMenu), [State.Off(VisualStateFlags.Enabled)]),
        (typeof(TabControl), [State.Off(VisualStateFlags.Enabled)]),
        // The base of scroll viewer, user control and headered content control: a state fill here would paint
        // the page those containers fill, which is what the tab plate used to do.
        (typeof(ContentControl), []),
        (typeof(Window), []),
        (typeof(GroupBox), [State.Off(VisualStateFlags.Enabled)]),
        (typeof(Label), [State.Off(VisualStateFlags.Enabled)]),
        (typeof(ToolTip), []),
        (typeof(ScrollBar), [State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Pressed), State.Off(VisualStateFlags.Enabled)]),
        (typeof(Slider),
         [
            State.On(VisualStateFlags.Hot), State.On(VisualStateFlags.Focused), State.On(VisualStateFlags.Pressed),
            State.Off(VisualStateFlags.Enabled), State.On(VisualStateFlags.Invalid),
         ]),
        (typeof(ProgressBar), []),
    ];

    /// <summary>
    /// A fresh sheet per test: the type-rule lookup is the same one an application does, and the library
    /// only ever adds rules to it.
    /// </summary>
    private static StyleSheet Registered()
    {
        var sheet = new StyleSheet();
        IslandsStyles.Register(sheet);
        return sheet;
    }

    [Fact]
    public void Every_covered_control_resolves_its_own_expui_rule()
    {
        StyleSheet sheet = Registered();

        foreach ((Type control, State[] states) in RestatedStates)
        {
            Style? style = sheet.GetByType(control);

            Assert.NotNull(style);

            // Exactly this type, not a family member's rule: ScrollBar, Slider, ProgressBar and
            // NumericUpDown share RangeBase, and one rule on that base would color all four with the
            // properties of one of them.
            Assert.Equal(control, style!.TargetType);
            Assert.NotNull(style.BasedOn);
        }
    }

    [Fact]
    public void The_toggle_switch_is_a_fixed_islands_capsule()
    {
        StyleSheet sheet = Registered();
        Style style = sheet.GetByType(typeof(ToggleSwitch))!;

        AssertSetterValue(style, FrameworkElement.MinWidthProperty, IslandsSwitchIndicator.SwitchWidth);
        AssertSetterValue(style, FrameworkElement.MinHeightProperty, IslandsSwitchIndicator.SwitchHeight);
        AssertSetterValue(style, FrameworkElement.WidthProperty, IslandsSwitchIndicator.SwitchWidth);
        AssertSetterValue(style, FrameworkElement.HeightProperty, IslandsSwitchIndicator.SwitchHeight);
        AssertSetterValue(style, Control.PaddingProperty, Thickness.Zero);
        Assert.Contains(style.Setters, setter => ReferenceEquals(setter.Property, Control.TemplateProperty));
        Assert.Contains(style.Setters, setter => ReferenceEquals(setter.Property, ToggleSwitch.ThumbBrushProperty));
    }

    [Fact]
    public void Every_painted_state_is_restateened_by_the_rule_that_paints_it()
    {
        StyleSheet sheet = Registered();

        foreach ((Type control, State[] expected) in RestatedStates)
        {
            Style style = sheet.GetByType(control)!;
            var declared = style.Triggers
                .Select(trigger => new State(trigger.Match, trigger.Exclude))
                .ToHashSet();

            foreach (State state in expected)
            {
                Assert.True(
                    declared.Contains(state),
                    $"{control.Name} no longer reacts to {state.Match} without {state.Exclude}; triggers: {string.Join(", ", declared)}");
            }
        }
    }

    /// <summary>A state key declared twice never fires the second time, because the first one wins.</summary>
    [Fact]
    public void No_rule_declares_the_same_state_twice()
    {
        StyleSheet sheet = Registered();

        foreach ((Type control, State[] _) in RestatedStates)
        {
            Style style = sheet.GetByType(control)!;
            var keys = style.Triggers
                .Select(trigger => new State(trigger.Match, trigger.Exclude))
                .ToList();

            Assert.Equal(keys.Count, keys.Distinct().Count());
        }
    }

    /// <summary>
    /// Islands does state the row hover — <c>List.hoverBackground</c>, <c>Tree.hoverBackground</c> and
    /// <c>Table.hoverBackground</c> all resolve to <c>selection-bg-hovered</c> — and the framework's own
    /// trigger cannot supply it, because an ordinary setter of the row's style outranks a trigger of the style
    /// it is based on: the transparent idle fill that keeps a list unstriped also drops the derived tint. Both
    /// halves matter, so both are pinned here.
    /// </summary>
    [Fact]
    public void Rows_carry_the_hover_plate_islands_states()
    {
        StyleSheet sheet = Registered();

        foreach (Type row in new[] { typeof(ItemContainer), typeof(GridViewRow) })
        {
            Style style = sheet.GetByType(row)!;
            var triggers = style.Triggers.ToList();

            // The idle fill stays a setter; in a trigger, rows would glow on the framework's own tint at rest.
            Assert.Contains(style.Setters, setter => ReferenceEquals(setter.Property, Control.BackgroundProperty));
            Assert.Contains(
                triggers,
                trigger => (trigger.Match & VisualStateFlags.Hot) != 0
                    && trigger.Setters.Any(setter => ReferenceEquals(setter.Property, Control.BackgroundProperty)));

            // A later trigger wins, so the selection has to be declared after the hover.
            int hot = triggers.FindIndex(trigger => (trigger.Match & VisualStateFlags.Hot) != 0);
            int selected = triggers.FindIndex(trigger => (trigger.Match & VisualStateFlags.Selected) != 0);
            Assert.True(
                hot < selected,
                $"{row.Name} declares its hover after its selection, so a selected row would tint under the pointer");
        }
    }

    /// <summary>
    /// Islands has no alternating rows, and the stripe a list or a grid mixes out of the control-background
    /// seed used to collapse into the surface under it. The light theme now darkens that seed for the sake of
    /// its selection text, so the stripe would show: every control that owns the switch has it thrown off by a
    /// rule on that very control, since a setter naming a property registered below the rule's target type is
    /// refused at freeze time.
    /// </summary>
    [Fact]
    public void No_item_view_stripes_its_rows()
    {
        StyleSheet sheet = Registered();

        AssertStripingOff(sheet, typeof(ListBox), ListBox.ZebraStripingProperty);
        AssertStripingOff(sheet, typeof(GridView), GridView.ZebraStripingProperty);
        AssertStripingOff(sheet, typeof(ComboBox), ComboBox.ZebraStripingProperty);
    }

    /// <summary>
    /// An owner mismatch or an incompatible value is thrown when the first control resolves its style, which
    /// is after the window is up: the running application dies there rather than looking slightly off.
    /// Freezing runs the framework's validation over every rule and every named style.
    /// </summary>
    [Fact]
    public void The_whole_sheet_freezes()
    {
        var sheet = Registered();

        sheet.Freeze();

        Assert.True(sheet.IsFrozen);
    }

    /// <summary>
    /// The chrome MewUI wraps a popup in draws its drop shadow with a radius read from the metrics record and
    /// nothing else, so a floating surface that kept an inline list's square plate would leave its corners
    /// outside the blur meant to sit under them. A menu and a date picker are the two surfaces Islands states
    /// no radius for, and they restate the arc from that metric — a boxed constant there is a fixed number
    /// that only survives a custom <c>UseMetrics</c> by accident.
    /// </summary>
    [Fact]
    public void A_popup_islands_states_no_radius_for_takes_its_arc_from_the_theme()
    {
        StyleSheet sheet = Registered();

        AssertArcIsThemeResolved(sheet.GetByType(typeof(ContextMenu))!, "context menu");
        AssertArcIsThemeResolved(sheet.Get(BuiltInStyles.DatePickerPopup)!, "date picker drop-down");

        // A list on the page is not a surface: Islands squares it, and that must not follow the popup arc.
        SetterBase plate = Assert.Single(
            sheet.GetByType(typeof(ScrollableItemsBase))!.Setters,
            candidate => ReferenceEquals(candidate.Property, Control.CornerRadiusProperty));
        Assert.Equal(0.0, plate.Value);
    }

    /// <summary>
    /// The two surfaces that deliberately sit a couple of pixels off the radius their shadow is blurred with,
    /// on a blurred edge: a tool tip, whose own key states 6, and a drop-down's list, whose corner is also the
    /// arc of the pills inside it. Both are recorded in <c>docs/color-map.md</c>.
    /// </summary>
    [Fact]
    public void A_popup_islands_states_a_radius_for_takes_its_own()
    {
        StyleSheet sheet = Registered();

        SetterBase tip = Assert.Single(
            sheet.GetByType(typeof(ToolTip))!.Setters,
            candidate => ReferenceEquals(candidate.Property, Control.CornerRadiusProperty));
        Assert.Equal(IslandsMetrics.ToolTipArc, Assert.IsType<double>(tip.Value));

        // MewUI derives a row plate's arc from the arc of the list it sits in, less that list's border inset
        // (ItemsControl.UpdateItemCornerRadius), so the list's corner is what decides the pill's.
        Assert.Contains(
            sheet.Get(BuiltInStyles.ComboBoxPopup)!.Setters,
            candidate => ReferenceEquals(candidate.Property, Control.CornerRadiusProperty));
        Assert.Equal(IslandsMetrics.SelectionArc, IslandsMetrics.PopupArc - IslandsMetrics.BorderThickness);
    }

    /// <summary>
    /// A drop-down builds its list itself and reaches it by name, so no type rule ever sees it: this one rule
    /// is the whole of a drop-down's popup geometry, and each of its three numbers is the only handle on
    /// something. The plate's inset can only come from the list's padding — a row's background is inflated
    /// back over the item padding and reaches the edge of the scrolled content whatever that padding is — the
    /// label's inset from the row's own padding, and the row height from the box, which copies its item height
    /// into the list as a local value the moment it opens.
    /// </summary>
    [Fact]
    public void The_drop_down_list_carries_islands_row_metrics()
    {
        StyleSheet sheet = Registered();

        AssertSetterValue(sheet.Get(BuiltInStyles.ComboBoxPopup)!, Control.PaddingProperty, IslandsMetrics.PopupContentPadding);
        AssertSetterValue(sheet.Get(BuiltInStyles.ComboBoxPopup)!, ListBox.ItemPaddingProperty, IslandsMetrics.RowPadding);
        AssertSetterValue(sheet.Get(BuiltInStyles.ComboBoxPopup)!, ListBox.ItemHeightProperty, IslandsMetrics.ListRowHeight);
        AssertSetterValue(sheet.GetByType(typeof(ComboBox))!, ComboBox.ItemHeightProperty, IslandsMetrics.ListRowHeight);
    }

    private static void AssertSetterValue<T>(Style style, MewProperty<T> property, T expected)
    {
        SetterBase setter = Assert.Single(style.Setters, candidate => ReferenceEquals(candidate.Property, property));

        Assert.Equal(expected, Assert.IsType<T>(setter.Value));
    }

    private static void AssertArcIsThemeResolved(Style style, string what)
    {
        SetterBase setter = Assert.Single(
            style.Setters,
            candidate => ReferenceEquals(candidate.Property, Control.CornerRadiusProperty));

        Assert.True(
            setter.Value is null,
            $"the {what}'s arc is the constant {setter.Value}, so it no longer tracks the radius its shadow is drawn with");
    }

    private static void AssertStripingOff(StyleSheet sheet, Type owner, MewProperty property)
    {
        SetterBase setter = Assert.Single(
            sheet.GetByType(owner)!.Setters,
            candidate => ReferenceEquals(candidate.Property, property));

        Assert.Equal(false, setter.Value);
    }

    [Fact]
    public void The_framework_keys_the_theme_shadows_are_spelled_right()
    {
        StyleSheet sheet = Registered();

        Assert.Equal("flat-button", BuiltInStyles.FlatButton);
        Assert.Equal("accent-button", BuiltInStyles.AccentButton);
        Assert.Equal("combobox-popup", BuiltInStyles.ComboBoxPopup);
        Assert.Equal("datepicker-popup", BuiltInStyles.DatePickerPopup);
        Assert.Equal("toolbar-button", BuiltInStyles.ToolBarButton);
        Assert.Equal("toolbar-toggle-button", BuiltInStyles.ToolBarToggleButton);
        Assert.Equal("toolbar-label", BuiltInStyles.ToolBarLabel);

        Assert.Equal(typeof(Button), sheet.Get(BuiltInStyles.FlatButton)!.TargetType);
        Assert.Equal(typeof(Button), sheet.Get(BuiltInStyles.AccentButton)!.TargetType);
        Assert.Equal(typeof(Button), sheet.Get(BuiltInStyles.ToolBarButton)!.TargetType);
        Assert.Equal(typeof(ToggleButton), sheet.Get(BuiltInStyles.ToolBarToggleButton)!.TargetType);
        Assert.Equal(typeof(ListBox), sheet.Get(BuiltInStyles.ComboBoxPopup)!.TargetType);
        Assert.Equal(typeof(Calendar), sheet.Get(BuiltInStyles.DatePickerPopup)!.TargetType);
        Assert.Equal(typeof(Label), sheet.Get(BuiltInStyles.ToolBarLabel)!.TargetType);
        Assert.Equal(typeof(ItemContainer), sheet.Get("items-control-row")!.TargetType);
        Assert.Equal(typeof(Button), sheet.Get(IslandsStyleNames.CompactButton)!.TargetType);
    }

    /// <summary>
    /// The framework's accent split button already matches Islands's primary button, and shadowing it
    /// would mean restating a template-injected face style for no gain.
    /// </summary>
    [Fact]
    public void The_accent_split_button_is_deliberately_not_shadowed()
    {
        StyleSheet sheet = Registered();

        Assert.Equal("accent-split-button", BuiltInStyles.AccentSplitButton);
        Assert.Null(sheet.Get("accent-split-button"));
    }

    [Fact]
    public void The_surfaces_an_application_asks_for_by_name_are_registered()
    {
        StyleSheet sheet = Registered();

        foreach (string name in new[]
                 {
                     "islands-panel", "islands-card", "islands-toolbar", "islands-status-bar",
                     "islands-dialog-footer",
                 })
        {
            Style style = sheet.Get(name)!;

            // Border is the framework's generic rectangle; these are opt-in by name so that a hairline or
            // a clip box does not get recolored with them.
            Assert.Equal(typeof(Border), style.TargetType);
        }
    }

    /// <summary>
    /// A tab header's plate lives in a style the drawer names onto it, never in the <see cref="ContentControl"/>
    /// rule: that rule is what a scroll viewer, a user control and a headered content control resolve to, and a
    /// hover fill on it repaints the whole page as the pointer crosses between its background and its children.
    /// </summary>
    [Fact]
    public void The_tab_plate_is_a_named_style_and_the_content_rule_carries_no_state()
    {
        StyleSheet sheet = Registered();

        Style content = sheet.GetByType(typeof(ContentControl))!;
        Assert.Empty(content.Triggers);
        Assert.Contains(content.Setters, setter =>
            ReferenceEquals(setter.Property, Control.BackgroundProperty));

        Style header = sheet.Get("islands-tab-header")!;
        Assert.Contains(header.Triggers, trigger => trigger.Match == VisualStateFlags.Hot);
        Assert.Contains(header.Triggers, trigger => trigger.Match == VisualStateFlags.Selected);
        Assert.Contains(header.Triggers, trigger =>
            trigger.Match == (VisualStateFlags.Selected | VisualStateFlags.Hot));

        // What the drawer used to write during its own arrange pass now belongs to the header's style, so the
        // strip still gets IntelliJ's insets and height without a layout pass mutating layout.
        Assert.Contains(header.Setters, setter => ReferenceEquals(setter.Property, Control.PaddingProperty));
        Assert.Contains(header.Setters, setter => ReferenceEquals(setter.Property, FrameworkElement.MinHeightProperty));

        Style row = sheet.Get("items-control-row")!;
        Assert.Equal(sheet.GetByType(typeof(ItemContainer))!.Setters.Count, row.Setters.Count);
        Assert.Equal(sheet.GetByType(typeof(ItemContainer))!.Triggers.Count, row.Triggers.Count);
    }

    [Fact]
    public void Metrics_carry_the_new_ui_density()
    {
        ThemeMetrics metrics = IslandsMetrics.Default;

        // Islands/Button/minimumSize = 72,28 and Component/arc = 8, on the New UI's 13 DIP UI font.
        Assert.Equal(28, metrics.BaseControlHeight);
        Assert.Equal(13, metrics.FontSize);
        Assert.Equal(12, metrics.FontSizeSmall);
        Assert.Equal(15, metrics.FontSizeMedium);
        Assert.Equal(1, metrics.ControlBorderThickness);
        Assert.Equal(8, IslandsMetrics.ComponentArc);
        Assert.Equal(IslandsMetrics.ComponentArc, metrics.ControlCornerRadius);
        Assert.Equal(8, IslandsMetrics.ButtonArc);
        Assert.Equal(6, metrics.ScrollBarThickness);

        // MewUI derives a row from the control height, which lands on 26 where Islands states 24 for
        // List.rowHeight — the framework ties both to the one number and Islands does not. That derivation is
        // what a control this theme does not cover falls back to; every rule that owns a row control states
        // Islands's own number, and the grid, which Islands states no row height for, keeps the derived 28.
        Assert.Equal(26, Math.Max(18, metrics.BaseControlHeight - 2));
        Assert.Equal(28, Math.Max(metrics.BaseControlHeight, 24));
        Assert.Equal(24, IslandsMetrics.ListRowHeight);
    }

    /// <summary>
    /// Static initializers run in declaration order, so a metric built from an inset declared below it
    /// captures an unset <see cref="Thickness"/> instead. Every row the framework sizes from
    /// <c>ItemPadding</c> — a grid's cells, a tree's rows, a menu's items — came out flush with its own edge.
    /// </summary>
    [Fact]
    public void No_metric_captures_an_inset_that_has_not_been_built_yet()
    {
        ThemeMetrics metrics = IslandsMetrics.Default;

        Assert.Equal(IslandsMetrics.RowPadding, metrics.ItemPadding);
        Assert.NotEqual(default, metrics.ItemPadding);
        Assert.Equal(new Thickness(8), metrics.ContainerPadding);
    }

    /// <summary>
    /// MewUI paints a check box and a radio button itself, from its own border and fill, and strokes the mark
    /// in <c>Palette.Accent</c>. Islands's box corner, mark stroke and radio ring therefore only follow the
    /// theme when the theme's own face is in place: the rule has to carry the template that installs it.
    /// </summary>
    [Fact]
    public void The_check_faces_come_from_the_theme_not_from_the_framework()
    {
        StyleSheet sheet = Registered();

        AssertCarriesTemplate(sheet.GetByType(typeof(CheckBox))!, "check box");
        AssertCarriesTemplate(sheet.GetByType(typeof(RadioButton))!, "radio button");
    }

    /// <summary>
    /// Islands lights the mark when a box is checked and leaves the label at the window text color. A checked
    /// trigger that moves the foreground paints the label in the mark's color instead, which on the light
    /// variant is the white the box is filled with — an invisible label.
    /// </summary>
    [Fact]
    public void Checking_a_box_moves_its_plate_and_never_its_label()
    {
        StyleSheet sheet = Registered();

        foreach (Type control in new[] { typeof(CheckBox), typeof(RadioButton) })
        {
            Style style = sheet.GetByType(control)!;
            StateTrigger checkedState = Assert.Single(style.Triggers, trigger => trigger.Match == VisualStateFlags.Checked);

            Assert.Contains(checkedState.Setters, setter => ReferenceEquals(setter.Property, Control.BackgroundProperty));
            Assert.DoesNotContain(checkedState.Setters, setter => ReferenceEquals(setter.Property, TextElement.ForegroundProperty));
        }
    }

    /// <summary>
    /// Islands's scroll thumb is a mid grey (#818594 light, #6f737a dark) that the framework's own derivation
    /// — a pale overlay mixed out of the window background — cannot reach, so the theme paints the thumb
    /// from the palette instead of leaving the derived one on the bar.
    /// </summary>
    [Fact]
    public void The_scroll_thumb_is_painted_from_the_palette_rather_than_derived()
    {
        Style style = Registered().GetByType(typeof(ScrollBar))!;

        SetterBase fill = Assert.Single(
            style.Setters,
            candidate => ReferenceEquals(candidate.Property, Control.BackgroundProperty));
        Assert.True(fill.Value is null, "the scroll bar's fill is a constant, so the derived thumb is what paints");

        foreach (VisualStateFlags state in new[] { VisualStateFlags.Hot, VisualStateFlags.Pressed })
        {
            Assert.Contains(
                style.Triggers,
                trigger => (trigger.Match & state) != 0
                    && trigger.Setters.Any(setter => ReferenceEquals(setter.Property, Control.BackgroundProperty)));
        }
    }

    /// <summary>
    /// Islands states two densities as tokens: the numbers the themes use, and a <c>.compact</c> set of the
    /// same names. The compact control height is what MewUI's row derivation turns back into Islands' own
    /// <c>List.rowHeight.compact</c> of 22 — the two agree, which is what makes the switch worth having.
    /// </summary>
    [Fact]
    public void The_compact_density_carries_islands_compact_keys()
    {
        ThemeMetrics metrics = IslandsMetrics.For(IslandsDensity.Compact);

        // Component/arc.compact, Button/minimumSize.compact, Island/arc.compact.
        Assert.Equal(6, metrics.ControlCornerRadius);
        Assert.Equal(6, IslandsDensity.Compact.ButtonArc);
        Assert.Equal(24, metrics.BaseControlHeight);
        Assert.Equal(16, IslandsDensity.Compact.IslandArc);
        Assert.Equal(12, metrics.FontSize);

        // List|Tree/rowHeight.compact = 22.
        Assert.Equal(22, Math.Max(18, metrics.BaseControlHeight - 2));
        Assert.Equal(22, IslandsDensity.Compact.ListRowHeight);

        // And the regular density is untouched by any of it.
        ThemeMetrics regular = IslandsMetrics.For(IslandsDensity.Regular);
        Assert.Equal(8, regular.ControlCornerRadius);
        Assert.Equal(28, regular.BaseControlHeight);
        Assert.Equal(26, Math.Max(18, regular.BaseControlHeight - 2));
    }

    /// <summary>
    /// A list's group menu sits in a header band, so it takes the compact metric. A drop-down is not a
    /// button and cannot resolve the button's named style, hence a style of its own with the same insets.
    /// </summary>
    [Fact]
    public void The_compact_drop_down_carries_the_compact_button_metric()
    {
        StyleSheet sheet = Registered();

        Style dropDown = sheet.Get("islands-compact-dropdown")!;
        Style button = sheet.Get("islands-compact-button")!;

        Assert.Equal(typeof(DropDownButton), dropDown.TargetType);

        // Chrome contributes an inset of its own, and the compact metrics are written later; the last setter
        // is the one that takes effect.
        SetterBase padding = dropDown.Setters.Last(
            candidate => ReferenceEquals(candidate.Property, Control.PaddingProperty));
        SetterBase buttonPadding = button.Setters.Last(
            candidate => ReferenceEquals(candidate.Property, Control.PaddingProperty));

        Assert.Equal(buttonPadding.Value, padding.Value);
    }

    /// <summary>
    /// A tree's row plates follow the control's own corner radius (the framework derives both from it),
    /// so squaring the tree with the other item views left its selection plate with square corners.
    /// </summary>
    [Fact]
    public void The_tree_row_plate_is_rounded()
    {
        StyleSheet sheet = Registered();
        Style style = sheet.GetByType(typeof(TreeView))!;

        SetterBase radius = style.Setters.Last(
            setter => ReferenceEquals(setter.Property, Control.CornerRadiusProperty));

        Assert.Equal(IslandsMetrics.SelectionArc, radius.Value);
    }

    /// <summary>
    /// A panel header's drop-down is a tool-strip button, not a field: no border, transparent at rest and
    /// no minimum width, or a panel's "+" and "⋯" come out as full-size combo boxes.
    /// </summary>
    [Fact]
    public void A_panel_header_drop_down_is_flat_and_has_no_minimum_width()
    {
        StyleSheet sheet = Registered();
        Style style = sheet.Get("islands-toolbar-dropdown")!;

        Assert.Equal(typeof(DropDownButton), style.TargetType);
        Assert.Contains(style.Setters, setter => ReferenceEquals(setter.Property, Control.BorderThicknessProperty));
        Assert.Contains(style.Setters, setter => ReferenceEquals(setter.Property, FrameworkElement.MinWidthProperty));
    }

    /// <summary>
    /// The tree and the context menus draw the same selection plate height, so one selected row and one
    /// selected menu item are indistinguishable in height wherever they appear.
    /// </summary>
    [Fact]
    public void A_tree_and_a_menu_share_one_row_height()
    {
        StyleSheet sheet = Registered();
        Style tree = sheet.GetByType(typeof(TreeView))!;
        Style menu = sheet.GetByType(typeof(ContextMenu))!;

        Assert.Equal(
            IslandsMetrics.NavigationRowHeight,
            tree.Setters.Last(setter => ReferenceEquals(setter.Property, TreeView.ItemHeightProperty)).Value);
        Assert.Equal(
            IslandsMetrics.NavigationRowHeight,
            menu.Setters.Last(setter => ReferenceEquals(setter.Property, ContextMenu.ItemHeightProperty)).Value);
        Assert.Equal(30, IslandsMetrics.NavigationRowHeight);
    }

    /// <summary>
    /// A menu row insets its plate from the popup edge and sets its label one step above the base type,
    /// the way IntelliJ's context menus read; the values live here rather than at each menu's call site.
    /// </summary>
    [Fact]
    public void A_menu_row_carries_its_own_inset_and_one_step_larger_type()
    {
        StyleSheet sheet = Registered();
        Style menu = sheet.GetByType(typeof(ContextMenu))!;

        Assert.Equal(
            new Thickness(4),
            menu.Setters.Last(setter => ReferenceEquals(setter.Property, Control.PaddingProperty)).Value);
        Assert.Contains(menu.Setters, setter => ReferenceEquals(setter.Property, TextElement.FontSizeProperty));
    }

    private static void AssertCarriesTemplate(Style style, string what)
    {
        SetterBase setter = Assert.Single(
            style.Setters,
            candidate => ReferenceEquals(candidate.Property, Control.TemplateProperty));

        Assert.True(
            setter.Value is ControlTemplate,
            $"the {what}'s face is {setter.Value ?? "null"}, so the framework's own icon is what gets drawn");
    }

    /// <summary>
    /// A menu row keeps air around its icon column and label. The framework reserves the icon column
    /// itself when items carry icons, which every menu here does, so the padding is a plain margin
    /// rather than the icon column folded in; the width floor stays Islands's own.
    /// </summary>
    [Fact]
    public void A_menu_row_carries_expuis_item_insets_and_its_minimum_width()
    {
        Style style = Registered().GetByType(typeof(ContextMenu))!;

        SetterBase padding = Assert.Single(
            style.Setters,
            candidate => ReferenceEquals(candidate.Property, ContextMenu.ItemPaddingProperty));
        Assert.Equal(new Thickness(14, 5, 20, 5), Assert.IsType<Thickness>(padding.Value));

        SetterBase minWidth = Assert.Single(
            style.Setters,
            candidate => ReferenceEquals(candidate.Property, FrameworkElement.MinWidthProperty));
        Assert.Equal(IslandsMetrics.MenuMinWidth, Assert.IsType<double>(minWidth.Value));
    }
}

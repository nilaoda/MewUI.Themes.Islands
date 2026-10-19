using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// The single source of truth for every Islands color used by this theme.
/// </summary>
/// <remarks>
/// <para>
/// Values are read from JetBrains' own theme definitions, checked in at <c>docs/islands</c>
/// (<c>ManyIslandsLight</c> / <c>ManyIslandsDark</c>). Islands is a token design: the gray, blue, red, green
/// and yellow ramps are the same in both variants and only the semantic tokens — <c>layer-0/1/2-bg</c>,
/// <c>text-*</c>, <c>core-bg-transparent-*</c> — resolve per variant. Each field names the token it comes
/// from; multiple roles can share the same resolved color.
/// </para>
/// <para>
/// Two things follow from that design. First, a surface is picked by layer rather than by component: the
/// application's background is <c>layer-0</c>, panels and dialogs are <c>layer-1</c>, and content — the
/// field you type into, the list you read — is <c>layer-2</c>. Second, hover and press are <em>translucent
/// overlays</em> over whatever surface they land on, not colors of their own, which is why those fields
/// carry an alpha.
/// </para>
/// </remarks>
internal static class IslandsColors
{
    /// <summary>Islands light: <c>ManyIslandsLight.theme.json</c>'s <c>ui</c> map, resolved against its tokens.</summary>
    internal static readonly IslandsVariantColors Light = new()
    {
        // */background = layer-1-bg: a panel sits one layer above the application background.
        PanelBackground = Color.FromHex("#f7f8f9"),
        // ComboBox/background = dialog-bg-inline: a field is the brightest layer, layer-2.
        ComponentBackground = Color.FromHex("#ffffff"),
        // Menu/background = layer-2-bg: a menu is content, so it takes the content layer.
        MenuBackground = Color.FromHex("#ffffff"),
        ToolBarBackground = Color.FromHex("#f7f8f9"),
        // ScrollBar/trackColor is fully transparent: Islands draws a thumb over nothing.
        ScrollTrackBackground = Color.FromHex("#000000").WithAlpha(0),
        // ToolTip is a dark chip in both variants.
        ToolTipBackground = Color.FromHex("#191a1c"),
        ToolTipForeground = Color.FromHex("#ffffff"),

        Text = Color.FromHex("#000000"),
        // text-secondary is the same gray in both variants; only the text around it moves.
        SecondaryText = Color.FromHex("#73767c"),
        DisabledText = Color.FromHex("#9fa2a8"),
        MenuAcceleratorText = Color.FromHex("#73767c"),
        MenuAcceleratorSelectionText = Color.FromHex("#000000"),
        Icon = Color.FromHex("#73767c"),
        Arrow = Color.FromHex("#73767c"),
        ArrowDisabled = Color.FromHex("#9fa2a8"),

        // Button/startBorderColor = gray-130.
        Border = Color.FromHex("#d1d3d9"),
        BorderDisabled = Color.FromHex("#dddfe4"),
        // */focusColor = accent-brand-border: Islands lights a hovered or focused edge with the accent.
        BorderFocused = Color.FromHex("#3871e1"),
        FocusRing = Color.FromHex("#3871e1"),
        // Popup/borderColor = gray-130 light, layer-1-border dark.
        MenuBorder = Color.FromHex("#d1d3d9"),
        // The system's own close-button red; not an Islands key, see IslandsVariantColors.
        CaptionCloseHover = Color.FromHex("#C42B1C"),
        // ComboPopup.border.os.windows = 1,1,1,1,#B5B7BD: a literal hex, the only one the light variant states.
        ComboPopupBorder = Color.FromHex("#B5B7BD"),
        // */selectionBackground = blue-140.
        MenuCheckBackground = Color.FromHex("#d0dffe"),
        Separator = Color.FromHex("#dddfe4"),

        Accent = Color.FromHex("#3871e1"),
        AccentLight = Color.FromHex("#538af9"),
        AccentText = Color.FromHex("#ffffff"),
        Link = Color.FromHex("#2f5eb9"),
        // The seed text is also what a realized row label shows, so it stays a legible dark grey rather than
        // the value that would land the border derivation exactly on Border above: it is nine channels off
        // instead, on a 1 DIP hairline.
        SeedWindowText = Color.FromHex("#3c3c3c"),

        Selection = Color.FromHex("#d0dffe"),
        SelectionText = Color.FromHex("#000000"),
        InactiveSelection = Color.FromHex("#e9eaee"),
        InactiveSelectionText = Color.FromHex("#000000"),
        // */hoverBackground = core-bg-transparent-hovered: 7% black over the surface.
        HoverBackground = Color.FromHex("#000000").WithAlpha(0x12),

        ButtonBackground = Color.FromHex("#ffffff"),
        ButtonForeground = Color.FromHex("#000000"),
        ButtonBorder = Color.FromHex("#d1d3d9"),
        ButtonHover = Color.FromHex("#000000").WithAlpha(0x12),
        ButtonPressed = Color.FromHex("#000000").WithAlpha(0x20),
        ButtonSelected = Color.FromHex("#d0dffe"),
        ButtonDisabledBackground = Color.FromHex("#f7f8f9"),
        ButtonDisabledForeground = Color.FromHex("#9fa2a8"),
        ToggleDisabledSelectedBackground = Color.FromHex("#dddfe4"),
        ToolbarHover = Color.FromHex("#000000").WithAlpha(0x12),
        ToolbarPressed = Color.FromHex("#000000").WithAlpha(0x20),
        ToolbarSelected = Color.FromHex("#3871e1"),

        // Button/default/*: the accent plate with text-over-accent on it.
        PrimaryButtonBackground = Color.FromHex("#3871e1"),
        PrimaryButtonForeground = Color.FromHex("#ffffff"),
        PrimaryButtonBorder = Color.FromHex("#3871e1"),
        PrimaryButtonHover = Color.FromHex("#538af9"),
        PrimaryButtonPressed = Color.FromHex("#2f5eb9"),
        PrimaryButtonFocusedBorder = Color.FromHex("#f7f8f9"),

        // Table/gridColor = layer-1-border-inline light, layer-0-border dark.
        GridLine = Color.FromHex("#e9eaee"),
        GridHeaderSeparator = Color.FromHex("#e9eaee"),
        GridHeaderBackground = Color.FromHex("#f7f8f9"),

        // EditorTabs/underlinedTabBackground = tab-selected-bg-active = blue-150.
        TabSelectedBackground = Color.FromHex("#e3ebfe"),
        // EditorTabs/underlinedBorderColor = tab-selected-border-active = blue-120.
        TabSelectedBorder = Color.FromHex("#a7c5ff"),
        // EditorTabs/inactiveUnderlinedTabBackground = tab-selected-bg-inactive = gray-150.
        TabSelectedInactiveBackground = Color.FromHex("#e9eaee"),
        // EditorTabs/inactiveUnderlinedTabBorderColor = tab-selected-border-inactive = gray-130.
        TabSelectedInactiveBorder = Color.FromHex("#d1d3d9"),
        // EditorTabs/underTabsBorderColor = layer-2-border.
        TabContentSeparator = Color.FromHex("#e9eaee"),
        TabHoverBackground = Color.FromHex("#000000").WithAlpha(0x12),
        TabFocusBackground = Color.FromHex("#e3ebfe"),

        // ScrollBar/thumbColor: a translucent grey capsule over the surface.
        ScrollTrack = Color.FromHex("#000000").WithAlpha(0),
        ScrollThumb = Color.FromHex("#808080").WithAlpha(0x59),
        ScrollThumbHover = Color.FromHex("#808080").WithAlpha(0x8c),
        ScrollThumbPressed = Color.FromHex("#808080").WithAlpha(0xbf),

        ProgressTrack = Color.FromHex("#dddfe4"),
        ProgressFill = Color.FromHex("#3871e1"),
        ProgressText = Color.FromHex("#000000"),
        ProgressTextOnFill = Color.FromHex("#ffffff"),

        SliderFill = Color.FromHex("#3871e1"),
        SliderFillHover = Color.FromHex("#538af9"),
        SliderTrack = Color.FromHex("#c9ccd6"),

        // The check mark is an icon, not a color: a 16 DIP box with a grey edge which fills with the accent
        // and a white mark when selected, and rings itself in the accent when focused.
        CheckBorder = Color.FromHex("#c3c5cb"),
        CheckBorderActive = Color.FromHex("#3871e1"),
        CheckDisabledBorder = Color.FromHex("#dddfe4"),
        CheckSelectedBorder = Color.FromHex("#3871e1"),
        CheckFilledBackground = Color.FromHex("#3871e1"),
        CheckFilledForeground = Color.FromHex("#ffffff"),
        CheckDisabledMark = Color.FromHex("#9fa2a8"),

        // accent-error-bg / accent-warning-bg / accent-success-bg.
        Error = Color.FromHex("#e4656e"),
        ErrorBorder = Color.FromHex("#ffc4c5"),
        Warning = Color.FromHex("#c28013"),
        WarningBorder = Color.FromHex("#f4cd9a"),
        Success = Color.FromHex("#4e9d6c"),
        SuccessBorder = Color.FromHex("#bbdbc2"),
    };

    /// <summary>Islands dark: <c>ManyIslandsDark.theme.json</c>'s <c>ui</c> map, resolved against its tokens.</summary>
    internal static readonly IslandsVariantColors Dark = new()
    {
        // */background = layer-0-bg: the dark application background is the darkest layer.
        PanelBackground = Color.FromHex("#191a1c"),
        // ComboBox/background = layer-1-bg.
        ComponentBackground = Color.FromHex("#26282c"),
        MenuBackground = Color.FromHex("#26282c"),
        ToolBarBackground = Color.FromHex("#191a1c"),
        ScrollTrackBackground = Color.FromHex("#000000").WithAlpha(0),
        ToolTipBackground = Color.FromHex("#26282c"),
        ToolTipForeground = Color.FromHex("#d1d3d9"),

        Text = Color.FromHex("#d1d3d9"),
        SecondaryText = Color.FromHex("#73767c"),
        DisabledText = Color.FromHex("#4c4f56"),
        MenuAcceleratorText = Color.FromHex("#73767c"),
        MenuAcceleratorSelectionText = Color.FromHex("#d1d3d9"),
        Icon = Color.FromHex("#9fa2a8"),
        Arrow = Color.FromHex("#9fa2a8"),
        ArrowDisabled = Color.FromHex("#4c4f56"),

        // Button/startBorderColor = gray-50.
        Border = Color.FromHex("#40434a"),
        BorderDisabled = Color.FromHex("#33353b"),
        BorderFocused = Color.FromHex("#3871e1"),
        FocusRing = Color.FromHex("#3871e1"),
        MenuBorder = Color.FromHex("#33353b"),
        CaptionCloseHover = Color.FromHex("#C42B1C"),
        // ComboPopup.border = 1,1,1,1,#4C4F56: a literal hex rather than a token.
        ComboPopupBorder = Color.FromHex("#4C4F56"),
        MenuCheckBackground = Color.FromHex("#2a4371"),
        Separator = Color.FromHex("#33353b"),

        Accent = Color.FromHex("#3871e1"),
        AccentLight = Color.FromHex("#538af9"),
        AccentText = Color.FromHex("#ffffff"),
        Link = Color.FromHex("#71a1fe"),
        // As in light: near-white for the labels, six channels off the border derivation.
        SeedWindowText = Color.FromHex("#dddddd"),

        Selection = Color.FromHex("#2a4371"),
        SelectionText = Color.FromHex("#d1d3d9"),
        InactiveSelection = Color.FromHex("#33353b"),
        InactiveSelectionText = Color.FromHex("#d1d3d9"),
        // core-bg-transparent-hovered: 9% white over the surface.
        HoverBackground = Color.FromHex("#ffffff").WithAlpha(0x17),

        // Button/endBackground is fully transparent dark: a button shows the layer it sits on.
        ButtonBackground = Color.FromHex("#191a1c"),
        ButtonForeground = Color.FromHex("#d1d3d9"),
        ButtonBorder = Color.FromHex("#40434a"),
        ButtonHover = Color.FromHex("#ffffff").WithAlpha(0x17),
        ButtonPressed = Color.FromHex("#ffffff").WithAlpha(0x29),
        ButtonSelected = Color.FromHex("#2a4371"),
        ButtonDisabledBackground = Color.FromHex("#191a1c"),
        ButtonDisabledForeground = Color.FromHex("#4c4f56"),
        ToggleDisabledSelectedBackground = Color.FromHex("#33353b"),
        ToolbarHover = Color.FromHex("#ffffff").WithAlpha(0x17),
        ToolbarPressed = Color.FromHex("#ffffff").WithAlpha(0x29),
        ToolbarSelected = Color.FromHex("#3871e1"),

        PrimaryButtonBackground = Color.FromHex("#3871e1"),
        PrimaryButtonForeground = Color.FromHex("#ffffff"),
        PrimaryButtonBorder = Color.FromHex("#3871e1"),
        PrimaryButtonHover = Color.FromHex("#538af9"),
        PrimaryButtonPressed = Color.FromHex("#2f5eb9"),
        PrimaryButtonFocusedBorder = Color.FromHex("#191a1c"),

        GridLine = Color.FromHex("#26282c"),
        GridHeaderSeparator = Color.FromHex("#26282c"),
        GridHeaderBackground = Color.FromHex("#191a1c"),

        // tab-selected-bg-active = blue-40; tab-selected-border-active = blue-60.
        TabSelectedBackground = Color.FromHex("#233558"),
        TabSelectedBorder = Color.FromHex("#2e4d89"),
        // tab-selected-bg-inactive = gray-30; tab-selected-border-inactive = gray-50.
        TabSelectedInactiveBackground = Color.FromHex("#26282c"),
        TabSelectedInactiveBorder = Color.FromHex("#40434a"),
        TabContentSeparator = Color.FromHex("#26282c"),
        TabHoverBackground = Color.FromHex("#ffffff").WithAlpha(0x17),
        TabFocusBackground = Color.FromHex("#233558"),

        ScrollTrack = Color.FromHex("#000000").WithAlpha(0),
        ScrollThumb = Color.FromHex("#808080").WithAlpha(0x59),
        ScrollThumbHover = Color.FromHex("#808080").WithAlpha(0x8c),
        ScrollThumbPressed = Color.FromHex("#808080").WithAlpha(0xbf),

        ProgressTrack = Color.FromHex("#40434a"),
        ProgressFill = Color.FromHex("#3871e1"),
        ProgressText = Color.FromHex("#d1d3d9"),
        ProgressTextOnFill = Color.FromHex("#ffffff"),

        SliderFill = Color.FromHex("#3871e1"),
        SliderFillHover = Color.FromHex("#538af9"),
        SliderTrack = Color.FromHex("#5a5d63"),

        // The icon strokes its box in gray-70 on the dark ramp.
        CheckBorder = Color.FromHex("#5a5d63"),
        CheckBorderActive = Color.FromHex("#3871e1"),
        CheckDisabledBorder = Color.FromHex("#33353b"),
        CheckSelectedBorder = Color.FromHex("#3871e1"),
        CheckFilledBackground = Color.FromHex("#3871e1"),
        CheckFilledForeground = Color.FromHex("#ffffff"),
        CheckDisabledMark = Color.FromHex("#4c4f56"),

        Error = Color.FromHex("#c54e58"),
        ErrorBorder = Color.FromHex("#80383e"),
        Warning = Color.FromHex("#a56906"),
        WarningBorder = Color.FromHex("#694820"),
        Success = Color.FromHex("#338555"),
        SuccessBorder = Color.FromHex("#29583c"),
    };
}

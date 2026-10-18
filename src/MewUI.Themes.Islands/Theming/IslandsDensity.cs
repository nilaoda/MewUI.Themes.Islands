using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Theming;

/// <summary>
/// Islands ships two densities: the regular one and a <c>.compact</c> set of the same tokens.
/// </summary>
/// <remarks>
/// The values are the <c>*.compact</c> keys of the Islands themes — <c>Component/arc.compact</c>,
/// <c>Button/minimumSize.compact</c>, <c>List|Tree/rowHeight.compact</c>, <c>ComboBox/padding.compact</c>,
/// <c>Island/arc.compact</c> — with the type sizes stepped down to match. A window that has to show more
/// rows than it has room for asks for this one through
/// <see cref="IslandsOptions.Density"/>.
/// </remarks>
public sealed record IslandsDensity
{
    /// <summary><c>Component/arc</c>.</summary>
    public required double ComponentArc { get; init; }

    /// <summary><c>Button/arc</c>.</summary>
    public required double ButtonArc { get; init; }

    /// <summary><c>Button/minimumSize</c>'s height, which MewUI sizes controls and rows from.</summary>
    public required double BaseControlHeight { get; init; }

    /// <summary><c>ComboBox/padding</c>.</summary>
    public required Thickness ComponentPadding { get; init; }

    /// <summary>A button's own inset. Islands states no key for it, so it steps with the control height.</summary>
    public required Thickness ButtonPadding { get; init; }

    /// <summary><c>List.cellMargins</c>: the gap between a row's content and its edge.</summary>
    public required Thickness RowPadding { get; init; }

    /// <summary>
    /// <c>List|Tree/rowHeight</c>: what a list, tree, menu or drop-down row is tall. Islands states no table
    /// row height, so a grid keeps the framework's own derivation.
    /// </summary>
    /// <remarks>
    /// MewUI derives a row from the control height (<c>Max(18, BaseControlHeight - 2)</c>), which is Islands's
    /// own 28 and therefore lands on 26; Islands states the two numbers separately. A rule that owns a row
    /// control states this one, because it is the number the two systems disagree about.
    /// </remarks>
    public required double ListRowHeight { get; init; }

    /// <summary><c>TabbedPane/tabInsets</c>.</summary>
    public required Thickness TabPadding { get; init; }

    /// <summary><c>TabbedPane/tabHeight</c>.</summary>
    public required double TabHeight { get; init; }

    /// <summary><c>EditorTabs/underlineHeight</c>.</summary>
    public required double TabUnderlineThickness { get; init; }

    /// <summary><c>Island/arc</c>: the corner of a section panel.</summary>
    public required double IslandArc { get; init; }

    /// <summary>
    /// <c>Island/borderWidth</c>: the ring of surrounding colour an island carries.
    /// </summary>
    /// <remarks>
    /// The border <em>is</em> the gap. Islands' island is a plate with six DIP of the surface behind it
    /// painted around the outside, so two islands side by side come out twelve apart without either of them
    /// knowing about the other, and an island against a window edge keeps the same air — which is why
    /// <c>Island/borderColor</c> is the tool window's own background and not a line colour. The theme drew a
    /// one-pixel outline in the border colour instead, and two panes sharing a splitter met edge to edge.
    /// </remarks>
    public required double IslandBorderWidth { get; init; }

    /// <summary>The UI font size. Islands states none, so these step with the control height.</summary>
    public required double FontSize { get; init; }

    public required double FontSizeSmall { get; init; }

    public required double FontSizeMedium { get; init; }

    /// <summary>
    /// The family a column of identifiers is set in: codes, ids and timestamps, where the point is to
    /// compare one character against the one above it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A list, not a name. MewUI draws with the first family in the list that is installed and takes the
    /// characters it lacks from the ones after it, so one value covers every platform this theme ships on:
    /// Windows resolves <c>Cascadia Mono</c> where the Terminal has installed it and <c>Consolas</c>
    /// everywhere else, and macOS falls through both to <c>Menlo</c>, which is always present.
    /// </para>
    /// <para>
    /// Nothing here carries the interface's own font, and that is deliberate: a monospaced family has no
    /// CJK coverage, so setting a title in one would hand its Chinese to whatever the platform substitutes,
    /// which is a different font on every machine. The theme offers this for the columns that hold
    /// identifiers and leaves prose alone.
    /// </para>
    /// </remarks>
    public required string MonospaceFontFamily { get; init; }

    /// <summary>What the themes state outright: 8 DIP arcs, 28 DIP controls, 40 DIP tabs.</summary>
    public static IslandsDensity Regular { get; } = new()
    {        ComponentArc = 8,
        ButtonArc = 8,
        BaseControlHeight = 28,
        ComponentPadding = new Thickness(9, 1, 6, 1),
        ButtonPadding = new Thickness(14, 2, 14, 2),
        RowPadding = new Thickness(6, 1, 6, 1),
        ListRowHeight = 24,
        TabPadding = new Thickness(12, 0, 12, 0),
        TabHeight = 40,
        TabUnderlineThickness = 4,
        IslandArc = 20,
        IslandBorderWidth = 6,
        FontSize = 13,
        FontSizeSmall = 12,
        FontSizeMedium = 15,
        MonospaceFontFamily = "Cascadia Mono, Consolas, Menlo, Monaco, Courier New",
    };

    /// <summary>The <c>.compact</c> keys: 6 DIP arcs, 24 DIP controls, 22 DIP rows, 16 DIP corners.</summary>
    public static IslandsDensity Compact { get; } = new()
    {
        ComponentArc = 6,
        ButtonArc = 6,
        BaseControlHeight = 24,
        ComponentPadding = new Thickness(6, 1, 6, 1),
        ButtonPadding = new Thickness(12, 1, 12, 1),
        RowPadding = new Thickness(4, 0, 4, 0),
        ListRowHeight = 22,
        TabPadding = new Thickness(6, 0, 6, 0),
        TabHeight = 36,
        TabUnderlineThickness = 3,
        IslandArc = 16,
        IslandBorderWidth = 4,
        FontSize = 12,
        FontSizeSmall = 11,
        FontSizeMedium = 14,
        // The same list: a denser table still has identifiers to compare, and the families that carry them
        // are the platform's, not the density's.
        MonospaceFontFamily = "Cascadia Mono, Consolas, Menlo, Monaco, Courier New",
    };
}

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace MewUI.Themes.Islands;

/// <summary>Monochrome symbols for workspace actions and navigation.</summary>
public enum IslandsIconKind
{
    Plus,
    Pencil,
    ArrowRight,
    Trash,
    Restore,
    Copy,
    Export,
    Import,
    More,
    History,
    Settings,
    Library,
    ChevronDown,
    ChevronUp,
}

/// <summary>Theme-aware vector icons, independent of platform fonts.</summary>
public static class IslandsIcon
{
    /// <summary>Creates a 16-DIP icon, optionally using the foreground for an accent surface.</summary>
    public static FrameworkElement Create(IslandsIconKind kind, bool overAccent = false) =>
        new Glyph(kind, overAccent).Width(16).Height(16);

    private sealed class Glyph(IslandsIconKind kind, bool overAccent) : FrameworkElement
    {
        private static readonly PathGeometry Plus =
            PathGeometry.Parse("M8 3 V13 M3 8 H13");

        private static readonly PathGeometry Pencil =
            PathGeometry.Parse("M4 13 L4.6 9.4 L11 3 L13 5 L6.6 11.4 Z");

        private static readonly PathGeometry ArrowRight =
            PathGeometry.Parse("M3 8 H12 M8.5 4.5 L12 8 L8.5 11.5");

        private static readonly PathGeometry Trash =
            PathGeometry.Parse("M3.5 4.5 H12.5 M6.5 2.5 H9.5 M5 4.5 L5.8 13.5 H10.2 L11 4.5 M7 7 V11 M9 7 V11");

        private static readonly PathGeometry Restore =
            PathGeometry.Parse("M6.5 3 L3 6.5 L6.5 10 M3 6.5 H10 V13.5 H7");

        private static readonly PathGeometry Copy =
            PathGeometry.Parse("M6 2.5 H10.5 V9.5 H6 Z M3.5 5.5 H8 V14 H3.5 Z");

        private static readonly PathGeometry Export =
            PathGeometry.Parse("M8 2.5 V9 M5 6 L8 3 L11 6 M3.5 11 V14 H12.5 V11");

        private static readonly PathGeometry Import =
            PathGeometry.Parse("M8 2.5 V9.5 M5 6 L8 9.5 L11 6 M3.5 11 V14 H12.5 V11");

        private static readonly PathGeometry More = PathGeometry.Parse("M3 8 H4 M7.5 8 H8.5 M12 8 H13");
        private static readonly PathGeometry History = PathGeometry.Parse("M3 3 V7 H7 M3.3 6 A5.5 5.5 0 1 1 3 10 M8 4.5 V8 L10.5 9.5");
        private static readonly PathGeometry Settings = PathGeometry.Parse("M2 4 H7 M10 4 H14 M2 12 H5 M8 12 H14 M7 2 V6 H10 V2 Z M5 10 V14 H8 V10 Z");
        private static readonly PathGeometry Library = PathGeometry.Parse("M2 2 V11 H6 M2 6 H6 M7 3 H13 V7 H7 Z M7 10 H13 V14 H7 Z");
        private static readonly PathGeometry ChevronDown = PathGeometry.Parse("M4 6 L8 10 L12 6");
        private static readonly PathGeometry ChevronUp = PathGeometry.Parse("M4 10 L8 6 L12 10");

        protected override Size MeasureContent(Size availableSize) => new(16, 16);

        protected override void OnRender(IGraphicsContext context)
        {
            base.OnRender(context);

            var bounds = Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var shape = kind switch
            {
                IslandsIconKind.Plus => Plus,
                IslandsIconKind.Pencil => Pencil,
                IslandsIconKind.ArrowRight => ArrowRight,
                IslandsIconKind.Trash => Trash,
                IslandsIconKind.Restore => Restore,
                IslandsIconKind.Copy => Copy,
                IslandsIconKind.Export => Export,
                IslandsIconKind.Import => Import,
                IslandsIconKind.More => More,
                IslandsIconKind.History => History,
                IslandsIconKind.Settings => Settings,
                IslandsIconKind.Library => Library,
                IslandsIconKind.ChevronDown => ChevronDown,
                IslandsIconKind.ChevronUp => ChevronUp,
                _ => Export,
            };

            context.Save();
            context.Translate(bounds.X, bounds.Y);
            var scale = bounds.Width / 16.0;
            context.Scale(scale, scale);
            var colors = IslandsTheme.CurrentColors;
            context.DrawPath(shape, overAccent ? colors.AccentText : colors.Icon, 1.2);
            context.Restore();
        }
    }
}

using System.Runtime.InteropServices;

namespace MewUI.Themes.Islands;

/// <summary>
/// The Windows 11 system backdrop (Mica) behind a window, and whether this machine can draw it.
/// </summary>
/// <remarks>
/// <para>
/// Mica is a desktop-tinted material the compositor draws <em>behind</em> a window, so it is only visible where
/// the window paints nothing. Asking for it therefore has two halves: tell the compositor to draw it, and stop
/// painting an opaque background over it. The chrome does both, and only then — a window that is told to draw a
/// backdrop it cannot draw would be left transparent over the desktop.
/// </para>
/// <para>
/// The attribute to set depends on the build. Windows 11 21H2 (22000) carries Mica behind the undocumented
/// <c>DWMWA_MICA_EFFECT</c>; 22H2 (22621) replaced it with the documented
/// <c>DWMWA_SYSTEMBACKDROP_TYPE</c>, whose <c>DWMSBT_MAINWINDOW</c> value is Mica. Older systems have neither,
/// and the call is skipped rather than attempted.
/// </para>
/// </remarks>
internal static class IslandsWindowBackdrop
{
    private const int DwmwaMicaEffect = 1029;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmsbtMainWindow = 2;

    /// <summary>Whether this system can draw Mica at all: Windows 11, build 22000 or later.</summary>
    internal static bool IsSupported =>
        OperatingSystem.IsWindows() && Environment.OSVersion.Version.Build >= 22000;

    /// <summary>
    /// Asks the compositor to draw Mica behind <paramref name="handle"/>, and reports whether it was asked.
    /// </summary>
    /// <returns>
    /// True when the window is now expected to show the backdrop, which is the signal to stop painting an
    /// opaque background behind it.
    /// </returns>
    internal static bool Apply(nint handle)
    {
        if (handle == 0 || !IsSupported)
        {
            return false;
        }

        int build = Environment.OSVersion.Version.Build;

        if (build >= 22621)
        {
            int backdrop = DwmsbtMainWindow;
            return DwmSetWindowAttribute(handle, DwmwaSystemBackdropType, ref backdrop, sizeof(int)) == 0;
        }

        int enabled = 1;
        return DwmSetWindowAttribute(handle, DwmwaMicaEffect, ref enabled, sizeof(int)) == 0;
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);
}

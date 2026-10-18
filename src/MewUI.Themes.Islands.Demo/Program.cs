using System.Runtime.CompilerServices;
using Aprillz.MewUI;
using MewUI.Themes.Islands;
using MewUI.Themes.Islands.Demo;

namespace MewUI.Themes.Islands.Demo;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var options = DemoOptions.Parse(args);

        var builder = Application.Create().UseTheme(options.Theme);
        builder = RegisterPlatform(builder);
        builder = builder.UseIslands(new IslandsOptions { Density = options.Density });

        builder.BuildMainWindow(() => new DemoWindow(options)).Run();
    }

    /// <summary>
    /// One platform per method. Resolving all three in one method would make a trimmed
    /// publish ask for backends the target does not carry.
    /// </summary>
    private static ApplicationBuilder RegisterPlatform(ApplicationBuilder builder)
    {
        if (OperatingSystem.IsWindows())
        {
            return RegisterWindows(builder);
        }

        return OperatingSystem.IsMacOS() ? RegisterMacOS(builder) : RegisterX11(builder);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ApplicationBuilder RegisterWindows(ApplicationBuilder builder) =>
        builder.UseWin32().UseDirect2D();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ApplicationBuilder RegisterMacOS(ApplicationBuilder builder) =>
        builder.UseMacOS().UseMewVGMetal();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ApplicationBuilder RegisterX11(ApplicationBuilder builder) =>
        builder.UseX11().UseMewVGX11();
}

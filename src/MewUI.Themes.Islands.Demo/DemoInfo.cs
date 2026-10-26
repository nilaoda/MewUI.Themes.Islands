using System.Reflection;

namespace MewUI.Themes.Islands.Demo;

/// <summary>The gallery's build version, including its preview suffix.</summary>
internal static class DemoInfo
{
    internal static string Version { get; } =
        typeof(DemoInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0] ?? "unknown";
}

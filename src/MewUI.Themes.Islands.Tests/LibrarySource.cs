using Aprillz.MewUI;

namespace MewUI.Themes.Islands.Tests;

/// <summary>
/// Finds the library's own directory from the test assembly's output, so the source-level guards can read
/// the files they check. Returns null when the tests run from a layout that does not carry the sources.
/// </summary>
internal static class LibrarySource
{
    internal static string? Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                string candidate = Path.Combine(directory.FullName, "MewUI.Themes.Islands");
                if (File.Exists(Path.Combine(candidate, "MewUI.Themes.Islands.csproj")))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }

    internal static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root ?? throw new InvalidOperationException(), relativePath));

    internal static IEnumerable<string> Files(string extension) =>
        Root is null
            ? []
            : Directory.EnumerateFiles(Root, extension, SearchOption.AllDirectories)
                       .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                                   && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));
}

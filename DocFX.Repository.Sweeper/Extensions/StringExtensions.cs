using System.IO;

namespace DocFX.Repository.Sweeper
{
    static class StringExtensions
    {
        internal static string NormalizePathDelimitors(this string path, bool appendDirectorySeparatorSuffix = false)
            => string.IsNullOrWhiteSpace(path)
                ? null
                : Path.GetFullPath(
                    appendDirectorySeparatorSuffix && !path.EndsWith(Path.DirectorySeparatorChar)
                        ? $"{path}{Path.DirectorySeparatorChar}"
                        : path);
    }
}
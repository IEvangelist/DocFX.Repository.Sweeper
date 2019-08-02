using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        internal static async Task<T> FindJsonFileAsync<T>(this string directory, string filename)
        {
            var dir = new DirectoryInfo(directory).TraverseToFile(filename);
            var filepath = Path.Combine(dir.FullName, filename);
            if (File.Exists(filepath))
            {
                var json = await File.ReadAllTextAsync(filepath);
                return json.FromJson<T>();
            }

            return default;
        }

        public static string MergePath(this string directory, string filePath)
        {
            if (string.IsNullOrWhiteSpace(directory) ||
                string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            if (filePath.StartsWith('~') || filePath.StartsWith(".."))
            {
                return Path.Combine(directory, filePath);
            }

            // This is for explicit file paths that are double rooted:
            //   media/tshoot-connect-attribute-not-syncing/tshoot-connect-attribute-not-syncing/syncingprocess.png
            if (filePath.Contains("/"))
            {
                var combined = Path.Combine(directory, filePath);
                if (File.Exists(combined))
                {
                    return combined;
                }
            }

            var directorySegments = directory.Split(Path.DirectorySeparatorChar);
            var pathSegments = filePath.Replace('/', Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
            var segments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            directorySegments.For(segment => segments.Add(segment));
            pathSegments.For(segment => segments.Add(segment));

            return string.Join(Path.DirectorySeparatorChar, segments);
        }
    }
}
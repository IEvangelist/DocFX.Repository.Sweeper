using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper
{
    static class StringExtensions
    {
        internal static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                var hash1 = (5381 << 16) + 5381;
                var hash2 = hash1;

                for (var i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }

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
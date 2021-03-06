﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocFX.Repository.Extensions
{
    public static class StringExtensions
    {
        public static bool SoundsLike(this string source, string comparison)
            => SoundEx.Difference(source, comparison) == 4;

        public static bool SoundsKindOfLike(this string source, string comparison)
            => SoundEx.Difference(source, comparison) >= 3;

        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                var hash1 = (5381 << 16) + 5381;
                var hash2 = hash1;

                for (var i = 0; i < str.Length; i += 2)
                {
                    hash1 = (hash1 << 5) + hash1 ^ str[i];
                    if (i == str.Length - 1)
                    {
                        break;
                    }

                    hash2 = (hash2 << 5) + hash2 ^ str[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }

        public static string NormalizePathDelimitors(this string path, bool appendDirectorySeparatorSuffix = false)
            => string.IsNullOrWhiteSpace(path)
                ? null
                : Path.GetFullPath(
                    appendDirectorySeparatorSuffix && !path.EndsWith(Path.DirectorySeparatorChar.ToString())
                        ? $"{path}{Path.DirectorySeparatorChar}"
                        : path);

        public static async Task<T> FindJsonFileAsync<T>(this string directory, string filename)
        {
            var dir = new DirectoryInfo(directory).TraverseToFile(filename);
            var filepath = Path.Combine(dir.FullName, filename);
            if (File.Exists(filepath))
            {
                var json = await ReadAllTextAsync(filepath);
                return json.FromJson<T>();
            }

            return default;
        }

        static async Task<string> ReadAllTextAsync(string filePath)
        {
            string text;
            using (var sourceReader = File.OpenText(filePath))
            {
                text = await sourceReader.ReadToEndAsync();
            }

            return text;
        }

        public static string MergePath(this string directory, string filePath)
        {
            if (string.IsNullOrWhiteSpace(directory) ||
                string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            if (Path.IsPathRooted(filePath))
            {
                return filePath;
            }

            if (filePath.StartsWith("~") || filePath.StartsWith(".."))
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

            return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
        }

        public static bool ContainsAny(this string value, IEnumerable<string> values) =>
            !string.IsNullOrWhiteSpace(value)
                ? values?.Any(v => !string.IsNullOrWhiteSpace(v) && value.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0) ?? false
                : false;
    }
}
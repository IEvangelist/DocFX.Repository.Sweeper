using DocFX.Repository.Sweeper.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace DocFX.Repository.Sweeper.Extensions
{
    static class FileInfoExtensions
    {
        static readonly HashSet<string> ValidImageFileExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".png", ".jpg", ".jpeg", ".gif", ".svg"
            };

        static readonly HashSet<string> ValidYamlFileExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".yml", ".yaml"
            };

        static readonly HashSet<string> ValidMarkdownFileExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".md", ".markdown", ".mdown", ".mkd", ".mdwn", ".mdtxt", "mdtext", ".rmd"
            };

        static readonly HashSet<string> ValidJsonFileExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".json"
            };

        internal static bool IsImageFile(this FileInfo file) =>
            file.IsMatchingFileType(ValidImageFileExtensions);

        internal static bool IsJsonFile(this FileInfo file) =>
            file.IsMatchingFileType(ValidJsonFileExtensions);

        internal static bool IsMarkdownFile(this FileInfo file) =>
            file.IsMatchingFileType(ValidMarkdownFileExtensions);

        internal static bool IsYamlFile(this FileInfo file) =>
            file.IsMatchingFileType(ValidYamlFileExtensions);

        private static bool IsMatchingFileType(this FileInfo file, ICollection<string> extensions) =>
            extensions?.Contains(file?.Extension) ?? false;

        internal static FileType GetFileType(this FileInfo file)
        {
            if (file.IsMarkdownFile()) return FileType.Markdown;
            if (file.IsImageFile()) return FileType.Image;
            if (file.IsYamlFile()) return FileType.Yaml;
            if (file.IsJsonFile()) return FileType.Json;

            return FileType.NotRelevant;
        }
    }
}
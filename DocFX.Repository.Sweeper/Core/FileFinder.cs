﻿using DocFX.Repository.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper.Core
{
    class FileFinder
    {
        static readonly RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        static readonly Regex FileExtensionRegex = new Regex("(?'ext'\\.\\w+$)", Options);
        static readonly Regex FileExtensionInUrlRegex = new Regex(@"(?'ext'\.\w{3,4})$|\?", Options);
        static readonly char[] InvalidPathCharacters = Path.GetInvalidPathChars();
        static readonly ISet<string> _blackListedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".com", ".net", ".aspx", ".org", ".blog"
        };

        internal static async ValueTask<(bool, string)> TryFindFileAsync(Options options, string directory, string filePath)
        {
            if (string.IsNullOrWhiteSpace(directory) ||
                string.IsNullOrWhiteSpace(filePath))
            {
                return (false, null);
            }

            try
            {
                filePath = await EnsureValidFilePathAsync(options, filePath);
                if (string.IsNullOrWhiteSpace(filePath) ||
                    filePath.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                    filePath.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
                    filePath.IndexOfAny(InvalidPathCharacters) != -1)
                {
                    return (false, null);
                }

                if (filePath.StartsWith("~/"))
                {
                    var path = options.DocFxJsonDirectory.FullName.MergePath(filePath.Substring(2)).NormalizePathDelimitors();
                    if (FileExtensionRegex.IsMatch(path))
                    {
                        return (File.Exists(path), path);
                    }
                }
                else
                {
                    var path = directory.MergePath(filePath).NormalizePathDelimitors();
                    if (FileExtensionRegex.IsMatch(path))
                    {
                        return (File.Exists(path), path);
                    }
                }

                var files = Directory.GetFiles(directory, $"{filePath}.*");
                if (files.Length > 0)
                {
                    return (true, files[0].NormalizePathDelimitors());
                }
            }
            catch
            {
            }

            return (false, null);
        }

        static async ValueTask<string> EnsureValidFilePathAsync(Options options, string filePath)
        {
            var config = await options.GetConfigAsync();
            if (filePath.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                filePath.StartsWith("https:", StringComparison.OrdinalIgnoreCase) &&
                FileExtensionInUrlRegex.IsMatch(filePath))
            {
                if (_blackListedExtensions.Contains(Path.GetExtension(filePath)))
                {
                    return null;
                }

                var uri = new Uri(options.HostUri, $"{config.Build.Dest}/");
                if (filePath.StartsWith(uri.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    filePath = filePath.Replace(uri.ToString(), "");
                }
                else if (filePath.StartsWith($"{options.HostUri}/", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = filePath.Replace($"{options.HostUri}/", "");
                }

                if (filePath.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                    filePath.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }
            else if (filePath.StartsWith($"/{config.Build.Dest}/", StringComparison.OrdinalIgnoreCase))
            {
                return filePath.Replace($"/{config.Build.Dest}/", "");
            }
            else if (filePath.StartsWith($"{config.Build.Dest}/", StringComparison.OrdinalIgnoreCase))
            {
                return filePath.Replace($"{config.Build.Dest}/", "");
            }

            return filePath;
        }
    }
}
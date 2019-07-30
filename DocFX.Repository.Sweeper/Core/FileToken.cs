﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocFX.Repository.Sweeper.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileToken
    {
        static readonly RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
        static readonly Regex FileExtensionRegex = new Regex("(\\.\\w+$)", Options);
        static readonly char[] InvalidPathCharacters = Path.GetInvalidPathChars();

        readonly FileInfo _fileInfo;
        readonly Lazy<Task<string[]>> _readAllLinesTask;

        IDictionary<int, string> _codeFenceSlugs;

        public FileToken(FileInfo file)
        {
            _fileInfo = file;
            FileType = _fileInfo.GetFileType();

            if (FileType == FileType.Markdown || FileType == FileType.Yaml)
            {
                _readAllLinesTask =
                    new Lazy<Task<string[]>>(() => File.ReadAllLinesAsync(_fileInfo?.FullName));
            }
        }

        public Metadata? Header { get; private set; }

        public string FilePath => _fileInfo?.FullName;

        public FileType FileType { get; }

        public ISet<string> TopicsReferenced { get; } = new HashSet<string>();

        public ISet<string> ImagesReferenced { get; } = new HashSet<string>();

        public IDictionary<int, string> CodeFenceSlugs => _codeFenceSlugs ?? (_codeFenceSlugs = new Dictionary<int, string>());

        public int TotalReferences => TopicsReferenced.Count + ImagesReferenced.Count;

        public bool IsRelevant => FileType != FileType.NotRelevant && FileType != FileType.Json;

        public bool IsMarkedForDeletion { get; set; }

        public IEnumerable<(int, string)> UnrecognizedCodeFenceSlugs
            => _codeFenceSlugs is null
                ? Enumerable.Empty<(int, string)>()
                : _codeFenceSlugs.Where(kvp => !Taxonomies.Languages.ContainsKey(kvp.Value))
                                 .Select(kvp => (kvp.Key, kvp.Value));

        public bool ContainsInvalidCodeFenceSlugs => UnrecognizedCodeFenceSlugs.Any();

        public override string ToString()
        {
            var type = FileType;
            switch (type)
            {
                case FileType.Markdown:
                case FileType.Yaml:
                    return $"{type} File: {_fileInfo.Name}, references {TopicsReferenced.Count} other files and {ImagesReferenced.Count} images.";

                case FileType.Json:
                case FileType.Image:
                    return FilePath;

                default:
                    return "For all intents and purposes, this is a meaningless file.";
            }
        }

        public bool HasReferenceTo(FileToken other)
        {
            if (TotalReferences == 0)
            {
                return false;
            }

            switch (other.FileType)
            {
                case FileType.Markdown:
                case FileType.Yaml:
                    return TopicsReferenced.Any(file => string.Equals(file, other.FilePath, StringComparison.OrdinalIgnoreCase));

                case FileType.Image:
                    return ImagesReferenced.Any(image => string.Equals(image, other.FilePath, StringComparison.OrdinalIgnoreCase));

                default:
                    return false;
            }
        }

        public async ValueTask InitializeAsync()
        {
            if (_readAllLinesTask is null)
            {
                return;
            }

            var lines = await _readAllLinesTask.Value;
            var dir = _fileInfo?.DirectoryName;
            var type = FileType;

            if (type == FileType.Markdown && Metadata.TryParse(lines, out var metadata))
            {
                Header = metadata;
            }

            foreach (var (tokenType, tokenValue, lineNumber) in
                lines.SelectMany((line, lineNumber) => FindAllTokensInLine(line, lineNumber + 1, FileTypeUtils.MapExpressions(type)))
                     .Where(tuple => !string.IsNullOrEmpty(tuple.Item2)))
            {
                if (tokenType == TokenType.CodeFence)
                {
                    CodeFenceSlugs[lineNumber] = tokenValue;
                    continue;
                }

                if (tokenType == TokenType.Unrecognizable)
                {
                    continue;
                }

                if (TryFindFile(dir, tokenValue, out var fullPath))
                {
                    var file = new FileInfo(fullPath);
                    switch (file.GetFileType())
                    {
                        case FileType.Image:
                            ImagesReferenced.Add(file.FullName);
                            break;
                        case FileType.Markdown:
                            TopicsReferenced.Add(file.FullName);
                            break;
                    }
                }
            }
        }

        IEnumerable<(TokenType, string, int)> FindAllTokensInLine(string line, int lineNumber, IEnumerable<Regex> expressions)
        {
            IEnumerable<(TokenType, string)> GetMatchingValues(Match match)
            {
                if (match is null)
                {
                    yield break;
                }

                if (match.Groups.Any(grp => grp.Name == "slug"))
                {
                    yield return (TokenType.CodeFence, match.Groups["slug"].Value);
                }

                if (match.Groups.Any(grp => grp.Name == "link"))
                {
                    yield return (TokenType.FileReference, match.Groups["link"].Value);
                }

                foreach (Group group in match.Groups)
                {
                    yield return (TokenType.FileReference, group.Value);
                }
            }

            foreach (var value in
                expressions.SelectMany(ex => ex.Matches(line))
                           .SelectMany(GetMatchingValues))
            {
                var (tokenType, tokenValue) = CleanMatching(value);
                yield return (tokenType, tokenValue, lineNumber);
            }
        }

        (TokenType, string) CleanMatching((TokenType tokenType, string tokenValue) tuple)
        {
            var (type, value) = tuple;
            if (string.IsNullOrWhiteSpace(value) ||
                value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("#"))
            {
                return default;
            }

            value = value.Trim();
            if (value.StartsWith(".//"))
            {
                value = value.Substring(3);
            }
            else if (value.StartsWith("./"))
            {
                value = value.Substring(2);
            }
            else if (value.StartsWith("xref:"))
            {
                value = value.Substring(5);
            }

            if (type == TokenType.CodeFence)
            {
                return (type, value);
            }

            var cleaned =
                StripQueryStringOrHeaderLink(value)
                    .Replace("~", "..")
                    .Replace("/azure/", "/articles/");
            var unescaped =
                Uri.UnescapeDataString(cleaned);

            return (type, unescaped);
        }

        static string StripQueryStringOrHeaderLink(string value)
        {
            string SplitOn(string str, string separator)
            {
                if (str.Contains(separator))
                {
                    var split = str.Split(separator);
                    str = split.Length > 0 ? split[0] : str;
                }

                return str;
            }

            value = SplitOn(value, "#");
            return SplitOn(value, "?");
        }

        static bool TryFindFile(string directory, string filePath, out string fullPath)
        {
            if (string.IsNullOrWhiteSpace(directory) ||
                string.IsNullOrWhiteSpace(filePath))
            {
                fullPath = null;
                return false;
            }

            if (filePath.IndexOfAny(InvalidPathCharacters) != -1)
            {
                fullPath = null;
                return false;
            }

            var path = Path.Combine(directory, filePath);
            if (FileExtensionRegex.IsMatch(path))
            {
                fullPath = path;
                return File.Exists(path);
            }

            try
            {
                var files = Directory.GetFiles(directory, $"{filePath}.*");
                if (files.Length > 0)
                {
                    fullPath = files[0];
                    return true;
                }
            }
            catch
            {
            }

            fullPath = null;
            return false;
        }
    }
}
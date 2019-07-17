using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocFX.Repository.Sweeper.Extensions;

namespace DocFX.Repository.Sweeper.Core
{
    class FileToken
    {
        static readonly Regex MarkdownLinkRegex = new Regex(@"[^!]\[.+?\]\((.+?)\)");
        static readonly Regex MarkdownImageLinkRegex = new Regex(@"\!\[(.*?)\][\[\(](.*?)[\ \]\)]");
        static readonly Regex MarkdownIncludeLinkRegex = new Regex(@"\[\!(.*?)\][\[\(](.*?)[\ \]\)]");
        static readonly Regex LinkAttributeRegex = new Regex("(?<=src=\"|href=\")(.*?)(?=\")");
        static readonly Regex YamlLinkRegex = new Regex(@"href:.+?(?'link'.*)");
        static readonly Regex YamlSrcLinkRegex = new Regex(@"src:.+?(?'link'.*)");
        static readonly Regex FileExtensionRegex = new Regex("(\\.\\w+$)");

        readonly FileInfo _fileInfo;
        readonly Lazy<Task<string[]>> _readAllLinesTask;
        readonly HashSet<string> _imagesReferenced = new HashSet<string>();
        readonly HashSet<string> _topicsReferenced = new HashSet<string>();

        internal FileToken(FileInfo file)
        {
            _fileInfo = file;
            FileType = _fileInfo.GetFileType();

            if (FileType == FileType.Markdown || FileType == FileType.Yaml)
            {
                _readAllLinesTask =
                    new Lazy<Task<string[]>>(() => File.ReadAllLinesAsync(_fileInfo?.FullName));
            }
        }

        internal string FilePath => _fileInfo?.FullName;
        internal FileType FileType { get; }
        internal ISet<string> TopicsReferenced => _topicsReferenced;
        internal ISet<string> ImagesReferenced => _imagesReferenced;
        internal int TotalReferences => TopicsReferenced.Count + ImagesReferenced.Count;
        internal bool IsRelevant => FileType != FileType.NotRelevant && FileType != FileType.Json;
        internal bool IsMarkedForDeletion { get; set; }

        public override string ToString()
        {
            var type = FileType;
            switch (type)
            {
                case FileType.Markdown:
                case FileType.Yaml:
                    return $"{type} File: {_fileInfo.Name}, references {_topicsReferenced.Count} other files and {_imagesReferenced.Count} images.";

                case FileType.Json:
                case FileType.Image:
                    return FilePath;

                default:
                    return "For all intents and purposes, this is a meaningless file.";
            }
        }

        internal bool HasReferenceTo(FileToken other)
        {
            if (TotalReferences == 0)
            {
                return false;
            }

            switch (other.FileType)
            {
                case FileType.Markdown:
                case FileType.Yaml:
                    return _topicsReferenced.Any(file => string.Equals(file, other.FilePath, StringComparison.OrdinalIgnoreCase));

                case FileType.Image:
                    return _imagesReferenced.Any(image => string.Equals(image, other.FilePath, StringComparison.OrdinalIgnoreCase));

                default:
                    return false;
            }
        }

        internal async Task InitializeAsync()
        {
            if (_readAllLinesTask is null)
            {
                return;
            }

            var lines = await _readAllLinesTask.Value;
            var dir = _fileInfo?.DirectoryName;
            var type = FileType;

            foreach (var link in
                lines.SelectMany(line => FindAllLinksInLine(line, MapExpressions(type)))
                     .Where(link => !string.IsNullOrEmpty(link)))
            {
                var path = Path.Combine(dir, link);
                if (File.Exists(path))
                {
                    var file = new FileInfo(path);
                    switch (file.GetFileType())
                    {
                        case FileType.Image:
                            _imagesReferenced.Add(file.FullName);
                            break;
                        case FileType.Markdown:
                            _topicsReferenced.Add(file.FullName);
                            break;
                    }
                }
            }
        }

        static IEnumerable<Regex> MapExpressions(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Markdown:
                    yield return MarkdownLinkRegex;
                    yield return MarkdownImageLinkRegex;
                    yield return MarkdownIncludeLinkRegex;
                    yield return LinkAttributeRegex;
                    break;

                case FileType.Yaml:
                    yield return YamlLinkRegex;
                    yield return YamlSrcLinkRegex;
                    yield return LinkAttributeRegex;
                    break;

                default:
                    yield break;
            }
        }

        IEnumerable<string> FindAllLinksInLine(string line, IEnumerable<Regex> expressions)
        {
            string GetMatchingValue(Match match)
            {
                if (match is null)
                {
                    return null;
                }

                if (match.Groups.Any(grp => grp.Name == "link"))
                {
                    return match.Groups["link"].Value;
                }

                if (match.Groups.Count > 0)
                {
                    return match.Groups[match.Groups.Count - 1].Value;
                }

                return match.Value;
            }

            foreach (var value in
                expressions.SelectMany(ex => ex.Matches(line).Cast<Match>())
                           .Select(GetMatchingValue))
            {
                yield return CleanMatching(value);
            }
        }

        string CleanMatching(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("#"))
            {
                return null;
            }

            if (FilePath.EndsWith("GraphSearchMethod.md"))
            {
                Debugger.Break();
            }

            if (value.StartsWith("./"))
            {
                value = value.Replace("./", "");
            }

            var cleaned =
                StripQueryStringOrHeaderLink(value.Trim())
                    .Replace("~", "..")
                    .Replace("/azure/", "/articles/");

            return FileExtensionRegex.IsMatch(cleaned) ? cleaned : $"{cleaned}.md";
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
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocFX.Repository.Sweeper.Extensions;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileToken
    {
        static readonly RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase;

        static readonly Regex MarkdownLinkRegex = new Regex(@"[^!]\[.+?\]\((.+?)\)", Options);
        static readonly Regex MarkdownImageLinkRegex = new Regex(@"\!\[(.*?)\][\[\(](.*?)[\ \]\)]", Options);
        static readonly Regex MarkdownLightboxImageLinkRegex = new Regex(@"\[\!\[(.*?)\][\[\(](.*?)[\ \]\)]\]\((.*?)\)", Options);
        static readonly Regex MarkdownIncludeLinkRegex = new Regex(@"\[\!(.*?)\][\[\(](.*?)[\ \]\)]", Options);
        static readonly Regex MarkdownReferenceLinkRegex = new Regex(@"\[.*\]:(.*)", Options);
        static readonly Regex LinkAttributeRegex = new Regex("(?<=src=\"|href=\")(.*?)(?=\")", Options);
        static readonly Regex YamlLinkRegex = new Regex(@"href:.+?(?'link'.*)", Options);
        static readonly Regex YamlSrcLinkRegex = new Regex(@"src:.+?(?'link'.*)", Options);
        static readonly Regex FileExtensionRegex = new Regex("(\\.\\w+$)", Options);

        readonly FileInfo _fileInfo;
        readonly Lazy<Task<string[]>> _readAllLinesTask;
        readonly HashSet<string> _imagesReferenced = new HashSet<string>();
        readonly HashSet<string> _topicsReferenced = new HashSet<string>();

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

        public string FilePath => _fileInfo?.FullName;
        public FileType FileType { get; }
        public ISet<string> TopicsReferenced => _topicsReferenced;
        public ISet<string> ImagesReferenced => _imagesReferenced;
        public int TotalReferences => TopicsReferenced.Count + ImagesReferenced.Count;
        public bool IsRelevant => FileType != FileType.NotRelevant && FileType != FileType.Json;
        public bool IsMarkedForDeletion { get; set; }

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
                    return _topicsReferenced.Any(file => string.Equals(file, other.FilePath, StringComparison.OrdinalIgnoreCase));

                case FileType.Image:
                    return _imagesReferenced.Any(image => string.Equals(image, other.FilePath, StringComparison.OrdinalIgnoreCase));

                default:
                    return false;
            }
        }

        public async Task InitializeAsync()
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
                    yield return MarkdownLightboxImageLinkRegex;
                    yield return MarkdownImageLinkRegex;
                    yield return MarkdownIncludeLinkRegex;
                    yield return MarkdownReferenceLinkRegex;
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
            IEnumerable<string> GetMatchingValues(Match match)
            {
                if (match is null)
                {
                    yield break;
                }

                if (match.Groups.Any(grp => grp.Name == "link"))
                {
                    yield return match.Groups["link"].Value;
                }

                if (match.Groups.Count == 3)
                {
                    yield return match.Groups[1].Value;
                    yield return match.Groups[2].Value;
                }
                else if (match.Groups.Count > 0)
                {
                    yield return match.Groups[match.Groups.Count - 1].Value;
                }

                yield return match.Value;
            }

            foreach (var value in
                expressions.SelectMany(ex => ex.Matches(line).Cast<Match>())
                           .SelectMany(GetMatchingValues))
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
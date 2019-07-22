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
        static readonly Regex FileExtensionRegex = new Regex("(\\.\\w+$)", Options);

        readonly FileInfo _fileInfo;
        readonly Lazy<Task<string[]>> _readAllLinesTask;

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
        public ISet<string> TopicsReferenced { get; } = new HashSet<string>();
        public ISet<string> ImagesReferenced { get; } = new HashSet<string>();
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

            foreach (var link in
                lines.SelectMany(line => FindAllLinksInLine(line, FileTypeUtils.MapExpressions(type)))
                     .Where(link => !string.IsNullOrEmpty(link)))
            {
                var path = Path.Combine(dir, link);
                if (File.Exists(path))
                {
                    var file = new FileInfo(path);
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

                foreach (Group group in match.Groups)
                {
                    yield return group.Value;
                }
            }

            foreach (var value in
                expressions.SelectMany(ex => ex.Matches(line))
                           .SelectMany(GetMatchingValues)
                           .Select(Uri.UnescapeDataString))
            {
                yield return CleanMatching(value);
            }
        }

        string CleanMatching(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("#"))
            {
                return null;
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

            var cleaned =
                StripQueryStringOrHeaderLink(value)
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
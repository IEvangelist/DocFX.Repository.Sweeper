using DocFX.Repository.Extensions;
using DocFX.Repository.Sweeper.Core;
using DocFX.Repository.Sweeper.OpenPublishing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper
{
    public static class FileTokenExtensions
    {
        public static async ValueTask InitializeAsync(
            this FileToken token,
            Options options,
            string destination = null)
        {
            if (token.FileType == FileType.Markdown ||
                token.FileType == FileType.Yaml)
            {
                var found = await FileTokenCacheUtility.TryFindCachedVersionAsync(token, options, destination);
                if (!found)
                {
                    var lines = await File.ReadAllLinesAsync(token.FilePath);
                    var dir = token.DirectoryName;
                    var type = token.FileType;

                    if (type == FileType.Markdown && Metadata.TryParse(lines, out var metadata))
                    {
                        token.Header = metadata;
                    }

                    foreach (var (tokenType, tokenValue, lineNumber) in
                        lines.SelectMany((line, lineNumber) => FindAllTokensInLine(line, lineNumber + 1, TokenExpressions.FileTypeToExpressionMap[type]))
                             .Where(tuple => !string.IsNullOrEmpty(tuple.Item2)))
                    {
                        if (tokenType == TokenType.CodeFence)
                        {
                            token.CodeFenceSlugs[lineNumber] = tokenValue;
                            continue;
                        }

                        if (tokenType == TokenType.Unrecognizable)
                        {
                            continue;
                        }

                        var (isFound, fullPath) = await FileFinder.TryFindFileAsync(options, dir, tokenValue);
                        if (isFound && !string.IsNullOrWhiteSpace(fullPath))
                        {
                            var file = new FileInfo(fullPath.NormalizePathDelimitors());
                            switch (file.GetFileType())
                            {
                                case FileType.Image:
                                    token.ImagesReferenced.Add(file.FullName);
                                    break;
                                case FileType.Markdown:
                                    token.TopicsReferenced.Add(file.FullName);
                                    break;
                            }
                        }
                    }

                    await FileTokenCacheUtility.CacheTokenAsync(token, options, destination);
                }
            }
        }

        static IEnumerable<(TokenType, string, int)> FindAllTokensInLine(
            string line,
            int lineNumber,
            IEnumerable<Regex> expressions)
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

        static (TokenType, string) CleanMatching((TokenType tokenType, string tokenValue) tuple)
        {
            var (type, value) = tuple;
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith("#"))
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

            var cleaned = StripQueryStringOrHeaderLink(value).Replace("~", "..");
            var unescaped = Uri.UnescapeDataString(cleaned);

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

        public static bool HasReferenceTo(this FileToken token, FileToken other)
        {
            if (token.TotalReferences == 0)
            {
                return false;
            }

            switch (other.FileType)
            {
                case FileType.Markdown:
                case FileType.Yaml:
                    return token.TopicsReferenced
                                .Any(file => string.Equals(file, other.FilePath, StringComparison.OrdinalIgnoreCase));

                case FileType.Image:
                    return token.ImagesReferenced
                                .Any(image => string.Equals(image, other.FilePath, StringComparison.OrdinalIgnoreCase));

                default:
                    return false;
            }
        }

        public static string GetUnrecognizedCodeFenceWarnings(this FileToken token)
        {
            if (!token.ContainsInvalidCodeFenceSlugs)
            {
                return null;
            }

            var builder = new StringBuilder(token.FilePath);
            if (!token.Header.Equals(default))
            {
                builder.AppendLine($"{Environment.NewLine}{token.Header.ToString()}");
            }
            builder.AppendLine($"    Has {token.UnrecognizedCodeFenceSlugs.Count():#,#} unrecognized code fence slugs.");
            foreach (var (line, slug) in token.UnrecognizedCodeFenceSlugs)
            {
                builder.AppendLine($"    line number {line:#,#} has {slug}");
            }

            return builder.ToString();
        }
    }
}
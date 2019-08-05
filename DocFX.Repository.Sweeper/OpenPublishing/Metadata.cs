using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public struct Metadata
    {
        static readonly RegexOptions Options =
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture;

        static readonly Regex GitHubAuthorRegex = new Regex(@"\Aauthor:\s*\b(?'author'.+?)\b", Options);
        static readonly Regex MicrosoftAuthorRegex = new Regex(@"ms.author:\s*\b(?'author'.+?)\b", Options);
        static readonly Regex ManagerRegex = new Regex(@"\Amanager:\s*\b(?'manager'.+?)\b", Options);
        static readonly Regex DateTimeRegex = new Regex(@"ms.date:\s*\b(?'date'.+?)$", Options);

        public string GitHubAuthor;
        public string MicrosoftAuthor;
        public string Manager;
        public DateTime? Date;

        delegate void OnValueParsed(ref Metadata metadata, string value);

        bool IsParsed
            => !string.IsNullOrWhiteSpace(GitHubAuthor)
            || !string.IsNullOrWhiteSpace(MicrosoftAuthor);

        public override string ToString()
        {
            if (IsParsed)
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(GitHubAuthor))
                {
                    builder.Append($"GitHub: {GitHubAuthor}");
                }
                if (!string.IsNullOrWhiteSpace(MicrosoftAuthor))
                {
                    builder.Append($"MS Alias: {MicrosoftAuthor}");
                }
                if (!string.IsNullOrWhiteSpace(Manager))
                {
                    builder.Append($"Manager: {Manager}");
                }
                if (Date.HasValue)
                {
                    builder.Append($"Date: {Date}");
                }
            }

            return string.Empty;
        }

        public static bool TryParse(IEnumerable<string> lines, out Metadata metadata)
        {
            var count = 0;
            bool ParsingMetadata(string line)
            {
                if (line?.Trim() == "---")
                {
                    ++ count;
                }

                return count < 2;
            };

            metadata = new Metadata();
            foreach (var line in 
                lines?.Where(line => !string.IsNullOrWhiteSpace(line))
                      .TakeWhile(ParsingMetadata) ?? Enumerable.Empty<string>())
            {
                if (TryFindNamedGroupValue(ref metadata, GitHubAuthorRegex, line, "author", delegate(ref Metadata md, string author) { md.GitHubAuthor = author; }))
                {
                    continue;
                }
                if (TryFindNamedGroupValue(ref metadata, MicrosoftAuthorRegex, line, "author", delegate (ref Metadata md, string author) { md.MicrosoftAuthor = author; }))
                {
                    continue;
                }
                if (TryFindNamedGroupValue(ref metadata, ManagerRegex, line, "manager", delegate (ref Metadata md, string manager) { md.Manager = manager; }))
                {
                    continue;
                }
                if (TryFindNamedGroupValue(ref metadata, DateTimeRegex, line, "date", delegate(ref Metadata md, string date) 
                {
                    if (DateTime.TryParse(date, out var dateTime))
                    {
                        md.Date = dateTime;
                    }
                }))
                {
                    continue;
                }
            }

            return metadata.IsParsed;
        }

        static bool TryFindNamedGroupValue(
            ref Metadata metadata,
            Regex regex, 
            string line, 
            string groupName,
            OnValueParsed onValueParsed)
        {
            var match = regex.Match(line);
            if (match.Success && match.Groups.Any(grp => grp.Name == groupName))
            {
                onValueParsed(ref metadata, match.Groups[groupName].Value);
            }

            return match.Success;
        }
    }
}
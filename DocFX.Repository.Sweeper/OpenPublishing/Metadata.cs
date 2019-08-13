using DocFX.Repository.Extensions;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    [ProtoContract]
    public struct Metadata
    {
        static readonly RegexOptions Options =
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture;

        static readonly Regex GitHubAuthorRegex = new Regex(@"\Aauthor:\s*\b(?'author'.+?)$", Options);
        static readonly Regex MicrosoftAuthorRegex = new Regex(@"ms.author:\s*\b(?'author'.+?)$", Options);
        static readonly Regex ManagerRegex = new Regex(@"\Amanager:\s*\b(?'manager'.+?)$", Options);
        static readonly Regex DateTimeRegex = new Regex(@"ms.date:\s*\b(?'date'.+?)$", Options);
        static readonly Regex UniversalIdentiferRegex = new Regex(@"uid:\s*\b(?'uid'.+?)$", Options);

        [ProtoMember(1)]
        public string GitHubAuthor;

        [ProtoMember(2)]
        public string MicrosoftAuthor;

        [ProtoMember(3)]
        public string Manager;

        [ProtoMember(4)]
        public DateTime? Date;

        [ProtoMember(5)]
        public string Uid;

        delegate void OnValueParsed(ref Metadata metadata, string value);

        bool IsParsed
            => !string.IsNullOrWhiteSpace(GitHubAuthor)
            && !string.IsNullOrWhiteSpace(MicrosoftAuthor);

        public override string ToString()
        {
            var details = new List<string>();
            if (!string.IsNullOrWhiteSpace(GitHubAuthor))
            {
                details.Add($"https://github.com/{GitHubAuthor}");
            }
            if (!string.IsNullOrWhiteSpace(MicrosoftAuthor))
            {
                details.Add($"Microsoft Alias: {MicrosoftAuthor} (http://who/is/{MicrosoftAuthor})");
            }
            if (!string.IsNullOrWhiteSpace(Manager))
            {
                details.Add($"Manager: {Manager} (http://who/is/{Manager})");
            }
            var (hasValue, date) = Date;
            if (hasValue)
            {
                details.Add($"Date: {date.ToShortDateString()}");
            }
            if (!string.IsNullOrWhiteSpace(Uid))
            {
                details.Add($"Uid: {Uid}");
            }

            return string.Join(", ", details);
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
                if (TryFindNamedGroupValue(ref metadata, UniversalIdentiferRegex, line, "uid", delegate (ref Metadata md, string uid) { md.Uid = uid; }))
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
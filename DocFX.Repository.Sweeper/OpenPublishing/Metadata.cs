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
        delegate void OnValueParsed(ref Metadata metadata, string value);

        static readonly OnValueParsed AssignGitHubAuthor = (ref Metadata md, string author) => md.GitHubAuthor = author;
        static readonly OnValueParsed AssignMicrosoftAuthor = (ref Metadata md, string author) => md.MicrosoftAuthor = author;
        static readonly OnValueParsed AssignManager = (ref Metadata md, string manager) => md.Manager = manager;
        static readonly OnValueParsed AssignUid = (ref Metadata md, string uid) => md.Uid = uid;
        static readonly OnValueParsed AssignTitle = (ref Metadata md, string title) => md.Title = title;
        static readonly OnValueParsed AssignTitleSuffix = (ref Metadata md, string titleSuffix) => md.TitleSuffix = titleSuffix;
        static readonly OnValueParsed AssignTopic = (ref Metadata md, string topic) => md.Topic = topic;
        static readonly OnValueParsed AssignDescription = (ref Metadata md, string description) => md.Description = description;
        static readonly OnValueParsed AssignService = (ref Metadata md, string service) => md.Service = service;
        static readonly OnValueParsed AssignSubservice = (ref Metadata md, string subService) => md.Subservice = subService;
        static readonly OnValueParsed AssignDate = delegate (ref Metadata md, string date)
        {
            if (DateTime.TryParse(date, out var dateTime))
            {
                md.Date = dateTime;
            }
        };

        static readonly RegexOptions Options =
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture;

        static readonly Regex GitHubAuthorRegex = new Regex(@"\Aauthor:\s*\b(?'author'.+?)$", Options);
        static readonly Regex MicrosoftAuthorRegex = new Regex(@"ms.author:\s*\b(?'msauthor'.+?)$", Options);
        static readonly Regex ManagerRegex = new Regex(@"\Amanager:\s*\b(?'manager'.+?)$", Options);
        static readonly Regex TitleRegex = new Regex(@"\Atitle:\s*\b(?'title'.+?)$", Options);
        static readonly Regex TitleSuffixRegex = new Regex(@"\AtitleSuffix:\s*\b(?'titleSuffix'.+?)$", Options);
        static readonly Regex DescriptionRegex = new Regex(@"\Adescription:\s*\b(?'description'.+?)$", Options);
        static readonly Regex DateTimeRegex = new Regex(@"ms.date:\s*\b(?'date'.+?)$", Options);
        static readonly Regex TopicRegex = new Regex(@"ms.topic:\s*\b(?'topic'.+?)$", Options);
        static readonly Regex ServiceRegex = new Regex(@"ms.service:\s*\b(?'service'.+?)$", Options);
        static readonly Regex SubserviceRegex = new Regex(@"ms.subservice:\s*\b(?'subservice'.+?)$", Options);
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

        [ProtoMember(6)]
        public string Title;

        [ProtoMember(7)]
        public string TitleSuffix;

        [ProtoMember(8)]
        public string Service;

        [ProtoMember(9)]
        public string Subservice;

        [ProtoMember(10)]
        public string Description;

        [ProtoMember(11)]
        public string Topic;

        internal bool HasValidDate
            => Date.GetValueOrDefault() > DateTime.MinValue;

        internal bool IsParsed
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
                if (TryFindNamedGroupValue(ref metadata, GitHubAuthorRegex, line, "author", AssignGitHubAuthor)) continue;
                if (TryFindNamedGroupValue(ref metadata, MicrosoftAuthorRegex, line, "msauthor", AssignMicrosoftAuthor)) continue;
                if (TryFindNamedGroupValue(ref metadata, ManagerRegex, line, "manager", AssignManager)) continue;
                if (TryFindNamedGroupValue(ref metadata, UniversalIdentiferRegex, line, "uid", AssignUid)) continue;
                if (TryFindNamedGroupValue(ref metadata, TitleRegex, line, "title", AssignTitle)) continue;
                if (TryFindNamedGroupValue(ref metadata, TitleSuffixRegex, line, "titleSuffix", AssignTitleSuffix)) continue;
                if (TryFindNamedGroupValue(ref metadata, TopicRegex, line, "topic", AssignTopic)) continue;
                if (TryFindNamedGroupValue(ref metadata, DescriptionRegex, line, "description", AssignDescription)) continue;
                if (TryFindNamedGroupValue(ref metadata, ServiceRegex, line, "service", AssignService)) continue;
                if (TryFindNamedGroupValue(ref metadata, SubserviceRegex, line, "subservice", AssignSubservice)) continue;
                if (TryFindNamedGroupValue(ref metadata, DateTimeRegex, line, "date", AssignDate)) continue;
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
﻿using System;
using System.Collections.Generic;
using System.IO;

namespace DocFX.Repository.Sweeper.Core
{
    class FileTokenFreshness
    {
        static readonly ISet<string> LinkTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "article",
            "quickstart",
            "tutorial",
            "overview",
            "conceptual",
            "landing-page",
            "interactive-tutorial",
            "hub-page",
            "guide"
        };

        public int DaysOld { get; set; }
        public DateTime NinetyDaysOldDate { get; set; }
        public DateTime PubDate { get; set; }
        public string Author { get; set; }
        public string Manager { get; set; }
        public string Topic { get; set; }
        public string Subservice { get; set; }
        public string FileName { get; set; }
        public string FolderName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }

        internal static FileTokenFreshness FromToken(
            FileToken token,
            int daysOld,
            string hostUrl,
            string source,
            string destination)
        {
            var header = token.Header;
            var tokenFreshness = new FileTokenFreshness
            {
                DaysOld = daysOld,
                NinetyDaysOldDate = header.Date.GetValueOrDefault().AddDays(90),
                FileName = Path.GetFileName(token.FilePath),
                FolderName = token.DirectoryName,
                Author = header.MicrosoftAuthor,
                Manager = header.Manager,
                Title = header.Title,
                Topic = header.Topic,
                Description = header.Description,
                PubDate = header.Date.GetValueOrDefault(),
                Subservice = header.Subservice
            };

            if (LinkTopics.Contains(header.Topic))
            {
                var index = token.FilePath.IndexOf(source);
                var route =
                    token.FilePath
                         .Substring(index)
                         .Replace(source, destination)
                         .Replace(".md", "")
                         .Replace("\\", "/");

                tokenFreshness.Link = $"{hostUrl}/{route}";
            }

            return tokenFreshness;
        }
    }
}
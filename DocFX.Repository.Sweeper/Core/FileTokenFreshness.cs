using System;
using System.IO;

namespace DocFX.Repository.Sweeper.Core
{
    class FileTokenFreshness
    {
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

            // TODO: consider adding auto generated link

            return tokenFreshness;
        }
    }
}
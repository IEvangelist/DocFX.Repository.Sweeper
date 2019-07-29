using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public struct Metadata
    {
        static readonly RegexOptions Options =
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture;

        internal static readonly Regex GitHubAuthorRegex =
            new Regex(@"\Aauthor:\W\b(?'author'.+?)\b", Options);

        internal static readonly Regex MicrosoftAuthorRegex =
            new Regex(@"ms.author:\W\b(?'author'.+?)\b", Options);

        public string GitHubAuthor;
        public string MicrosoftAuthor;

        bool IsParsed => !string.IsNullOrWhiteSpace(GitHubAuthor) && !string.IsNullOrWhiteSpace(MicrosoftAuthor);

        public static bool TryParse(IEnumerable<string> lines, out Metadata metadata)
        {
            var count = 0;
            bool ParsingMetadata(string line)
            {
                var trimmed = line.Trim();
                if (trimmed == "---")
                {
                    ++ count;
                }

                return count < 2;
            };

            metadata = new Metadata();
            foreach (var line in 
                lines?.Where(line => !string.IsNullOrWhiteSpace(line))
                      .TakeWhile(ParsingMetadata))
            {
                if (TryFindAuthor(GitHubAuthorRegex, line, out var gitHubAuthor))
                {
                    metadata.GitHubAuthor = gitHubAuthor;
                }
                if (TryFindAuthor(MicrosoftAuthorRegex, line, out var microsoftAuthor))
                {
                    metadata.MicrosoftAuthor = microsoftAuthor;
                }
            }

            return metadata.IsParsed;
        }

        static bool TryFindAuthor(Regex regex, string line, out string author)
        {
            var match = regex.Match(line);
            author = match.Success ? match.Groups["author"].Value : null;
            return match.Success;
        }
    }
}
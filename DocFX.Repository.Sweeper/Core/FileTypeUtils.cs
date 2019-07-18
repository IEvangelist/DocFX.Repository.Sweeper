using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.Core
{
    class FileTypeUtils
    {
        internal static readonly IDictionary<FileType, FileTypeUtils> Utilities = new Dictionary<FileType, FileTypeUtils>
        {
            [FileType.Markdown] = new FileTypeUtils { Color = ConsoleColor.Cyan },
            [FileType.Image] = new FileTypeUtils { Color = ConsoleColor.Yellow },
            [FileType.Yaml] = new FileTypeUtils { Color = ConsoleColor.Blue },
            [FileType.Json] = new FileTypeUtils { Color = ConsoleColor.DarkGreen },
            [FileType.NotRelevant] = new FileTypeUtils(),
        };

        static readonly RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase;

        static readonly Regex MarkdownLinkRegex = new Regex(@"[^!]\[.+?\]\((\s*.+?)\)", Options);
        static readonly Regex MarkdownImageLinkRegex = new Regex(@"\!\[(.*?)\][\[\(](\s*.*?)[\ \]\)]", Options);
        static readonly Regex MarkdownLightboxImageLinkRegex = new Regex(@"\[\!\[(.*?)\][\[\(](\s*.*?)[\ \]\)]\]\((.*?)\)", Options);
        static readonly Regex MarkdownIncludeLinkRegex = new Regex(@"\[\!(.*?)\][\[\(](\s*.*?)[\ \]\)]", Options);
        static readonly Regex MarkdownReferenceLinkRegex = new Regex(@"\[.*\]:(\s*.*)", Options);
        static readonly Regex MarkdownReferenceLinkWithTitleRegex = new Regex(@"\[.*\]:(?'link'.+?(?=""))", Options);
        static readonly Regex MarkdownCatchAllLinkRegex = new Regex(@"(.*?)\][\[\(](\s*.*?)[\ \]\)]", Options);
        static readonly Regex LinkAttributeRegex = new Regex("(?<=src=\"|href=\")(.*?)(?=\")", Options);
        static readonly Regex YamlLinkRegex = new Regex(@"href:.+?(?'link'.*)", Options);
        static readonly Regex YamlSrcLinkRegex = new Regex(@"src:.+?(?'link'.*)", Options);

        internal static IEnumerable<Regex> MapExpressions(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Markdown:
                    yield return MarkdownLinkRegex;
                    yield return MarkdownLightboxImageLinkRegex;
                    yield return MarkdownImageLinkRegex;
                    yield return MarkdownIncludeLinkRegex;
                    yield return MarkdownReferenceLinkRegex;
                    yield return MarkdownReferenceLinkWithTitleRegex;
                    yield return MarkdownCatchAllLinkRegex;
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

        internal ConsoleColor Color { get; private set; }
    }
}
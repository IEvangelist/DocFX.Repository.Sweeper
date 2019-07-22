using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileTypeUtils
    {
        internal static readonly IDictionary<FileType, FileTypeUtils> Utilities = new Dictionary<FileType, FileTypeUtils>
        {
            [FileType.Markdown] = new FileTypeUtils { Color = ConsoleColor.Cyan },
            [FileType.Image] = new FileTypeUtils { Color = ConsoleColor.Yellow },
            [FileType.Yaml] = new FileTypeUtils { Color = ConsoleColor.Magenta },
            [FileType.Json] = new FileTypeUtils { Color = ConsoleColor.DarkGreen },
            [FileType.NotRelevant] = new FileTypeUtils(),
        };

        static readonly RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase;

        static readonly Regex MarkdownLinkRegex = new Regex(@"[^!]\[.+?\]\((?'link'\s*.+?)\)", Options);
        static readonly Regex MarkdownImageLinkRegex = new Regex(@"\!\[(.*?)\][\[\(](?'link'\s*.*?)[\ \]\)\r\n]", Options);
        static readonly Regex MarkdownLightboxImageLinkRegex = new Regex(@"\[\!\[(.*?)\][\[\(](\s*.*?)[\ \]\)]\]\((.*?)\)", Options);
        static readonly Regex MarkdownIncludeLinkRegex = new Regex(@"\[\!(.*?)\][\[\(](?'link'\s*.*?)[\ \]\)\r\n]", Options);
        static readonly Regex MarkdownReferenceLinkRegex = new Regex(@"\[.*\]:(?'link'\s*.*)", Options);
        static readonly Regex MarkdownReferenceLinkWithTitleRegex = new Regex(@"\[.*\]:(?'link'.+?(?=""))", Options);
        static readonly Regex MarkdownCatchAllLinkRegex = new Regex(@"(.*?)\][\[\(](?'link'\s*.*?)[\ \]\)\r\n]", Options);
        static readonly Regex MarkdownNestedParathesesRegex = new Regex(@"\](?:[^()]|(?<open>[(])|(?<content-open>[)]))*(?(open)(?!))", Options);
        static readonly Regex SrcLinkAttributeRegex = new Regex("src\\s*=\\s*\"(?'link'.+?)\"", Options);
        static readonly Regex HrefLinkAttributeRegex = new Regex("href\\s*=\\s*\"(?'link'.+?)\"", Options);
        static readonly Regex YamlLinkRegex = new Regex(@"href:.+?(?'link'.*)", Options);
        static readonly Regex YamlSrcLinkRegex = new Regex(@"src:.+?(?'link'.*)", Options);

        static readonly IDictionary<FileType, IEnumerable<Regex>> Expressions =
            new Dictionary<FileType, IEnumerable<Regex>>
            {
                [FileType.Markdown] = new[]
                {
                    MarkdownLinkRegex,
                    MarkdownImageLinkRegex,
                    MarkdownLightboxImageLinkRegex,
                    MarkdownIncludeLinkRegex,
                    MarkdownReferenceLinkRegex,
                    MarkdownReferenceLinkWithTitleRegex,
                    MarkdownNestedParathesesRegex,
                    MarkdownCatchAllLinkRegex,
                    SrcLinkAttributeRegex,
                    HrefLinkAttributeRegex
                },
                [FileType.Yaml] = new[]
                {
                    YamlLinkRegex,
                    YamlSrcLinkRegex,
                    SrcLinkAttributeRegex,
                    HrefLinkAttributeRegex
                }
            };

        public static IEnumerable<Regex> MapExpressions(FileType fileType) => Expressions[fileType];

        internal ConsoleColor Color { get; private set; }
    }
}
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.Core
{
    class TokenExpressions
    {
        static readonly RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
        static readonly RegexOptions ExplicitOptions = Options | RegexOptions.ExplicitCapture;

        internal static readonly Regex MarkdownLinkRegex = new Regex(@"\[(.+?)\]\((?'link'\s*.+?)([\ \]\)\r\n]|\z)", ExplicitOptions);
        internal static readonly Regex MarkdownLightboxImageLinkRegex = new Regex(@"\[\!\[(.*?)\][\[\(](\s*.*?)[\ \]\)]\]\((.*?)\)", Options);
        internal static readonly Regex MarkdownIncludeLinkRegex = new Regex(@"\[\!(.*?)\][\[\(](?'link'\s*.*?)([\ \]\)\r\n]|\z)", ExplicitOptions);
        internal static readonly Regex MarkdownReferenceLinkRegex = new Regex(@"\[.*\]:(?'link'\s*.*)", ExplicitOptions);
        internal static readonly Regex MarkdownReferenceLinkWithTitleRegex = new Regex(@"\[.*\]:(?'link'.+?(?=""))", ExplicitOptions);
        internal static readonly Regex MarkdownCatchAllLinkRegex = new Regex(@"(.*?)\][\[\(](?'link'\s*.*?)([\ \]\)\r\n]|\z)", ExplicitOptions);
        internal static readonly Regex MarkdownNestedParathesesRegex = new Regex(@"\](?:[^()]|(?<open>[(])|(?<link-open>[)]))*(?(open)(?!))", ExplicitOptions);
        internal static readonly Regex SrcLinkAttributeRegex = new Regex("src\\s*=\\s*\"(?'link'.+?)\"", ExplicitOptions);
        internal static readonly Regex HrefLinkAttributeRegex = new Regex("href\\s*=\\s*\"(?'link'.+?)\"", ExplicitOptions);
        internal static readonly Regex CodeFenceRegex = new Regex(@"(`{3,4})(?'slug'.*?[^```])\z|\r\n", ExplicitOptions);
        internal static readonly Regex YamlLinkRegex = new Regex(@"href:.+?(?'link'.*)", ExplicitOptions);
        internal static readonly Regex YamlSrcLinkRegex = new Regex(@"src:.+?(?'link'.*)", ExplicitOptions);

        internal static readonly IDictionary<FileType, IEnumerable<Regex>> FileTypeToExpressionMap =
            new Dictionary<FileType, IEnumerable<Regex>>
            {
                [FileType.Markdown] = new[]
                {
                    MarkdownLinkRegex,
                    MarkdownLightboxImageLinkRegex,
                    MarkdownIncludeLinkRegex,
                    MarkdownReferenceLinkRegex,
                    MarkdownReferenceLinkWithTitleRegex,
                    MarkdownNestedParathesesRegex,
                    MarkdownCatchAllLinkRegex,
                    SrcLinkAttributeRegex,
                    HrefLinkAttributeRegex,
                    CodeFenceRegex
                },
                [FileType.Yaml] = new[]
                {
                    YamlLinkRegex,
                    YamlSrcLinkRegex,
                    SrcLinkAttributeRegex,
                    HrefLinkAttributeRegex
                }
            };
    }
}
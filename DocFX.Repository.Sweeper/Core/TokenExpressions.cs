using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.Core
{
    class TokenExpressions
    {
        static readonly RegexOptions Options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase;
        static readonly RegexOptions ExplicitOptions = Options | RegexOptions.ExplicitCapture;

        static readonly Regex MarkdownLinkRegex = new Regex(@"[^!]\[.+?\]\((?'link'\s*.+?)\)", ExplicitOptions);
        static readonly Regex MarkdownImageLinkRegex = new Regex(@"\!\[(.*?)\][\[\(](?'link'\s*.*?)([\ \]\)\r\n]|\z)", ExplicitOptions);
        static readonly Regex MarkdownLightboxImageLinkRegex = new Regex(@"\[\!\[(.*?)\][\[\(](\s*.*?)[\ \]\)]\]\((.*?)\)", Options);
        static readonly Regex MarkdownIncludeLinkRegex = new Regex(@"\[\!(.*?)\][\[\(](?'link'\s*.*?)([\ \]\)\r\n]|\z)", ExplicitOptions);
        static readonly Regex MarkdownReferenceLinkRegex = new Regex(@"\[.*\]:(?'link'\s*.*)", ExplicitOptions);
        static readonly Regex MarkdownReferenceLinkWithTitleRegex = new Regex(@"\[.*\]:(?'link'.+?(?=""))", ExplicitOptions);
        static readonly Regex MarkdownCatchAllLinkRegex = new Regex(@"(.*?)\][\[\(](?'link'\s*.*?)([\ \]\)\r\n]|\z)", ExplicitOptions);
        static readonly Regex MarkdownNestedParathesesRegex = new Regex(@"\](?:[^()]|(?<open>[(])|(?<content-open>[)]))*(?(open)(?!))", Options);
        static readonly Regex SrcLinkAttributeRegex = new Regex("src\\s*=\\s*\"(?'link'.+?)\"", ExplicitOptions);
        static readonly Regex HrefLinkAttributeRegex = new Regex("href\\s*=\\s*\"(?'link'.+?)\"", ExplicitOptions);
        static readonly Regex CodeFenceRegex = new Regex(@"```(?'slug'.*?[^```])\z|\r\n", ExplicitOptions);
        static readonly Regex YamlLinkRegex = new Regex(@"href:.+?(?'link'.*)", ExplicitOptions);
        static readonly Regex YamlSrcLinkRegex = new Regex(@"src:.+?(?'link'.*)", ExplicitOptions);

        internal static readonly IDictionary<FileType, IEnumerable<Regex>> FileTypeToExpressionMap =
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
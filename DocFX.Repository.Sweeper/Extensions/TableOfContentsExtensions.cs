using DocFX.Repository.Sweeper.OpenPublishing;
using System.Collections.Generic;
using System.Linq;

namespace DocFX.Repository.Sweeper.Extensions
{
    public static class TableOfContentsExtensions
    {
        public static string FindOverviewLink(this List<TableOfContents> tocs)
            => tocs?.FirstOrDefault(toc => toc.IsOverview)
                   ?.items
                   ?.ElementAtOrDefault(0)
                   ?.href;
    }
}
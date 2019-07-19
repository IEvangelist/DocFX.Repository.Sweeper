using System;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class TableOfContents : Reference
    {
        const string Overview = nameof(Overview);
        const string Index = "index.yml";

        public Reference[] items { get; set; }

        public bool IsOverview => string.Equals(name, Overview, StringComparison.OrdinalIgnoreCase);

        public bool IsIndex => string.Equals(href, Index, StringComparison.OrdinalIgnoreCase);
    }

    public class Reference
    {
        public string name { get; set; }
        public string href { get; set; }
    }
}
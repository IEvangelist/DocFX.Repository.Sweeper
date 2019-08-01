namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class DocFxConfig
    {
        internal const string FileName = "docfx.json";

        public Build Build { get; set; }
    }

    public class Build
    {
        public string MarkdownEngineName { get; set; }
        public string Dest { get; set; }
        public Content[] Content { get; set; }
    }

    public class Content
    {
        public string Dest { get; set; }
        public string Src { get; set; }
        public string[] Files { get; set; }
        public string[] Exclude { get; set; }
    }
}
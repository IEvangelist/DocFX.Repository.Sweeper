namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class DocFxConfig
    {
        public Build Build { get; set; }
    }

    public class Build
    {
        public string MarkdownEngineName { get; set; }
        public string Dest { get; set; }
    }
}
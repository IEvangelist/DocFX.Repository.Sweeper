using CommandLine;

namespace DocFX.Repository.Sweeper
{
    public class Options
    {
        [Option('s', "directory", Required = true, HelpText = "The source directory to act on (can be subdirectory or top-level).")]
        public string SourceDirectory { get; set; }

        [Option('t', "topics", Default = true, HelpText = "If true, finds orphaned topic (markdown) files.")]
        public bool FindOrphanedTopics { get; set; } = true;

        [Option('i', "images", Default = true, HelpText = "If true, finds orphaned image files (.png, .jpg, .jpeg, .gif, .svg).")]
        public bool FindOrphanedImages { get; set; } = true;

        [Option('d', "delete", Required = false, HelpText = "If true, deletes orphaned markdown or image files.")]
        public bool Delete { get; set; }
    }
}
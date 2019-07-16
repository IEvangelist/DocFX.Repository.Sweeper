using CommandLine;

namespace DocFX.Repository.Sweeper
{
    class Options
    {
        [Option('s', "directory", Required = true, HelpText = "Directory to start search for markdown files, or the media directory to search in for orphaned .png files, or the directory to search in for orphaned INCLUDE files.")]
        public string SourceDirectory { get; set; }

        [Option('t', "topics", Default = true, HelpText = "Use this option to find orphaned topic (markdown files).")]
        public bool FindOrphanedTopics { get; set; } = true;

        [Option('i', "images", Default = true, HelpText = "Use this option to find orphaned image files (.png, .jpg, .jpeg, .gif, .svg).")]
        public bool FindOrphanedImages { get; set; } = true;

        [Option('d', "delete", Required = false, HelpText = "Set to true to delete orphaned markdown or image files.")]
        public bool Delete { get; set; }
    }
}
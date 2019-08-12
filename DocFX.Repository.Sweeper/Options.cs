using DocFX.Repository.Extensions;
using CommandLine;
using System;
using System.IO;

namespace DocFX.Repository.Sweeper
{
    public class Options
    {
        readonly Lazy<DirectoryInfo> _sourceDirectory;
        readonly Lazy<DirectoryInfo> _docFxJsonDirectory;
        readonly Lazy<Uri> _directoryUri;
        readonly Lazy<Uri> _hostUri;

        public Options()
        {
            _sourceDirectory = new Lazy<DirectoryInfo>(() => new DirectoryInfo(SourceDirectory));
            _docFxJsonDirectory = new Lazy<DirectoryInfo>(() => new DirectoryInfo(SourceDirectory).TraverseToFile("docfx.json"));
            _directoryUri = new Lazy<Uri>(() => new Uri(SourceDirectory));
            _hostUri = new Lazy<Uri>(() => new Uri(HostUrl));
        }

        public DirectoryInfo Directory => _sourceDirectory.Value;

        public DirectoryInfo DocFxJsonDirectory => _docFxJsonDirectory.Value;

        public string NormalizedDirectory => Directory?.FullName.NormalizePathDelimitors() ?? null;

        public Uri DirectoryUri => _directoryUri.Value;

        public Uri HostUri => _hostUri.Value;

        [Option('s', "directory", Required = true, HelpText = "The source directory to act on (can be subdirectory or top-level).")]
        public string SourceDirectory { get; set; }

        [Option('t', "topics", HelpText = "If true, finds orphaned topic (markdown) files.")]
        public bool FindOrphanedTopics { get; set; }

        [Option('i', "images", HelpText = "If true, finds orphaned image files (.png, .jpg, .jpeg, .gif, .svg).")]
        public bool FindOrphanedImages { get; set; }

        [Option('e', "explicitScope", HelpText = "If true, will only evaluate cross references within the source directory (WARNING, this could cause errant deletions).")]
        public bool ExplicitScope { get; set; }

        [Option('l', "limit", HelpText = "The number of files to limit for a given sweep (if not specified will eagerly delete all possible).")]
        public int DeletionLimit { get; set; }

        [Option('d', "delete", HelpText = "If true, deletes orphaned markdown or image files.")]
        public bool Delete { get; set; }

        [Option('o', "outputWarnings", HelpText = "If true, writes various warnings to the standard output.")]
        public bool OutputWarnings { get; set; }

        [Option('r', "redirects", HelpText = "If true, writes redirections of deleted files to .openpublishing.redirection.json.")]
        public bool ApplyRedirects { get; set; }

        [Option('h', "hosturl", Default = "https://docs.microsoft.com", HelpText = "If 'redirects' is true, this is required and is the host where the docs site is hosted.")]
        public string HostUrl { get; set; } = "https://docs.microsoft.com";

        [Option('q', "query", HelpText = "If 'redirects' is true, this is an optional query string to be applied to redirect validation.")]
        public string QueryString { get; set; }

        [Option('c', "cache", HelpText = "If true, enables caching of file tokens (much faster sequential execution).")]
        public bool EnableCaching { get; set; }
    }
}
﻿using CommandLine;
using System;
using System.IO;

namespace DocFX.Repository.Sweeper
{
    public class Options
    {
        readonly Lazy<DirectoryInfo> _sourceDirectory;
        readonly Lazy<Uri> _directoryUri;

        public Options()
        {
            _sourceDirectory = new Lazy<DirectoryInfo>(() => new DirectoryInfo(SourceDirectory));
            _directoryUri = new Lazy<Uri>(() => new Uri(SourceDirectory));
        }

        public DirectoryInfo Directory => _sourceDirectory.Value;

        public Uri DirectoryUri => _directoryUri.Value;

        [Option('s', "directory", Required = true, HelpText = "The source directory to act on (can be subdirectory or top-level).")]
        public string SourceDirectory { get; set; }

        [Option('t', "topics", HelpText = "If true, finds orphaned topic (markdown) files.")]
        public bool FindOrphanedTopics { get; set; }

        [Option('i', "images", HelpText = "If true, finds orphaned image files (.png, .jpg, .jpeg, .gif, .svg).")]
        public bool FindOrphanedImages { get; set; }

        [Option('l', "limit", HelpText = "The number of files to limit for a given sweep (if not specified will eagerly delete all possible).")]
        public int DeletionLimit { get; set; }

        [Option('d', "delete", HelpText = "If true, deletes orphaned markdown or image files.")]
        public bool Delete { get; set; }

        [Option('o', "outputWarnings", HelpText = "If true, writes various warnings to the standard output.")]
        public bool OutputWarnings { get; set; }

        [Option('r', "redirects", HelpText = "If true, writes redirections of deleted files to .openpublishing.redirection.json.")]
        public bool ApplyRedirects { get; set; }

        [Option('h', "hosturl", Default = "https://docs.microsoft.com", HelpText = "If 'redirects' is true, this is required and is the host where the docs site is hosted.")]
        public string HostUrl { get; set; }

        [Option('q', "query", HelpText = "If 'redirects' is true, this is an optional query string to be applied to redirect validation.")]
        public string QueryString { get; set; }
    }
}
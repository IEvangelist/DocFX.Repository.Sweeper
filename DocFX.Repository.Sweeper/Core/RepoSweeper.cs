﻿using Kurukuru;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper.Core
{
    public class RepoSweeper
    {
        readonly FileTokenizer _fileTokenizer = new FileTokenizer();
        readonly RedirectionAppender _redirectionAppender = new RedirectionAppender();

        public async Task<SweepSummary> SweepAsync(Options options, Stopwatch stopwatch)
        {
            var orphanedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var orphanedTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ConsoleColor.Green.WriteLine($"\nSearching \"{options.SourceDirectory}\" for orphaned files.");

            var (status, tokenMap) = await _fileTokenizer.TokenizeAsync(options);
            if (status == Status.Error)
            {
                status.WriteLine($"Unexpected error... early exit.");
                return new SweepSummary { Status = status };
            }

            var allTokensInMap = tokenMap.SelectMany(kvp => kvp.Value).Where(t => t.TotalReferences > 0);
            Console.WriteLine($"Spent {stopwatch.Elapsed.ToHumanReadableString()} tokenizing files.");
            Console.WriteLine();

            var directory = options.DirectoryUri;
            var typeStopwatch = new Stopwatch();

            foreach (var type in
                tokenMap.Keys
                        .Where(IsRelevantToken)
                        .OrderBy(type => type))
            {
                if (type == FileType.Image && !options.FindOrphanedImages ||
                    type == FileType.Markdown && !options.FindOrphanedTopics)
                {
                    continue;
                }

                typeStopwatch.Restart();

                var allTokens = tokenMap[type];
                var tokens = allTokens.Where(token => token.IsRelevant);
                var count = 0;

                Spinner.Start($"Scoping \"{type}\" file workload.", spinner =>
                {
                    spinner.Color = ConsoleColor.Blue;
                    count = tokens.Count();
                    spinner.Succeed();
                }, Patterns.Arc);

                type.WriteLine($"Evaluating \"{type}\" files. Scanning for cross references throughout the entire DocFx doc set.");

                using (var progressBar =
                    new ProgressBar(
                        count,
                        $"{type} files...",
                        new ProgressBarOptions
                        {
                            ForegroundColor = FileTypeUtils.Utilities[type].Color
                        }))
                {
                    Parallel.ForEach(
                        tokens.OrderBy(token => token.FilePath),
                        (token, state) =>
                        {
                            var relative = directory.MakeRelativeUri(new Uri(token.FilePath));
                            progressBar.Tick($"{type} files...{relative}");

                            if (IsTokenReferencedAnywhere(token, allTokensInMap))
                            {
                                return;
                            }

                            if (!IsTokenWithinScopedDirectory(token, options.NormalizedDirectory))
                            {
                                return;
                            }

                            switch (token.FileType)
                            {
                                case FileType.Markdown:
                                    if (options.FindOrphanedTopics && orphanedTopics.Add(token.FilePath))
                                    {
                                        token.IsMarkedForDeletion = options.Delete;
                                    }
                                    break;

                                case FileType.Image:
                                    if (options.FindOrphanedImages && orphanedImages.Add(token.FilePath))
                                    {
                                        token.IsMarkedForDeletion = options.Delete;
                                    }
                                    break;
                            }
                        });

                    typeStopwatch.Stop();
                    progressBar.Tick($"- {count:#,#} \"{type}\" files processed in {typeStopwatch.Elapsed.ToHumanReadableString()}.");
                }
            }

            if (options.FindOrphanedTopics)
            {
                await HandleOrphanedFilesAsync(orphanedTopics, FileType.Markdown, options);
            }

            if (options.FindOrphanedImages)
            {
                await HandleOrphanedFilesAsync(orphanedImages, FileType.Image, options);
            }

            return new SweepSummary
            {
                Status = Status.Success,
                TotalFilesProcessed = tokenMap.SelectMany(kvp => kvp.Value).Count(),
                TotalCrossReferences = tokenMap.Select(kvp => kvp.Value.Sum(t => t.TotalReferences)).Sum()
            };
        }

        static bool IsTokenWithinScopedDirectory(FileToken token, string sourceDir) 
            => token?.FilePath?.IndexOf(sourceDir, StringComparison.OrdinalIgnoreCase) != -1;

        static bool IsTokenReferencedAnywhere(FileToken fileToken, IEnumerable<FileToken> tokens)
            => tokens.Where(token => token != fileToken)
                     .Any(otherToken =>
                         !otherToken.IsMarkedForDeletion &&
                         otherToken.HasReferenceTo(fileToken));

        static bool IsRelevantToken(FileType fileType)
            => fileType != FileType.NotRelevant && fileType != FileType.Json;

        async Task HandleOrphanedFilesAsync(ISet<string> files, FileType type, Options options)
        {
            if (files.Any())
            {
                type.WriteLine($"Found {files.Count:#,#} orphaned {type} files.");

                IEnumerable<string> LimitFiles(ISet<string> values, Options opts)
                {
                    return values.OrderBy(fileName => fileName)
                                 .TakeWhile((_, index) => opts.DeletionLimit == 0 || index < opts.DeletionLimit);
                }

                var workingFiles = LimitFiles(files, options);
                foreach (var (ext, count) in
                    files.Select(file => Path.GetExtension(file).ToUpper())
                         .GroupBy(ext => ext)
                         .Select(grp => (grp.Key, grp.Count())))
                {
                    type.WriteLine($"    {count:#,#} ({ext}) files");
                }

                if (options.Delete)
                {
                    if (type == FileType.Markdown && options.ApplyRedirects)
                    {
                        await _redirectionAppender.ApplyRedirectsAsync(workingFiles, options);
                    }

                    foreach (var file in workingFiles.Where(File.Exists))
                    {
                        if (options.OutputWarnings)
                        {
                            type.WriteLine($"Deleting: {file}.");
                        }

                        File.Delete(file);
                    }

                    type.WriteLine($"Deleted {workingFiles.Count():#,#} {type} files.");
                }

                Console.WriteLine();
            }
            else
            {
                type.WriteLine($"Wow, awesome! There are zero orphaned {type} files...");
            }
        }
    }
}
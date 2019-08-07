using Kurukuru;
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

        public async ValueTask<SweepSummary> SweepAsync(Options options, Stopwatch stopwatch)
        {
            broadenSweep:

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

                var scopedOrEntire = options.ExplicitScope ? "explicitly scoped" : "entire";
                type.WriteLine($"Evaluating \"{type}\" files. Scanning for cross references throughout the {scopedOrEntire} DocFx doc set.");

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
                            var relative = options.DirectoryUri.ToRelativePath(token.FilePath);
                            progressBar.Tick($"{type} files: {relative}");

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


            // If the user attempts a quick delete pass, and there are deletions found.
            // Broaden the sweep to ensure that we're not mistakenly deleting things...
            if (options.ExplicitScope && options.Delete)
            {
                if (options.FindOrphanedTopics && orphanedTopics.Count > 0 ||
                    options.FindOrphanedImages && orphanedImages.Count > 0)
                {
                    options.ExplicitScope = false;
                    goto broadenSweep; // Ugh, a goto... really?!
                }
            }

            if (options.FindOrphanedTopics)
            {
                await HandleOrphanedFilesAsync(orphanedTopics, FileType.Markdown, options);
            }

            if (options.OutputWarnings && tokenMap.ContainsKey(FileType.Markdown))
            {
                await WriteMarkdownWarningsAsync(tokenMap);
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

        static async ValueTask WriteMarkdownWarningsAsync(IDictionary<FileType, IList<FileToken>> tokenMap)
        {
            var path = Path.GetFullPath("warning.txt");
            using (var writer = new StreamWriter(path))
            {
                foreach (var warning in
                    tokenMap[FileType.Markdown].Where(md => md.ContainsInvalidCodeFenceSlugs)
                                               .OrderBy(md => md.FilePath)
                                               .Select(md => md.GetUnrecognizedCodeFenceWarnings()))
                {
                    await writer.WriteLineAsync(warning);
                }

                ConsoleColor.DarkMagenta.WriteLine($"Warnings written to: {path}");
            }
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

        async ValueTask HandleOrphanedFilesAsync(ISet<string> files, FileType type, Options options)
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
                         .OrderBy(grp => grp.Key)
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
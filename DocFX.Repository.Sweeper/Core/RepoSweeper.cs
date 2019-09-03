using DocFX.Repository.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;
using Kurukuru;
using ServiceStack;
using ShellProgressBar;
using System;
using System.Collections.Concurrent;
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

        ConcurrentBag<(int, FileToken)> _freshnessTokens;

        ConcurrentBag<(int, FileToken)> FreshnessTokens => _freshnessTokens ?? (_freshnessTokens = new ConcurrentBag<(int, FileToken)>());

        public async ValueTask<SweepSummary> SweepAsync(Options options, Stopwatch stopwatch)
        {
            broadenSweep: // label for usage of "goto", can't go recursive as variables stack.

            var orphanedImages = new ConcurrentDictionary<string, FileToken>(StringComparer.OrdinalIgnoreCase);
            var orphanedTopics = new ConcurrentDictionary<string, FileToken>(StringComparer.OrdinalIgnoreCase);

            ConsoleColor.Green.WriteLine($"\nScanning \"{options.SourceDirectory}\" for relevant files.");

            var today = DateTime.Now.Date;
            var config = await options.GetConfigAsync();
            var (status, tokenMap) = await _fileTokenizer.TokenizeAsync(options, config);
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
                    type == FileType.Markdown && !options.FindOrphanedTopics && !options.ReportFreshness)
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
                    if (type == FileType.Markdown && !options.ReportFreshness)
                    {
                        // Map topic references from Xrefs.
                        var uidToFilePathMap =
                            tokens.Where(t => !string.IsNullOrWhiteSpace(t.Header.Uid))
                                  .ToDictionary(t => t.Header.Uid, t => t.FilePath, StringComparer.OrdinalIgnoreCase);
                        foreach (var token in tokens)
                        {
                            ++ count;
                            foreach (var uid in token.Xrefs)
                            {
                                if (uidToFilePathMap.TryGetValue(uid, out var filePath))
                                {
                                    token.TopicsReferenced.Add(filePath);
                                }
                            }
                        }
                    }
                    else
                    {
                        count = tokens.Count();
                    }

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

                            if (IsTokenReportingFreshness(options, token, today)) return;
                            if (IsTokenWhiteListed(token)) return;
                            if (IsTokenReferencedAnywhere(token, allTokensInMap)) return;
                            if (!IsTokenWithinScopedDirectory(token, options.NormalizedDirectory)) return;

                            switch (token.FileType)
                            {
                                case FileType.Markdown:
                                    if (options.FindOrphanedTopics && orphanedTopics.TryAdd(token.FilePath, token))
                                    {
                                        token.IsMarkedForDeletion = options.Delete;
                                    }
                                    break;

                                case FileType.Image:
                                    if (options.FindOrphanedImages && orphanedImages.TryAdd(token.FilePath, token))
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
                    options.EnableCaching = options.ExplicitScope = false;
                    goto broadenSweep; // Ugh, a goto... really - but can't go recursive 
                }
            }

            await HandlePostProcessingAsync(options, orphanedImages, orphanedTopics, today, config, tokenMap);

            return new SweepSummary
            {
                Status = Status.Success,
                TotalFilesProcessed = tokenMap.SelectMany(kvp => kvp.Value).Count(),
                TotalCrossReferences = tokenMap.Select(kvp => kvp.Value.Sum(t => t.TotalReferences)).Sum()
            };
        }

        async Task HandlePostProcessingAsync(
            Options options,
            IDictionary<string, FileToken> orphanedImages,
            IDictionary<string, FileToken> orphanedTopics,
            DateTime today,
            DocFxConfig config, 
            IDictionary<FileType, IList<FileToken>> tokenMap)
        {
            if (options.ReportFreshness)
            {
                await WriteFreshnessReportAsync(FreshnessTokens, today, options.HostUrl, config.Build.Dest);
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
        }

        bool IsTokenReportingFreshness(Options options, FileToken token, DateTime today)
        {
            if (options.ReportFreshness &&
                token.Header.IsParsed &&
                token.Header.HasValidDate)
            {
                if (IsTokenWithinScopedDirectory(token, options.NormalizedDirectory))
                {
                    // Eighty days out, so we have 10 days to "keep it fresh"
                    var daysOld = (token.Header.Date.Value - today).Days;
                    if (daysOld <= -80)
                    {
                        FreshnessTokens.Add((daysOld, token));
                    }
                }

                return true;
            }

            return false;
        }

        static async ValueTask WriteFreshnessReportAsync(
            IEnumerable<(int, FileToken)> tokens,
            DateTime today,
            string hostUrl,
            string destination)
        {
            if (tokens.Any())
            {
                var path = Path.GetFullPath($"freshness-{today:yyyyMMdd}.csv");
                using (var writer = new StreamWriter(path))
                {
                    var freshnessTokens =
                        tokens.OrderBy(_ => _.Item1)
                              .ThenBy(_ => _.Item2.FilePath)
                              .Select(tuple => FileTokenFreshness.FromToken(tuple.Item2, tuple.Item1, hostUrl, destination))
                              .ToList();

                    await writer.WriteAsync(freshnessTokens.ToCsv());
                }

                ConsoleColor.DarkMagenta.WriteLine($"Freshness written to \"{path}\"");
            }
        }

        static async ValueTask WriteMarkdownWarningsAsync(IDictionary<FileType, IList<FileToken>> tokenMap)
        {
            var path = Path.GetFullPath("unrecognized-code-slugs.txt");
            using (var writer = new StreamWriter(path))
            {
                var warnings =
                    tokenMap[FileType.Markdown].Where(md => md.ContainsInvalidCodeFenceSlugs
                                                         && md.Header.IsParsed)
                                               .OrderBy(md => md.Header.Manager
                                                           ?? md.Header.MicrosoftAuthor
                                                           ?? md.Header.GitHubAuthor)
                                               .ThenByDescending(md => md.Header.Date)
                                               .ThenBy(md => md.FilePath)
                                               .Select(md => md.GetUnrecognizedCodeFenceWarnings())
                                               .ToList();

                await writer.WriteLineAsync($"Found {warnings.Count:#,#} files with unrecognized code slugs.");
                foreach (var warning in warnings)
                {
                    await writer.WriteLineAsync(warning);
                }

                ConsoleColor.DarkMagenta.WriteLine($"Warnings written to \"{path}\"");
            }
        }

        internal static bool IsTokenWhiteListed(FileToken token)
        {
            if (token is null || !token.IsRelevant)
            {
                return true;
            }

            var isWhiteListed = false;
            switch (token.FileType)
            {
                case FileType.Markdown:
                    // Markdown files named "f1*.md" or in our whitelisting are whitelisted.
                    var fileName = Path.GetFileNameWithoutExtension(token.FilePath);
                    isWhiteListed =
                        fileName.Contains("f1", StringComparison.OrdinalIgnoreCase) ||
                        Whitelisted.FileNames.Any(name => string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase));
                    break;
                case FileType.Image:
                    // Image files in wwwroot, sample or snippet directories are whitelisted.
                    isWhiteListed = token.FilePath.ContainsAny(Whitelisted.DirectoryNames);
                    break;
            }

            return isWhiteListed;
        }

        internal static bool IsTokenWithinScopedDirectory(FileToken token, string sourceDir)
            => token?.FilePath?.IndexOf(sourceDir, StringComparison.OrdinalIgnoreCase) != -1;

        internal static bool IsTokenReferencedAnywhere(FileToken fileToken, IEnumerable<FileToken> tokens)
            => tokens.Where(token => !ReferenceEquals(token, fileToken))
                     .Any(otherToken => otherToken.HasReferenceTo(fileToken));

        static bool IsRelevantToken(FileType fileType)
            => fileType != FileType.NotRelevant && fileType != FileType.Json;

        async ValueTask HandleOrphanedFilesAsync(IDictionary<string, FileToken> files, FileType type, Options options)
        {
            if (files.Any())
            {
                type.WriteLine($"Found {files.Count:#,#} orphaned {type} files.");

                foreach (var (ext, count) in
                    files.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                         .Select(kvp => Path.GetExtension(kvp.Key).ToUpper())
                         .GroupBy(ext => ext)
                         .OrderBy(grp => grp.Key)
                         .Select(grp => (grp.Key, grp.Count())))
                {
                    type.WriteLine($"    {count:#,#} ({ext}) files");
                }

                if (options.Delete)
                {
                    long bytesDeleted = 0;
                    var deletedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var (file, info) in
                        files.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                             .Where(kvp => File.Exists(kvp.Key))
                             .OrderBy(kvp => kvp.Key))
                    {
                        try
                        {
                            File.Delete(file);

                            if (!File.Exists(file) && deletedFiles.Add(file))
                            {
                                bytesDeleted += info.FileSizeInBytes;
                                if (options.OutputWarnings)
                                {
                                    type.WriteLine($"Deleted {file} - {info.Header.ToString()}.");
                                }
                            }

                            if (options.DeletionLimit > 0 && deletedFiles.Count == options.DeletionLimit)
                            {
                                break;
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (type == FileType.Markdown && options.ApplyRedirects)
                    {
                        await _redirectionAppender.ApplyRedirectsAsync(deletedFiles, options);
                    }

                    type.WriteLine($"Deleted {deletedFiles.Count:#,#} {type} files, a total of {bytesDeleted.FromBytesToString()}.");
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
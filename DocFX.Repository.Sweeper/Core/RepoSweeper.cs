﻿using DocFX.Repository.Sweeper.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;
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

        public async Task<SweepSummary> SweepAsync(Options options, Stopwatch stopwatch)
        {
            var orphanedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var orphanedTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var directoryStringLength = options.SourceDirectory.Length;
            var directory = new Uri(options.SourceDirectory);

            Console.WriteLine();
            ConsoleColor.Green.WriteLine($"Searching \"{options.SourceDirectory}\" for orphaned files.");

            var (status, tokenMap) = await _fileTokenizer.TokenizeAsync(options);
            if (status == TokenizationStatus.Error)
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
                var allTokens = tokenMap[type];

                typeStopwatch.Restart();

                var tokens = allTokens.Where(token => token.IsRelevant);
                var count = 0;
                Spinner.Start($"Counting {type} files...", spinner =>
                {
                    spinner.Color = ConsoleColor.Blue;
                    count = tokens.Count();
                    spinner.Succeed();
                }, Patterns.Arc);

                type.WriteLine($"Processing {type} files");

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
                        token =>
                        {
                            var relative = directory.MakeRelativeUri(new Uri(token.FilePath));
                            progressBar.Tick($"{type} files...{relative}");

                            if (IsTokenReferencedAnywhere(token, allTokensInMap))
                            {
                                return;
                            }

                            switch (token.FileType)
                            {
                                case FileType.Markdown:
                                    if (options.FindOrphanedTopics &&
                                        IsTokenWithinScopedDirectory(token, options.SourceDirectory, directoryStringLength))
                                    {
                                        orphanedTopics.Add(token.FilePath);
                                        token.IsMarkedForDeletion = options.Delete;
                                    }
                                    break;

                                case FileType.Image:
                                    if (options.FindOrphanedImages &&
                                        IsTokenWithinScopedDirectory(token, options.SourceDirectory, directoryStringLength))
                                    {
                                        orphanedImages.Add(token.FilePath);
                                        token.IsMarkedForDeletion = options.Delete;
                                    }
                                    break;
                            }
                        });

                    typeStopwatch.Stop();
                    progressBar.Tick($"- {type} files processed in {typeStopwatch.Elapsed.ToHumanReadableString()}.");
                }
            }

            if (options.FindOrphanedTopics)
            {
                await HandleFoundFilesAsync(orphanedTopics, FileType.Markdown, options);
            }

            if (options.FindOrphanedImages)
            {
                await HandleFoundFilesAsync(orphanedImages, FileType.Image, options);
            }

            return new SweepSummary
            {
                Status = TokenizationStatus.Success,
                TotalFilesProcessed = tokenMap.SelectMany(kvp => kvp.Value).Count(),
                TotalCrossReferences = tokenMap.Select(kvp => kvp.Value.Sum(t => t.TotalReferences)).Sum()
            };
        }

        static bool IsTokenWithinScopedDirectory(FileToken token, string sourceDir, int directoryStringLength)
        {
            var tokenPath =
                token.FilePath.Length > directoryStringLength
                    ? token.FilePath.Substring(0, directoryStringLength)
                    : null;
            var isWithinScopedDirectory =
                tokenPath?.Equals(sourceDir, StringComparison.OrdinalIgnoreCase) ?? false;

            return isWithinScopedDirectory;
        }

        static bool IsTokenReferencedAnywhere(FileToken fileToken, IEnumerable<FileToken> tokens)
            => tokens.Where(token => token != fileToken)
                     .Any(otherToken =>
                         !otherToken.IsMarkedForDeletion &&
                         otherToken.HasReferenceTo(fileToken));

        static bool IsRelevantToken(FileType fileType)
            => fileType != FileType.NotRelevant && fileType != FileType.Json;

        static async Task HandleFoundFilesAsync(ISet<string> files, FileType type, Options options)
        {
            if (files.Any())
            {
                type.WriteLine($"Found {files.Count:#,#} orphaned {type} files.");

                foreach (var (ext, count) in
                    files.Select(file => Path.GetExtension(file).ToLower())
                         .GroupBy(ext => ext)
                         .Select(grp => (grp.Key, grp.Count())))
                {
                    type.WriteLine($"    [found {ext} {count:#,#} files]");
                }

                if (options.Delete)
                {
                    if (type == FileType.Markdown)
                    {
                        await Task.CompletedTask;
                        // await HandleRedirectionAsync(files, options);
                    }

                    foreach (var file in files.Where(File.Exists))
                    {
                        if (options.OutputDeletedFiles)
                        {
                            type.WriteLine($"Deleting: {file}.");
                        }

                        File.Delete(file);
                    }

                    type.WriteLine($"Deleted {files.Count():#,#} {type} files.");
                }

                Console.WriteLine();
            }
        }

        static async Task HandleRedirectionAsync(ISet<string> files, Options options)
        {
            var redirection = 
                await FindFileAsync<Redirection>(
                    ".openpublishing.redirection.json",
                    options.SourceDirectory);

            if (redirection?.Redirections.Any() ?? false)
            {

                // foreach file order by path
                // get directory
                // from dir find TOC.yml, if there is not an INDEX.yml file... then find the top-level .md
                // If the INDEX contains "documentType: LandingData" then we're done

                // EXAMPLE:
                // 
                // { 
                //     "source_path": "articles/cognitive-services/Computer-vision/QuickStarts/curl-disk.md",
                //     "redirect_url": "/azure/cognitive-services/computer-vision",
                //     "redirect_document_id": false
                // }
            }
        }

        private static async Task<T> FindFileAsync<T>(string filename, string directory)
        {
            var dir = new DirectoryInfo(directory).TraverseToFile(filename);
            var filepath = Path.Combine(dir.FullName, filename);
            if (File.Exists(filepath))
            {
                var json = await File.ReadAllTextAsync(filepath);
                return json.FromJson<T>();
            }

            return default;
        }
    }
}
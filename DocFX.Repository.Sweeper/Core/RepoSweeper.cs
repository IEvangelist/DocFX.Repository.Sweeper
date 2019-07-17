using DocFX.Repository.Sweeper.Extensions;
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

        public async Task SweepAsync(Options options, Stopwatch stopwatch)
        {
            var orphanedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var orphanedTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var directoryStringLength = options.SourceDirectory.Length;
            var directory = new Uri(options.SourceDirectory);
            WriteLine($"Searching \"{options.SourceDirectory}\" for orphaned files.", ConsoleColor.DarkCyan);

            var (status, tokenMap) = await _fileTokenizer.TokenizeAsync(options);
            if (status == TokenizationStatus.Error)
            {
                WriteLine($"Unexpected error... early exit.", ConsoleColor.DarkRed);
                return;
            }

            var allTokensInMap = tokenMap.SelectMany(kvp => kvp.Value).Where(t => t.TotalReferences > 0);
            WriteLine($"Spent {stopwatch.Elapsed.ToHumanReadableString()} tokenizing files.", ConsoleColor.Red);

            var typeStopwatch = new Stopwatch();
            foreach (var key in
                tokenMap.Keys
                        .Where(IsRelevantToken)
                        .OrderBy(type => type))
            {
                var allTokens = tokenMap[key];

                typeStopwatch.Restart();
                WriteLine($"\nProcessing {key} files", ConsoleColor.Cyan);

                Parallel.ForEach(
                    allTokens.OrderBy(token => token.FilePath)
                             .Where(token =>
                                    token.IsRelevant &&
                                    !IsTokenReferencedAnywhere(token, allTokensInMap)),
                    token =>
                    {
                        switch (token.FileType)
                        {
                            case FileType.Markdown:
                                if (options.FindOrphanedTopics &&
                                    IsTokenWithinScopedDirectory(token, options.SourceDirectory, directoryStringLength))
                                {
                                    WriteNonReferencedFileToOutput(token, directory);
                                    orphanedTopics.Add(token.FilePath);
                                    token.IsMarkedForDeletion = options.Delete;
                                }
                                break;

                            case FileType.Image:
                                if (options.FindOrphanedImages &&
                                    IsTokenWithinScopedDirectory(token, options.SourceDirectory, directoryStringLength))
                                {
                                    WriteNonReferencedFileToOutput(token, directory);
                                    orphanedImages.Add(token.FilePath);
                                    token.IsMarkedForDeletion = options.Delete;
                                }
                                break;
                        }
                    });

                typeStopwatch.Stop();
                WriteLine($"Processed {key} files in {typeStopwatch.Elapsed.ToHumanReadableString()}", ConsoleColor.Cyan);
            }

            if (options.FindOrphanedImages)
            {
                HandleFoundFiles(orphanedImages, FileType.Image, options.Delete, ConsoleColor.Cyan);
            }
            if (options.FindOrphanedTopics)
            {
                HandleFoundFiles(orphanedTopics, FileType.Markdown, options.Delete, ConsoleColor.Yellow);
            }
        }

        private static void WriteNonReferencedFileToOutput(FileToken token, Uri directory)
        {
            if (token.FileType != FileType.Yaml)
            {
                var relative = directory.MakeRelativeUri(new Uri(token.FilePath)).ToString();
                Console.WriteLine($"The \"{relative}\" file is not referenced anywhere.");
            }
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

        static void HandleFoundFiles(ISet<string> files, FileType type, bool delete, ConsoleColor color)
        {
            if (files.Any())
            {
                Console.WriteLine();
                WriteLine($"Found {files.Count:#,#} orphaned {type} files.", color);

                if (delete)
                {
                    foreach (var file in files.Where(File.Exists))
                    {
                        WriteLine($"Deleting: {file}.", ConsoleColor.Magenta);
                        File.Delete(file);
                    }
                }
            }
        }

        static void WriteLine(string message, ConsoleColor color)
        {
            var original = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }
    }
}
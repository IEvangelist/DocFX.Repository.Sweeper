using DocFX.Repository.Sweeper.Extensions;
using Kurukuru;
using ShellProgressBar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileTokenizer
    {
        public async Task<(TokenizationStatus, IDictionary<FileType, IList<FileToken>>)> TokenizeAsync(Options options)
        {
            var directoryLength = options.SourceDirectory.Length;
            var dir = FindDocFxRootDirectory(new DirectoryInfo(options.SourceDirectory));
            if (dir is null)
            {
                return (TokenizationStatus.Error, null);
            }

            var count = 0;
            Spinner.Start("Gathering files...", spinner =>
            {
                spinner.Color = ConsoleColor.Blue;
                count = 
                    dir.EnumerateDirectories()
                       .AsParallel()
                       .SelectMany(d => d.EnumerateFiles("*.*", SearchOption.AllDirectories))
                       .Count();
                spinner.Succeed();
            }, Patterns.Arc);
            var map = new ConcurrentDictionary<FileType, IList<FileToken>>();

            using (var progressBar = new ProgressBar(count, "Tokenizing files..."))
            {
                await dir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                         .ForEachAsync(
                    Environment.ProcessorCount,
                    async file =>
                    {
                        var fileToken = new FileToken(file);
                        await fileToken.InitializeAsync();

                        progressBar.Tick($"Tokenizing files...{ToRelativePath(fileToken.FilePath, directoryLength)}");
                        if (map.TryGetValue(fileToken.FileType, out var tokens))
                        {
                            tokens.Add(fileToken);
                        }
                        else
                        {
                            map[fileToken.FileType] = new List<FileToken> { fileToken };
                        }
                    });
                progressBar.Tick("Finished tokenizing files...");
            }

            return (TokenizationStatus.Success, map);
        }

        static DirectoryInfo FindDocFxRootDirectory(DirectoryInfo sourceDirectory)
        {
            try
            {
                while (sourceDirectory.GetFiles("docfx.json", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    sourceDirectory = sourceDirectory.Parent;
                    if (sourceDirectory == sourceDirectory?.Root)
                    {
                        Console.WriteLine("Could not find a directory containing docfx.json.");
                        return null;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Could not find directory {sourceDirectory.FullName}");
                return null;
            }

            return sourceDirectory;
        }

        static string ToRelativePath(string filePath, int directoryLength)
        {
            try
            {
                var fileLength = filePath.Length;
                return fileLength > directoryLength
                    ? filePath.Substring(directoryLength, filePath.Length - directoryLength)
                    : filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return filePath;
            }
        }
    }
}
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
        public async Task<(Status, IDictionary<FileType, IList<FileToken>>)> TokenizeAsync(Options options)
        {
            var directoryLength = options.SourceDirectory.Length;
            var dir = new DirectoryInfo(options.SourceDirectory).TraverseToFile("docfx.json");
            if (dir is null)
            {
                return (Status.Error, null);
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
                        await fileToken.InitializeAsync(options);

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

            return (Status.Success, map);
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
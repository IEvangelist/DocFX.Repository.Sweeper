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
            var dirUri = options.DirectoryUri;
            var dir =
                options.ExplicitScope
                    ? options.Directory
                    : new DirectoryInfo(options.SourceDirectory).TraverseToFile("docfx.json");

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

                        progressBar.Tick($"Tokenizing files...{dirUri.ToRelativePath(fileToken.FilePath)}");
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
    }
}
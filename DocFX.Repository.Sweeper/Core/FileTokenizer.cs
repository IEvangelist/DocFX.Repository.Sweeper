using Kurukuru;
using DocFX.Repository.Extensions;
using ShellProgressBar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFX.Repository.Sweeper.OpenPublishing;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileTokenizer
    {
        public async ValueTask<(Status, IDictionary<FileType, IList<FileToken>>)> TokenizeAsync(Options options, DocFxConfig config)
        {
            var dirUri = options.DirectoryUri;
            var dir = options.ExplicitScope ? options.Directory : options.DocFxJsonDirectory;
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

            var tokens = new ConcurrentBag<FileToken>();
            var destination = config.Build.Dest;

            using (var progressBar = new ProgressBar(count, "Tokenizing files..."))
            {
                var searchPattern = options.ReportFreshness ? "*.md" : "*.*";
                await dir.EnumerateFiles(searchPattern, SearchOption.AllDirectories)
                         .ForEachAsync(
                    Environment.ProcessorCount,
                    async fileInfo =>
                    {
                        FileToken fileToken = fileInfo;
                        await fileToken.InitializeAsync(options);

                        var relyingOnCache = options.EnableCaching ? " (relying on cache)" : string.Empty;
                        progressBar.Tick($"Materialzing file tokens{relyingOnCache}...{dirUri.ToRelativePath(fileToken.FilePath)}");

                        tokens.Add(fileToken);
                    });
                progressBar.Tick("Materialization complete...");
            }

            var cachedCount = FileTokenCacheUtility.CachedCount;
            if (cachedCount > 0)
            {
                ConsoleColor.Green.WriteLine($"Materialized {cachedCount:#,#} file tokens from the local cache rather than re-reading and parsing them.");
            }

            return (Status.Success, tokens.GroupBy(t => t.FileType).ToDictionary(grp => grp.Key, grp => grp.ToList() as IList<FileToken>));
        }
    }
}
using DocFX.Repository.Sweeper.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;
using Kurukuru;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            var directory = options.DirectoryUri;

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
                Spinner.Start($"Scoping \"{type}\" file workload.", spinner =>
                {
                    spinner.Color = ConsoleColor.Blue;
                    count = tokens.Count();
                    spinner.Succeed();
                }, Patterns.Arc);

                type.WriteLine($"Processing \"{type}\" files.");

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
                    progressBar.Tick($"- {count:#,#} \"{type}\" files processed in {typeStopwatch.Elapsed.ToHumanReadableString()}.");
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
                        await ApplyRedirectsAsync(files, options);
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

        static async Task ApplyRedirectsAsync(ISet<string> files, Options options)
        {
            var redirectConfig =
                await FindJsonFileAsync<RedirectConfig>(
                    ".openpublishing.redirection.json",
                    options.SourceDirectory);

            if (redirectConfig?.Redirections.Any() ?? false)
            {
                var docfx =
                    await FindJsonFileAsync<DocFxConfig>(
                        "docfx.json",
                        options.SourceDirectory);

                var dest = docfx?.Build?.Dest;
                var sourceDirectory = options.Directory.Parent;
                var redirectMap = new Dictionary<string, ISet<Redirect>>(StringComparer.OrdinalIgnoreCase);

                DirectoryInfo workingDirectory = null;

                foreach (var (directory, infos) in
                    files.OrderBy(path => path)
                         .Select(file => new FileInfo(file))
                         .GroupBy(info => info.DirectoryName)
                         .Select(grp => (grp.Key, grp)))
                {
                    if (workingDirectory is null ||
                        !string.Equals(workingDirectory.FullName, directory, StringComparison.OrdinalIgnoreCase))
                    {
                        workingDirectory = new DirectoryInfo(directory).TraverseToFile("toc.yml");
                    }

                    foreach (var info in infos)
                    {
                        var sourcePath = Path.GetRelativePath(sourceDirectory.FullName, info.FullName).Replace(@"\", "/");
                        var redirectUri = ToRedirectUrl(sourcePath, dest, Path.GetFileName(workingDirectory.FullName));
                        var redirect = new Redirect
                        {
                            SourcePath = sourcePath,
                            RedirectUrl = redirectUri
                        };

                        if (redirectMap.TryGetValue(workingDirectory.FullName, out var redirects))
                        {
                            redirects.Add(redirect);
                        }
                        else
                        {
                            redirectMap[workingDirectory.FullName] = new HashSet<Redirect> { redirect };
                        }
                    }
                }

                await SaveRedirectAsync(options, redirectConfig, redirectMap);
            }
        }

        static async Task SaveRedirectAsync(Options options, RedirectConfig redirectConfig, Dictionary<string, ISet<Redirect>> redirectMap)
        {
            try
            {
                redirectConfig.Redirections.AddRange(
                    redirectMap.SelectMany(kvp => kvp.Value));

                var dir = options.Directory.TraverseToFile("docfx.json");
                var json = redirectConfig.ToJson();
                var path = Path.Combine(dir.FullName, "docfx.json");

                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                ConsoleColor.DarkMagenta.WriteLine($"Unable to apply redirects. {ex.Message}");
            }
        }

        static Task<T> FindJsonFileAsync<T>(string filename, string directory)
            => FindFileAsync(filename, directory, json => json.FromJson<T>());

        static Task<T> FindYamlFileAsync<T>(string filename, string directory)
            => FindFileAsync(filename, directory, yaml => yaml.FromYaml<T>());

        static async Task<T> FindFileAsync<T>(string filename, string directory, Func<string, T> parse)
        {
            var dir = new DirectoryInfo(directory).TraverseToFile(filename);
            var filepath = Path.Combine(dir.FullName, filename);
            if (File.Exists(filepath))
            {
                var json = await File.ReadAllTextAsync(filepath);
                return parse(json);
            }

            return default;
        }

        static string ToRedirectUrl(string sourcePath, string dest, string index)
        {
            var segments = sourcePath.Split("/").Skip(1).Select(s => s.ToLower());
            var builder = new StringBuilder($"/{dest}");

            foreach (var segment in segments)
            {
                builder.Append($"/{segment}");
                if (string.Equals(segment, index, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            return builder.ToString();
        }
    }
}
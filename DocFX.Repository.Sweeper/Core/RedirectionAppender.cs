using DocFX.Repository.Sweeper.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;
using Kurukuru;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper.Core
{
    class RedirectionAppender
    {
        static HttpClient _httpClient;

        static readonly IDictionary<string, bool> _validRedirectUrlCache =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        internal async Task ApplyRedirectsAsync(IEnumerable<string> files, Options options)
        {
            var redirectConfig =
                await FindJsonFileAsync<RedirectConfig>(
                    ".openpublishing.redirection.json",
                    options.SourceDirectory);

            if (redirectConfig?.Redirections.Any() ?? false)
            {
                await Spinner.StartAsync("Validating redirect URLs.", async spinner =>
                {
                    spinner.Color = ConsoleColor.Blue;

                    var docfx =
                    await FindJsonFileAsync<DocFxConfig>(
                        "docfx.json",
                        options.SourceDirectory);

                    var dest = docfx?.Build?.Dest;
                    var sourceDirectory = options.Directory.Parent;
                    var redirectMap = new Dictionary<string, ISet<Redirect>>(StringComparer.OrdinalIgnoreCase);

                    foreach (var (directory, infos) in
                        files.Where(file => !string.IsNullOrWhiteSpace(file))
                             .OrderBy(path => path)
                             .Select(file => new FileInfo(file))
                             .GroupBy(info => info.DirectoryName)
                             .Select(grp => (grp.Key, grp)))
                    {
                        var dir = new DirectoryInfo(directory);
                        var workingDirectory = dir.TraverseToFile("toc.yml") ?? dir.TraverseToFile("index.yml");
                        if (workingDirectory is null)
                        {
                            if (options.OutputWarnings)
                            {
                                ConsoleColor.Blue.WriteLine(
                                    $"Unable to find a toc.yml or index.yml in the {directory} directory!");
                            }
                            continue;
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

                            var isValidRedirect = await IsValidRedirectPathAsync(options.HostUrl, redirectUri, options.QueryString);
                            if (!isValidRedirect)
                            {
                                continue;
                            }

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

                    spinner.Succeed();
                    await SaveRedirectAsync(options, redirectConfig, redirectMap);
                }, Patterns.Arc);
            }
        }

        static async Task<bool> IsValidRedirectPathAsync(string hostUrl, string redirectPath, string queryString)
        {
            var redirectUrl = $"{hostUrl}{redirectPath}{queryString}";
            if (_validRedirectUrlCache.TryGetValue(redirectUrl, out var isValid) && isValid)
            {
                return true;
            }

            try
            {
                _httpClient = _httpClient ?? new HttpClient();
                using (var response = await _httpClient.GetAsync(redirectUrl))
                {
                    var validUrl = response.StatusCode != HttpStatusCode.NotFound;
                    return _validRedirectUrlCache[redirectUrl] = validUrl;
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(ex.Message);
                    Debugger.Break();
                }

                return false;
            }
        }

        static async Task SaveRedirectAsync(Options options, RedirectConfig redirectConfig, Dictionary<string, ISet<Redirect>> redirectMap)
        {
            try
            {
                var startingCount = redirectConfig.Redirections.Count;
                redirectConfig.Redirections.AddRange(redirectMap.SelectMany(kvp => kvp.Value));
                var additions = redirectConfig.Redirections.Count - startingCount;
                var dir = options.Directory.TraverseToFile(".openpublishing.redirection.json");
                var json = redirectConfig.ToJson();
                var path = Path.Combine(dir.FullName, ".openpublishing.redirection.json");

                await File.WriteAllTextAsync(path, json);

                FileType.Markdown.WriteLine($"Automatically applied {additions:#,#} redirects to the \".openpublishing.redirection.json\" file.");
            }
            catch (Exception ex) when (options.OutputWarnings)
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
using DocFX.Repository.Sweeper.OpenPublishing;
using Kurukuru;
using System;
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

        internal async ValueTask ApplyRedirectsAsync(IEnumerable<string> files, Options options)
        {
            var redirectConfig = await options.GetRedirectConfigAsync();
            if (redirectConfig?.Redirections.Any() ?? false)
            {
                await Spinner.StartAsync("Validating redirect URLs.", async spinner =>
                {
                    spinner.Color = ConsoleColor.Blue;

                    var docfx = await options.GetConfigAsync();
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

        static async ValueTask<bool> IsValidRedirectPathAsync(string hostUrl, string redirectPath, string queryString)
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

        static async ValueTask SaveRedirectAsync(Options options, RedirectConfig redirectConfig, Dictionary<string, ISet<Redirect>> redirectMap)
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
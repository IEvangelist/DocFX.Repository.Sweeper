using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Environment;

namespace DocFX.Repository.Sweeper.Core
{
    static class FileTokenCacheUtility
    {
        static readonly string SweeperRoamingDir = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), "DocFx Sweeper");
        static readonly string CacheDir = Path.Combine(SweeperRoamingDir, "Cache");

        internal static int CachedCount = 0;

        internal static async ValueTask<bool> TryFindCachedVersionAsync(FileToken token, Options options, string destination)
        {
            try
            {
                if (!options.EnableCaching)
                {
                    return false;
                }

                var cachedTokenPath = await GetTokenCachePathAsync(token, options, destination);
                if (File.Exists(cachedTokenPath))
                {
                    token.MergeWith(cachedTokenPath.ReadFromProtoBufFile<FileToken>());
                    Interlocked.Increment(ref CachedCount);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        internal static async ValueTask CacheTokenAsync(FileToken token, Options options, string destination)
        {
            try
            {
                if (!options.EnableCaching)
                {
                    return;
                }

                token.WriteToProtoBufFile(await GetTokenCachePathAsync(token, options, destination));
            }
            catch
            {
            }
        }

        static async ValueTask<string> GetTokenCachePathAsync(FileToken token, Options options, string destination)
        {
            var dest =
                string.IsNullOrWhiteSpace(destination)
                    ? (await options.GetConfigAsync()).Build.Dest
                    : destination;

            var destDir = Path.Combine(CacheDir, dest);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            return Path.Combine(destDir, token.CachedJsonFileName);
        }

        internal static void PurgeCache()
        {
            try
            {
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                var dir = new DirectoryInfo(CacheDir);
                foreach (var file in
                    dir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                       .Where(file => File.Exists(file.FullName)))
                {
                    try
                    {
                        if (file.LastWriteTime < thirtyDaysAgo)
                        {
                            file.Delete();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }
    }
}
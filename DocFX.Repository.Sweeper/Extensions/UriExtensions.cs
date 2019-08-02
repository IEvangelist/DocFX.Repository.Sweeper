using System;

namespace DocFX.Repository.Sweeper
{
    static class UriExtensions
    {
        internal static string ToRelativePath(this Uri rootUri, string path) =>
            rootUri.MakeRelativeUri(new Uri(path)).ToString();
    }
}
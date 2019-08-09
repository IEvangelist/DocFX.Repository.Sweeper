using System;

namespace DocFX.Repository.Extensions
{
    public static class UriExtensions
    {
        public static string ToRelativePath(this Uri rootUri, string path) =>
            rootUri.MakeRelativeUri(new Uri(path)).ToString();
    }
}
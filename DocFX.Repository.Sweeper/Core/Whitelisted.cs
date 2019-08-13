using System.Collections;
using System.Collections.Generic;

namespace DocFX.Repository.Sweeper.Core
{
    static class Whitelisted
    {
        static internal IEnumerable<string> FileNames { get; } =
            new List<string>
            {
                "index", "readme", "license", "changelog", "issue_template",
            };

        static internal IEnumerable<string> DirectoryNames { get; } =
            new List<string>
            {
                "wwwroot", "sample", "snippet"
            };
    }
}
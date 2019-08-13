using System.Collections.Generic;

namespace DocFX.Repository.Sweeper.Core
{
    static class References
    {
        static internal IEnumerable<string> WhiteListedDirectoryNames { get; } =
            new List<string>
            {
                "wwwroot", "sample", "snippet"
            };
    }
}
using System;
using System.Collections.Generic;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileTypeUtils
    {
        internal static readonly IDictionary<FileType, FileTypeUtils> Utilities = new Dictionary<FileType, FileTypeUtils>
        {
            [FileType.Markdown] = new FileTypeUtils { Color = ConsoleColor.Cyan },
            [FileType.Image] = new FileTypeUtils { Color = ConsoleColor.Yellow },
            [FileType.Yaml] = new FileTypeUtils { Color = ConsoleColor.Magenta },
            [FileType.Json] = new FileTypeUtils { Color = ConsoleColor.DarkGreen },
            [FileType.NotRelevant] = new FileTypeUtils(),
        };

        internal ConsoleColor Color { get; private set; }
    }
}
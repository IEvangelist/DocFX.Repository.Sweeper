using DocFX.Repository.Sweeper.Core;
using DocFX.Repository.Sweeper.Extensions;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace DocFX.Repository.SweeperTests
{
    public class FileInfoExtensionsTests
    {
        public static readonly IEnumerable<object[]> FileTypeInput = new List<object[]>
        {
            new object[] { "file.png", FileType.Image },
            new object[] { "file.jpg", FileType.Image },
            new object[] { "file.jpeg", FileType.Image },
            new object[] { "file.gif", FileType.Image },
            new object[] { "file.svg", FileType.Image },

            new object[] { "file.yml", FileType.Yaml },
            new object[] { "file.yaml", FileType.Yaml },

            new object[] { "file.md", FileType.Markdown },
            new object[] { "file.markdown", FileType.Markdown },
            new object[] { "file.mdown", FileType.Markdown },
            new object[] { "file.mkd", FileType.Markdown },
            new object[] { "file.mdwn", FileType.Markdown },
            new object[] { "file.mdtxt", FileType.Markdown },
            new object[] { "file.mdtext", FileType.Markdown },
            new object[] { "file.rmd", FileType.Markdown },

            new object[] { "file.json", FileType.Json },

            new object[] { "file.dll", FileType.NotRelevant },
        };

        [
            Theory,
            MemberData(nameof(FileTypeInput))
        ]
        public void CorrectlyDeterminesFileType(string file, FileType type)
            => Assert.Equal(new FileInfo(file).GetFileType(), type);
    }
}
using DocFX.Repository.Sweeper.OpenPublishing;
using System;
using System.Collections.Generic;
using Xunit;

namespace DocFX.Repository.SweeperTests
{
    public class MetadataTests
    {
        public static readonly IEnumerable<object[]> MetadataParseInput = new List<object[]>
        {
            new object[] { new[] { " ---", "ms.date: 07/7/1984", "author:IEvangelist", "ms.author: dapine", "manager:nitinme", "--- " }, true, "IEvangelist", "dapine", "nitinme", new DateTime(1984, 7, 7) },
            new object[] { new[] { "--- ", "manager:nitinme", "ms.date: 07/7/1984", "author: IEvangelist", "ms.author:dapine", "  --- " }, true, "IEvangelist", "dapine", "nitinme", new DateTime(1984, 7, 7) },
            new object[] { new[] { " ---", "author:IEvangelist", "ms.author: dapine", "ms.date: 07/07/1984", "manager:nitinme" }, true, "IEvangelist", "dapine", "nitinme", new DateTime(1984, 7, 7) },
            new object[] { new[] { "author:IEvangelist", "ms.date: 7/7/1984", "manager: nitinme", "ms.author: dapine", "--- " }, true, "IEvangelist", "dapine", "nitinme", new DateTime(1984, 7, 7) },
            new object[] { new[] { "manager:nitinme", "author:IEvangelist", "ms.author: dapine", "ms.date: 7/7/84" }, true, "IEvangelist", "dapine", "nitinme", new DateTime(1984, 7, 7) },
            new object[] { new[] { "manager:nitinme", " ---", "author:IEvangelist", "---", "ms.author:dapine" }, false, null, null, null, null },
            new object[] { null, false, null, null, null, null }
        };

        [
            Theory,
            MemberData(nameof(MetadataParseInput))
        ]
        public void MetadataTryParsesCorrectly(
            string[] lines,
            bool expectedToBeParsed,
            string expectedGitHubAuthor,
            string expectedMircorosftAuthor,
            string expectedManager,
            DateTime? expectedDate)
        {
            var parsed = Metadata.TryParse(lines, out var metadata);

            Assert.Equal(expectedToBeParsed, parsed);
            if (expectedToBeParsed)
            {
                Assert.NotEqual(default, metadata);
                Assert.Equal(expectedGitHubAuthor, metadata.GitHubAuthor);
                Assert.Equal(expectedMircorosftAuthor, metadata.MicrosoftAuthor);
                Assert.Equal(expectedManager, metadata.Manager);
                Assert.Equal(expectedDate, metadata.Date);
            }
        }
    }
}
using DocFX.Repository.Sweeper.OpenPublishing;
using System.Collections.Generic;
using Xunit;

namespace DocFX.Repository.SweeperTests
{
    public class MetadataTests
    {
        public static readonly IEnumerable<object[]> MetadataParseInput = new List<object[]>
        {
            new object[] { new[] { " ---", "author:IEvangelist", "ms.author: dapine", "--- " }, true, "IEvangelist", "dapine" },
            new object[] { new[] { "--- ", "author: IEvangelist", "ms.author:dapine", "  --- " }, true, "IEvangelist", "dapine" },
            new object[] { new[] { " ---", "author:IEvangelist", "ms.author: dapine" }, true, "IEvangelist", "dapine" },
            new object[] { new[] { "author:IEvangelist", "ms.author: dapine", "--- " }, true, "IEvangelist", "dapine" },
            new object[] { new[] { "author:IEvangelist", "ms.author: dapine" }, true, "IEvangelist", "dapine" },
            new object[] { new[] { " ---", "author:IEvangelist", "---", "ms.author:dapine" }, false, null, null },
            new object[] { null, false, null, null }
        };

        [
            Theory,
            MemberData(nameof(MetadataParseInput))
        ]
        public void MetadataTryParsesCorrectly(
            string[] lines,
            bool expectedToBeParsed,
            string expectedGitHubAuthor,
            string expectedMircorosftAuthor)
        {
            var parsed = Metadata.TryParse(lines, out var metadata);

            Assert.Equal(expectedToBeParsed, parsed);
            if (expectedToBeParsed)
            {
                Assert.NotEqual(default, metadata);
                Assert.Equal(expectedGitHubAuthor, metadata.GitHubAuthor);
                Assert.Equal(expectedMircorosftAuthor, metadata.MicrosoftAuthor);
            }
        }
    }
}
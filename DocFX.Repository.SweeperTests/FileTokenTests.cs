using DocFX.Repository.Sweeper.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DocFX.Repository.SweeperTests
{
    public class FileTokenTests
    {
        [Fact]
        public async Task InitializesCorrectlyTest()
        {
            var dir = "docs-repo";
            var token = new FileToken(new FileInfo($"{dir}/all-possible-refs.md"));
            await token.InitializeAsync();

            var expectedTokens = new[]
            {
                new FileToken(new FileInfo($"{dir}/ref-one.md")),
                new FileToken(new FileInfo($"{dir}/ref-two.md")),
                new FileToken(new FileInfo($"{dir}/ref-(three).md")),
                new FileToken(new FileInfo($"{dir}/ref-four.md")),
                new FileToken(new FileInfo($"{dir}/ref-five.md")),
                new FileToken(new FileInfo($"{dir}/subdir/ref-six.md")),
                new FileToken(new FileInfo($"{dir}/ref-seven.md")),
                new FileToken(new FileInfo($"{dir}/image-one.svg")),
                new FileToken(new FileInfo($"{dir}/image-two.jpeg")),
                new FileToken(new FileInfo($"{dir}/image-three.jpg")),
                new FileToken(new FileInfo($"{dir}/image-four.png")),
                new FileToken(new FileInfo($"{dir}/media/context/image-five.png")),
                new FileToken(new FileInfo($"{dir}/media/tbl-image.png")),
                new FileToken(new FileInfo($"{dir}/media/serious.png")),
                new FileToken(new FileInfo($"{dir}/media/spaces.jpg")),
                new FileToken(new FileInfo($"{dir}/grandparent/media/csharp logo.png")),
            };

            Assert.NotNull(token.Header);
            Assert.Equal("dapine", token.Header.Value.MicrosoftAuthor);
            Assert.Equal("IEvangelist", token.Header.Value.GitHubAuthor);

            Assert.True(token.ContainsInvalidCodeFenceSlugs);
            Assert.Contains(new KeyValuePair<int, string>(33, "csharp"), token.CodeFenceSlugs);
            Assert.Contains(new KeyValuePair<int, string>(40, "c#"), token.CodeFenceSlugs);
            Assert.Contains(new KeyValuePair<int, string>(44, "js"), token.CodeFenceSlugs);
            Assert.Contains(new KeyValuePair<int, string>(52, "javascript"), token.CodeFenceSlugs);
            Assert.Equal(2, token.UnrecognizedCodeFenceSlugs.Count());

            Assert.Equal(expectedTokens.Where(t => t.FileType == FileType.Image).Count(), token.ImagesReferenced.Count);
            Assert.Equal(expectedTokens.Where(t => t.FileType == FileType.Markdown).Count(), token.TopicsReferenced.Count);
            Assert.Equal(expectedTokens.Length, token.TotalReferences);

            foreach (var otherToken in expectedTokens)
            {
                Assert.True(token.HasReferenceTo(otherToken));
            }
        }
    }
}
using DocFX.Repository.Sweeper;
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
        // Uncomment to do explicit testing of actual files.
        //[Fact]
        //public async Task TestExplicitFileAsync()
        //{
        //    var dir = @"C:\Repos\azure-docs-pr\articles\automation";
        //    var token = new FileToken(new FileInfo($"{dir}\\index.yml"));
        //    await token.InitializeAsync(new Options
        //    {
        //        SourceDirectory = dir
        //    });

        //    Assert.NotNull(token);
        //}

        [Fact]
        public async Task ReadsCorrectlyTest()
        {
            var dir = "docs-repo";
            FileToken originalToken = new FileInfo($"{dir}/all-possible-refs.md");
            await originalToken.InitializeAsync(new Options
            {
                SourceDirectory = dir
            });

            var json = originalToken.ToJson();
            await File.WriteAllTextAsync("temporary.json", json);
            var token = (await File.ReadAllTextAsync("temporary.json")).FromJson<FileToken>();

            AssertExpectations(dir, token);
        }

        [Fact]
        public async Task InitializesCorrectlyTest()
        {
            var dir = "docs-repo";
            FileToken token = new FileInfo($"{dir}/all-possible-refs.md");
            await token.InitializeAsync(new Options
            {
                SourceDirectory = dir
            });

            AssertExpectations(dir, token);
        }

        static void AssertExpectations(string dir, FileToken token)
        {
            var expectedTokens = new FileToken[]
            {
                new FileInfo($"{dir}/ref-one.md"),
                new FileInfo($"{dir}/ref-two.md"),
                new FileInfo($"{dir}/ref-(three).md"),
                new FileInfo($"{dir}/ref-four.md"),
                new FileInfo($"{dir}/ref-five.md"),
                new FileInfo($"{dir}/subdir/ref-six.md"),
                new FileInfo($"{dir}/ref-seven.md"),
                new FileInfo($"{dir}/image-one.svg"),
                new FileInfo($"{dir}/image-two.jpeg"),
                new FileInfo($"{dir}/image-three.jpg"),
                new FileInfo($"{dir}/image-four.png"),
                new FileInfo($"{dir}/media/context/image-five.png"),
                new FileInfo($"{dir}/media/tbl-image.png"),
                new FileInfo($"{dir}/media/more.jpg"),
                new FileInfo($"{dir}/media/serious.png"),
                new FileInfo($"{dir}/media/spaces.jpg"),
                new FileInfo($"{dir}/media/index/link.png"),
                new FileInfo($"{dir}/media/index/linkage.png"),
                new FileInfo($"{dir}/media/context/context/seriously-why.png"),
                new FileInfo($"{dir}/grandparent/media/csharp logo.png")
            };

            // Check header metadata
            Assert.NotNull(token.Header);
            Assert.Equal("dapine", token.Header.Value.MicrosoftAuthor);
            Assert.Equal("i-Evangelist", token.Header.Value.GitHubAuthor);

            // Check code fence slugs
            Assert.True(token.ContainsInvalidCodeFenceSlugs);
            Assert.Contains(new KeyValuePair<int, string>(33, "csharp"), token.CodeFenceSlugs);
            Assert.Contains(new KeyValuePair<int, string>(40, "c#"), token.CodeFenceSlugs); // Invalid code fence slug
            Assert.Contains(new KeyValuePair<int, string>(44, "js"), token.CodeFenceSlugs);
            Assert.Contains(new KeyValuePair<int, string>(52, "javascript"), token.CodeFenceSlugs);
            Assert.Single(token.UnrecognizedCodeFenceSlugs);

            // Check references to images and other markdown files
            foreach (var otherToken in expectedTokens)
            {
                Assert.True(token.HasReferenceTo(otherToken), $"{token} doesn't have a reference to {otherToken}");
            }

            Assert.Equal(expectedTokens.Where(t => t.FileType == FileType.Image).Count(), token.ImagesReferenced.Count);
            Assert.Equal(expectedTokens.Where(t => t.FileType == FileType.Markdown).Count(), token.TopicsReferenced.Count);
            Assert.Equal(expectedTokens.Length, token.TotalReferences);
        }
    }
}
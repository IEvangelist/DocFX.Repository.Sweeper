using DocFX.Repository.Sweeper.Core;
using System.IO;
using Xunit;

namespace DocFX.Repository.SweeperTests
{
    public class SweeperTests
    {
        [Fact]
        public void IsTokenReferencedAnywhereTest()
        {
            //RepoSweeper.IsTokenReferencedAnywhere
        }

        [
            Theory,
            InlineData(@"samples\content.md", false),
            InlineData(@"samples\image.png", true),
            InlineData(@"wwwroot\image.png", true),
            InlineData(@"snippets\image.png", true),
            InlineData(@"examples\image.png", false),
            InlineData(@"article\readme.md", true),
            InlineData(@"article\license", true),
            InlineData(@"article\issue_template.md", true),
            InlineData(@"article\index.md", true),
            InlineData(@"article\changelog.md", true)
        ]
        public void IsTokenWhiteListedTest(string filePath, bool expectedToBeWhitelisted)
        {
            FileToken token = new FileInfo(filePath);
            Assert.Equal(expectedToBeWhitelisted, RepoSweeper.IsTokenWhiteListed(token));
        }

        [Fact]
        public void IsTokenWithinScopedDirectoryTest()
        {
            //RepoSweeper.IsTokenWithinScopedDirectory
        }
    }
}
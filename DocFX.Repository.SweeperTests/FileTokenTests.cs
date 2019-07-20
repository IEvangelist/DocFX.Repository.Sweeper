using DocFX.Repository.Sweeper.Core;
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

            Assert.Equal(15, token.TotalReferences);
            Assert.Equal(8, token.ImagesReferenced.Count);
            Assert.Equal(7, token.TopicsReferenced.Count);

            foreach (var otherToken in
                new[]
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
                    new FileToken(new FileInfo($"{dir}/media/tbl-image.png")),
                    new FileToken(new FileInfo($"{dir}/media/serious.png")),
                    new FileToken(new FileInfo($"{dir}/media/spaces.jpg")),
                    new FileToken(new FileInfo($"{dir}/grandparent/media/csharp logo.png")),
                })
            {
                Assert.True(token.HasReferenceTo(otherToken));
            }
        }

        [Fact]
        public void RegexTest()
        {
            var test =
                @"		<td valign=""top"" width=""50 % "">2.<br/><a src=""./ref-seven.md"" width=""85 % "" alt-text=""Shows the Microsoft sign-in page""/><ul><li>User is redirected to Microsoft Sign-in page</li><li>User provides credentials to sign in</li></ul></td>
";

            Assert.Contains(
                FileTypeUtils.MapExpressions(FileType.Markdown)
                             .SelectMany(regex => regex.Matches(test)),
                match =>
                match?.Groups.Any(grp => 
                                  grp.Name == "link" && grp.Value == "./ref-seven.md") ?? false);
        }
    }
}
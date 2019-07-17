using DocFX.Repository.Sweeper.Core;
using System.IO;
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

            Assert.Equal(9, token.TotalReferences);
            Assert.Equal(5, token.ImagesReferenced.Count);
            Assert.Equal(4, token.TopicsReferenced.Count);

            foreach (var otherToken in
                new[]
                {
                    new FileToken(new FileInfo($"{dir}/ref-one.md")),
                    new FileToken(new FileInfo($"{dir}/ref-two.md")),
                    new FileToken(new FileInfo($"{dir}/ref-three.md")),
                    new FileToken(new FileInfo($"{dir}/ref-four.md")),
                    new FileToken(new FileInfo($"{dir}/image-one.svg")),
                    new FileToken(new FileInfo($"{dir}/image-two.jpeg")),
                    new FileToken(new FileInfo($"{dir}/image-three.jpg")),
                    new FileToken(new FileInfo($"{dir}/image-four.png")),
                    new FileToken(new FileInfo($"{dir}/grandparent/media/csharp-logo.png")),
                })
            {
                Assert.True(token.HasReferenceTo(otherToken));
            }
        }
    }
}
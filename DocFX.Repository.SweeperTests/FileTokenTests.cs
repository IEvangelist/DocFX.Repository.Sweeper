﻿using DocFX.Repository.Sweeper.Core;
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
    }
}
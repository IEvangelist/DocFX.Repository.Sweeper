using DocFX.Repository.Sweeper.Core;
using System.Linq;
using Xunit;

namespace DocFX.Repository.SweeperTests
{
    public class TokenExpressionTests
    {
        [
            Theory,
            InlineData("```csharp", "csharp"),
            InlineData("````powershell", "powershell")
        ]
        public void CodeFenceTest(string input, string expected) =>
            Assert.Equal(expected, TokenExpressions.CodeFenceRegex.Match(input)?.Groups["slug"].Value);

        [
            Theory,
            InlineData("[ref](./../subdir/ref.md?toc=%2fcli%2fmodule%2ftoc.json)", "./../subdir/ref.md?toc=%2fcli%2fmodule%2ftoc.json"),
            InlineData("[ref](subdir/ref.md)", "subdir/ref.md")
        ]
        public void MarkdownLinkTest(string input, string expected) =>
            Assert.Equal(expected, TokenExpressions.MarkdownLinkRegex.Match(input)?.Groups["link"].Value);

        [
            Theory,
            InlineData("![seriously]( ./media/serious.png )", " ./media/serious.png"),
            InlineData("![...](./media/context/image-five.png", "./media/context/image-five.png"),
            InlineData("	![Select Menu, then System, and then Administration](./media/more.jpg)", "./media/more.jpg"),
            InlineData("![Ugh, so mad at people for doing this](https://docs.microsoft.com/azure/media/index/link.png)", "https://docs.microsoft.com/azure/media/index/link.png"),
            InlineData("![This is pretty lame too](/azure/media/index/linkage.png)", "/azure/media/index/linkage.png"),
            InlineData("![but why](media/context/context/seriously-why.png)", "media/context/context/seriously-why.png"),
            InlineData("![image four][1]", "")
        ]
        public void MarkdownImageLinkTest(string input, string expected) =>
            Assert.Equal(expected, TokenExpressions.MarkdownLinkRegex.Match(input)?.Groups["link"].Value);

        [
            Theory,
            InlineData(
                "[![two](../docs-repo/image-two.jpeg)](./image-three.jpg#lightbox)",
                "../docs-repo/image-two.jpeg",
                "./image-three.jpg#lightbox")
        ]
        public void MarkdownLightboxImageLinkTest(string input, string expectedOne, string expectedTwo)
        {
            var matches = TokenExpressions.MarkdownLightboxImageLinkRegex.Matches(input);
            Assert.Contains(matches, match => match.Groups.Any(grp => grp.Value == expectedOne));
            Assert.Contains(matches, match => match.Groups.Any(grp => grp.Value == expectedTwo));
        }

        [
            Theory,
            InlineData("[!INCLUDE [ref-one](ref-one.md)]", "ref-one.md")
        ]
        public void MarkdownIncludeLinkTest(string input, string expected) =>
            Assert.Equal(expected, TokenExpressions.MarkdownIncludeLinkRegex.Match(input)?.Groups["link"].Value);

        [
            Theory,
            InlineData("[!INCLUDE [ref-three](~/docs-repo/ref-(three).md)]", "~/docs-repo/ref-(three).md")
        ]
        public void MarkdownNestedParathesesTest(string input, string expected) =>
            Assert.Equal(expected, TokenExpressions.MarkdownNestedParathesesRegex.Match(input)?.Groups["link"].Value);
    }
}
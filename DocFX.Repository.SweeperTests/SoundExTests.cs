using DocFX.Repository.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DocFX.Repository.SweeperTests
{
    public class SoundExTests
    {
        public static readonly IEnumerable<object[]> SoundExInput = new List<object[]>
        {
            new object[] { "Soundex", "S532" },
            new object[] { "Example", "E251" },
            new object[] { "Sownteks", "S532" },
            new object[] { "Ekzampul", "E251" },
            new object[] { "Euler", "E460" },
            new object[] { "Gauss", "G200" },
            new object[] { "Hilbert", "H416" },
            new object[] { "Knuth", "K530" },
            new object[] { "Lloyd", "L300" },
            new object[] { "Lukasiewicz", "L222" },
            new object[] { "Ellery", "E460" },
            new object[] { "Ghosh", "G200" },
            new object[] { "Heilbronn", "H416" },
            new object[] { "Kant", "K530" },
            new object[] { "Ladd", "L300" },
            new object[] { "Lissajous", "L222" },
            new object[] { "Wheaton", "W350" },
            new object[] { "Burroughs", "B620" },
            new object[] { "Burrows", "B620" },
            new object[] { "O'Hara", "O600" },
            new object[] { "Washington", "W252" },
            new object[] { "Lee", "L000" },
            new object[] { "Gutierrez", "G362" },
            new object[] { "Pfister", "P236" },
            new object[] { "Jackson", "J250" },
            new object[] { "Tymczak", "T522" },
            new object[] { "VanDeusen", "V532" },
            new object[] { "Ashcraft", "A261" }
        };

        [
            Theory,
            MemberData(nameof(SoundExInput))
        ]
        public void SoundExTest(string input, string expected)
        {
            var actual = SoundEx.Encode(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DifferenceTest() => Assert.Equal(4, SoundEx.Difference("fereri", "Ferrari"));

        [
            Theory,
            InlineData("powerhshell", "powershell, powershell-interactive, prolog", false),
            InlineData("JScript.NET", "", false),
            InlineData("JScript.NET", "craftcms, fortran, javascript, protobuf, thrift", true)
        ]
        public void FindPossibleIntendedAlternativesTest(string input, string expected, bool kindOfSoundsLike)
            => Assert.Equal(
                expected, 
                string.Join(
                    ", ",
                    Taxonomies.UniqueMonikers
                              .Where(moniker => kindOfSoundsLike ? moniker.SoundsKindOfLike(input) : moniker.SoundsLike(input))
                              .OrderBy(m => m)));
    }
}
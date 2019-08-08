using System.Linq;
using static System.String;

namespace DocFX.Repository.Extensions
{
    // Inspired by: https://www.rosettacode.org/wiki/Soundex#C.23
    public static class SoundEx
    {
        const string SoundExAlphabet = "0123012#02245501262301#202";

        public static string Encode(string word)
        {
            var soundexString = Empty;
            var lastSoundexChar = '?';
            word = word.ToUpper();

            foreach (var c in from ch in word
                              where ch >= 'A' &&
                                    ch <= 'Z' &&
                                    soundexString.Length < 4
                              select ch)
            {
                var thisSoundexChar = SoundExAlphabet[c - 'A'];
                if (soundexString.Length == 0)
                {
                    soundexString += c;
                }
                else if (thisSoundexChar == '#')
                {
                    continue;
                }
                else if (thisSoundexChar != '0' &&
                         thisSoundexChar != lastSoundexChar)
                {
                    soundexString += thisSoundexChar;
                }

                lastSoundexChar = thisSoundexChar;
            }

            return soundexString.PadRight(4, '0');
        }

        public static int Difference(string left, string right)
        {
            var result = 0;
            if (left.Equals(string.Empty) || right.Equals(string.Empty))
            {
                return result;
            }

            var soundex1 = Encode(left);
            var soundex2 = Encode(right);
            if (soundex1.Equals(soundex2))
            {
                result = 4;
            }
            else
            {
                if (soundex1[0] == soundex2[0])
                {
                    result = 1;
                }

                var sub1 = soundex1.Substring(1, 3); //characters 2, 3 and 4
                if (soundex2.IndexOf(sub1) > -1)
                {
                    result += 3;
                    return result;
                }
                var sub2 = soundex1.Substring(2, 2); //characters 3 and 4
                if (soundex2.IndexOf(sub2) > -1)
                {
                    result += 2;
                    return result;
                }
                var sub3 = soundex1.Substring(1, 2); //characters 2 and 3
                if (soundex2.IndexOf(sub3) > -1)
                {
                    result += 2;
                    return result;
                }

                var sub4 = soundex1[1];
                if (soundex2.IndexOf(sub4) > -1)
                {
                    ++ result;
                }

                var sub5 = soundex1[2];
                if (soundex2.IndexOf(sub5) > -1)
                {
                    ++ result;
                }

                var sub6 = soundex1[3];
                if (soundex2.IndexOf(sub6) > -1)
                {
                    ++ result;
                }
            }

            return result;
        }
    }
}
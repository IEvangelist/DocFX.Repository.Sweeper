using System;

namespace DocFX.Repository.Extensions
{
    // Borrowed from: https://stackoverflow.com/a/4975942/2410379
    public static class LongExtensions
    {
        static readonly string[] Suffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        public static string FromBytesToString(this long value)
        {
            if (value == 0)
            {
                return $"0 {Suffix[0]}";
            }

            var bytes = Math.Abs(value);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);

            return $"{Math.Sign(value) * num} {Suffix[place]}";
        }
    }
}
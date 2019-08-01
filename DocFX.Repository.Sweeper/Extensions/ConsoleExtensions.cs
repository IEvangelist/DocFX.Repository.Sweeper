using DocFX.Repository.Sweeper.Core;
using System;

namespace DocFX.Repository.Sweeper
{
    static class ConsoleExtensions
    {
        internal static void WriteLine(this ConsoleColor color, string message)
            => WriteLine(message, color);

        internal static void WriteLine(this Status status, string message)
            => WriteLine(message, status == Status.Success ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed);

        internal static void WriteLine(this FileType fileType, string message)
            => WriteLine(message, FileTypeUtils.Utilities[fileType].Color);

        static void WriteLine(string message, ConsoleColor color)
        {
            var original = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }
    }
}
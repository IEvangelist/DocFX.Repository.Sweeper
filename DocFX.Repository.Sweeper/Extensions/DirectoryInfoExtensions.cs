using System;
using System.IO;

namespace DocFX.Repository.Sweeper.Extensions
{
    static class DirectoryInfoExtensions
    {
        internal static DirectoryInfo TraverseToFile(this DirectoryInfo directory, string filename)
        {
            try
            {
                while (directory.GetFiles(filename, SearchOption.TopDirectoryOnly).Length == 0)
                {
                    directory = directory.Parent;
                    if (directory == directory?.Root)
                    {
                        Console.WriteLine($"Could not find a directory containing {filename}.");
                        return null;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Could not find directory {directory.FullName}");
                return null;
            }

            return directory;
        }
    }
}
﻿using System.IO;

namespace DocFX.Repository.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo TraverseToFile(this DirectoryInfo directory, string filename)
        {
            try
            {
                while (directory.GetFiles(filename, SearchOption.TopDirectoryOnly).Length == 0)
                {
                    directory = directory.Parent;
                    if (directory == directory?.Root)
                    {
                        return null;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }

            return directory;
        }
    }
}
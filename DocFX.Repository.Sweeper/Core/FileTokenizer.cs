using DocFX.Repository.Sweeper.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileTokenizer
    {
        public async Task<(TokenizationStatus, IDictionary<FileType, IList<FileToken>>)> TokenizeAsync(Options options)
        {
            var dir = FindDocFxRootDirectory(new DirectoryInfo(options.SourceDirectory));
            if (dir is null)
            {
                return (TokenizationStatus.Error, null);
            }

            var map = new ConcurrentDictionary<FileType, IList<FileToken>>();

            await dir.EnumerateFiles("*.*", SearchOption.AllDirectories)
                     .ForEachAsync(
                         Environment.ProcessorCount,
                         async file =>
                         {
                             var fileToken = new FileToken(file);
                             await fileToken.InitializeAsync();

                             if (map.TryGetValue(fileToken.FileType, out var tokens))
                             {
                                 tokens.Add(fileToken);
                             }
                             else
                             {
                                 map[fileToken.FileType] = new List<FileToken> { fileToken };
                             }
                         });

            return (TokenizationStatus.Success, map);
        }

        static DirectoryInfo FindDocFxRootDirectory(DirectoryInfo sourceDirectory)
        {
            try
            {
                while (sourceDirectory.GetFiles("docfx.json", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    sourceDirectory = sourceDirectory.Parent;
                    if (sourceDirectory == sourceDirectory?.Root)
                    {
                        Console.WriteLine("Could not find a directory containing docfx.json.");
                        return null;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Could not find directory {sourceDirectory.FullName}");
                return null;
            }

            return sourceDirectory;
        }
    }
}
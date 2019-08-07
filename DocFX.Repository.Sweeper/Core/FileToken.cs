using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocFX.Repository.Sweeper.OpenPublishing;
using Newtonsoft.Json;

namespace DocFX.Repository.Sweeper.Core
{
    public class FileToken
    {
        IDictionary<int, string> _codeFenceSlugs;

        public Metadata? Header { get; internal set; }

        public string DirectoryName { get; internal set; }

        public string FilePath { get; internal set; }

        public FileType FileType { get; internal set; }

        public ISet<string> TopicsReferenced { get; } = new HashSet<string>();

        public ISet<string> ImagesReferenced { get; } = new HashSet<string>();

        public IDictionary<int, string> CodeFenceSlugs => _codeFenceSlugs ?? (_codeFenceSlugs = new Dictionary<int, string>());

        public int TotalReferences => TopicsReferenced.Count + ImagesReferenced.Count;

        public bool IsRelevant => FileType != FileType.NotRelevant && FileType != FileType.Json;

        public bool IsMarkedForDeletion { get; internal set; }

        [JsonIgnore]
        public IEnumerable<(int, string)> UnrecognizedCodeFenceSlugs
            => _codeFenceSlugs is null
                ? Enumerable.Empty<(int, string)>()
                : _codeFenceSlugs.Where(kvp => !Taxonomies.UniqueMonikers.Contains(kvp.Value))
                                 .Select(kvp => (kvp.Key, kvp.Value));

        [JsonIgnore]
        public bool ContainsInvalidCodeFenceSlugs => UnrecognizedCodeFenceSlugs.Any();

        public static implicit operator FileToken(FileInfo fileInfo) =>
            new FileToken
            {
                DirectoryName = fileInfo.DirectoryName,
                FilePath = fileInfo.FullName,
                FileType = fileInfo.GetFileType()
            };

        public override string ToString()
        {
            var type = FileType;
            switch (type)
            {
                case FileType.Markdown:
                case FileType.Yaml:
                    return $"{type} File: {new FileInfo(FilePath).Name}, references {TopicsReferenced.Count} other files and {ImagesReferenced.Count} images.";

                case FileType.Json:
                case FileType.Image:
                    return FilePath;

                default:
                    return "For all intents and purposes, this is a meaningless file.";
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocFX.Repository.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;
using Newtonsoft.Json;
using ProtoBuf;

namespace DocFX.Repository.Sweeper.Core
{
    [ProtoContract]
    public class FileToken
    {
        IDictionary<int, string> _codeFenceSlugs;

        [JsonProperty("h"), ProtoMember(1)]
        public Metadata Header { get; set; }

        [JsonProperty("d"), ProtoMember(2)]
        public string DirectoryName { get; internal set; }

        [JsonProperty("fp"), ProtoMember(3)]
        public string FilePath { get; internal set; }

        [JsonProperty("ft"), ProtoMember(4)]
        public FileType FileType { get; internal set; }

        [JsonProperty("lwt"), ProtoMember(5)]
        public DateTime LastWriteTime { get; internal set; }

        [JsonProperty("t"), ProtoMember(6)]
        public ISet<string> TopicsReferenced { get; } = new HashSet<string>();

        [JsonProperty("i"), ProtoMember(7)]
        public ISet<string> ImagesReferenced { get; } = new HashSet<string>();

        [JsonProperty("c"), ProtoMember(8)]
        public IDictionary<int, string> CodeFenceSlugs => _codeFenceSlugs ?? (_codeFenceSlugs = new Dictionary<int, string>());

        [JsonProperty("s"), ProtoMember(9)]
        public long FileSizeInBytes { get; internal set; }

        [JsonIgnore]
        public int TotalReferences => TopicsReferenced.Count + ImagesReferenced.Count;

        [JsonIgnore]
        public bool IsRelevant => FileType != FileType.NotRelevant && FileType != FileType.Json;

        [JsonIgnore]
        public bool IsMarkedForDeletion { get; internal set; }

        [JsonIgnore]
        public IEnumerable<(int, string)> UnrecognizedCodeFenceSlugs
            => _codeFenceSlugs is null
                ? Enumerable.Empty<(int, string)>()
                : _codeFenceSlugs.Where(kvp => !Taxonomies.UniqueMonikers.Contains(kvp.Value) && !kvp.Value.Contains("```"))
                                 .Select(kvp => (kvp.Key, kvp.Value));

        [JsonIgnore]
        public bool ContainsInvalidCodeFenceSlugs => UnrecognizedCodeFenceSlugs.Any();

        public static implicit operator FileToken(FileInfo fileInfo) =>
            new FileToken
            {
                LastWriteTime = fileInfo.LastWriteTime,
                DirectoryName = fileInfo.DirectoryName,
                FilePath = fileInfo.FullName,
                FileType = fileInfo.GetFileType(),
                FileSizeInBytes = fileInfo.Length
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

        internal void MergeWith(FileToken fileToken)
        {
            if (fileToken is null)
            {
                return;
            }

            _codeFenceSlugs = fileToken.CodeFenceSlugs;
            DirectoryName = fileToken.DirectoryName;
            FilePath = fileToken.FilePath;
            FileType = fileToken.FileType;
            Header = fileToken.Header;
            LastWriteTime = fileToken.LastWriteTime;
            FileSizeInBytes = fileToken.FileSizeInBytes;
            ImagesReferenced.UnionWith(fileToken.ImagesReferenced);
            TopicsReferenced.UnionWith(fileToken.TopicsReferenced);
        }

        [JsonIgnore]
        internal string CachedJsonFileName => 
            $"cache.{FilePath.GetDeterministicHashCode()}.{LastWriteTime.ToString().GetDeterministicHashCode()}.bin";
    }
}
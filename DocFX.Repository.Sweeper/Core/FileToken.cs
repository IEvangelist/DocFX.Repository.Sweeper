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

        [ProtoMember(1)]
        public Metadata Header { get; set; }

        [ProtoMember(2)]
        public string DirectoryName { get; internal set; }

        [ProtoMember(3)]
        public string FilePath { get; internal set; }

        [ProtoMember(4)]
        public FileType FileType { get; internal set; }

        [ProtoMember(5)]
        public DateTime LastWriteTime { get; internal set; }

        [ProtoMember(6)]
        public ISet<string> TopicsReferenced { get; } = new HashSet<string>();

        [ProtoMember(7)]
        public ISet<string> ImagesReferenced { get; } = new HashSet<string>();

        [ProtoMember(8)]
        public IDictionary<int, string> CodeFenceSlugs => _codeFenceSlugs ?? (_codeFenceSlugs = new Dictionary<int, string>());

        [ProtoMember(9)]
        public long FileSizeInBytes { get; internal set; }

        [ProtoMember(10)]
        public ISet<string> Xrefs { get; } = new HashSet<string>();

        public int TotalReferences => TopicsReferenced.Count + ImagesReferenced.Count;

        public bool IsRelevant => FileType != FileType.NotRelevant && FileType != FileType.Json;

        public bool IsMarkedForDeletion { get; internal set; }

        public IEnumerable<(int, string)> UnrecognizedCodeFenceSlugs
            => _codeFenceSlugs is null
                ? Enumerable.Empty<(int, string)>()
                : _codeFenceSlugs.Where(kvp => !Taxonomies.UniqueMonikers.Contains(kvp.Value) && !kvp.Value.Contains("```"))
                                 .Select(kvp => (kvp.Key, kvp.Value));

        public bool ContainsInvalidCodeFenceSlugs => UnrecognizedCodeFenceSlugs.Any();

        public static implicit operator FileToken(FileInfo fileInfo) =>
            new FileToken
            {
                LastWriteTime = fileInfo.LastWriteTime,
                DirectoryName = fileInfo.DirectoryName,
                FilePath = fileInfo.FullName,
                FileType = fileInfo.GetFileType(),
                FileSizeInBytes = fileInfo.Exists ? fileInfo.Length : 0
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
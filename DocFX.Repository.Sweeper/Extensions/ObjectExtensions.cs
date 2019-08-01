using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization;
using static Newtonsoft.Json.JsonConvert;

namespace DocFX.Repository.Sweeper
{
    public static class ObjectExtensions
    {
        static readonly DefaultContractResolver ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = ContractResolver,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static T FromJson<T>(this string json, JsonSerializerSettings serializerSettings = null)
            => string.IsNullOrWhiteSpace(json) ? default : DeserializeObject<T>(json, serializerSettings ?? DefaultSettings);

        public static string ToJson(this object value, JsonSerializerSettings serializerSettings = null)
            => value is null ? null : SerializeObject(value, serializerSettings ?? DefaultSettings);

        static readonly IDeserializer Deserializer
            = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

        public static T FromYaml<T>(this string yaml)
            => string.IsNullOrWhiteSpace(yaml) ? default : Deserializer.Deserialize<T>(yaml);
    }
}
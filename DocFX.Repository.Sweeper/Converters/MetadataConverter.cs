using DocFX.Repository.Sweeper.OpenPublishing;
using Newtonsoft.Json;
using System;

namespace DocFX.Repository.Sweeper.Converters
{
    public class MetadataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) 
            => objectType == typeof(Metadata) || objectType == typeof(Metadata?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string gitHubAuthor = null;
            string microsoftAuthor = null;
            string manager = null;
            DateTime? date = null;

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    break;
                }

                var propertyName = (string)reader.Value;
                if (!reader.Read())
                {
                    continue;
                }

                if (propertyName == nameof(gitHubAuthor))
                {
                    gitHubAuthor = serializer.Deserialize<string>(reader);
                }
                if (propertyName == nameof(microsoftAuthor))
                {
                    microsoftAuthor = serializer.Deserialize<string>(reader);
                }
                if (propertyName == nameof(manager))
                {
                    manager = serializer.Deserialize<string>(reader);
                }
                if (propertyName == nameof(date))
                {
                    date = serializer.Deserialize<DateTime?>(reader);
                }
            }

            return new Metadata
            {
                GitHubAuthor = gitHubAuthor,
                MicrosoftAuthor = microsoftAuthor,
                Manager = manager,
                Date = date
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Metadata metadata)
            {
                writer.WriteStartObject();
                if (!string.IsNullOrWhiteSpace(metadata.GitHubAuthor))
                {
                    writer.WritePropertyName("gitHubAuthor");
                    serializer.Serialize(writer, metadata.GitHubAuthor);
                }
                if (!string.IsNullOrWhiteSpace(metadata.MicrosoftAuthor))
                {
                    writer.WritePropertyName("microsoftAuthor");
                    serializer.Serialize(writer, metadata.MicrosoftAuthor);
                }
                if (!string.IsNullOrWhiteSpace(metadata.Manager))
                {
                    writer.WritePropertyName("manager");
                    serializer.Serialize(writer, metadata.Manager);
                }
                if (metadata.Date.HasValue)
                {
                    writer.WritePropertyName("date");
                    serializer.Serialize(writer, metadata.Date.Value);
                }
                writer.WriteEndObject();
            }
        }
    }
}
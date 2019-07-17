using static Newtonsoft.Json.JsonConvert;

namespace DocFX.Repository.Sweeper.Extensions
{
    public static class ObjectExtensions
    {
        public static T FromJson<T>(this string json) => string.IsNullOrWhiteSpace(json) ? default : DeserializeObject<T>(json);

        public static string ToJson(this object value) => value is null ? null : SerializeObject(value);
    }
}
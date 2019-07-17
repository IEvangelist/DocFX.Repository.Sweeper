using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class Redirect
    {
        [JsonProperty(PropertyName = "source_path")]
        public string SourcePath { get; set; }

        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "redirect_document_id")]
        public bool RedirectDocumentId { get; set; }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class RedirectConfig
    {
        [JsonProperty(PropertyName = "redirections")]
        public List<Redirect> Redirections { get; set; }
    }
}
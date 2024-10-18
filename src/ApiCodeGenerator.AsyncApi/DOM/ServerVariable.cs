using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class ServerVariable : RefObject<ServerVariable>
    {
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("enum")]
        public ICollection<string>? Enum { get; set; }

        [JsonProperty("default")]
        public string? Default { get; set; }

        [JsonProperty("examples")]
        public ICollection<string>? Examples { get; set; }
    }
}

using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class Server : RefObject<Server>
    {
        [JsonProperty("url")]
        public required string Url { get; set; }

        [JsonProperty("protocol")]
        public required string Protocol { get; set; }

        [JsonProperty("protocolVersion")]
        public string? ProtocolVersion { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("variables")]
        public IDictionary<string, ServerVariable>? Variables { get; set; }

        [JsonProperty("security")]
        public ICollection<SecurityRequirement>? Security { get; set; }

        [JsonProperty("tags")]
        public ICollection<Tag>? Tags { get; set; }

        [JsonProperty("bindings")]
        public ServerBindings? Bindings { get; set; }
    }
}

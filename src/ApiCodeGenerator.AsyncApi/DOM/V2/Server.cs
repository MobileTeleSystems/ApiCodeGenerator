using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal class Server
{
    [JsonProperty("url", Required = Required.Always)]
    public required string Url { get; set; }

    [JsonProperty("protocol", Required = Required.Always)]
    public required string Protocol { get; set; }

    [JsonProperty("protocolVersion")]
    public string? ProtocolVersion { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("variables")]
    public IDictionary<string, Reference<ServerVariable>>? Variables { get; set; }

    [JsonProperty("security")]
    public SecurityRequirement[]? Security { get; set; }

    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    [JsonProperty("bindings")]
    public Reference<ServerBindings>? Bindings { get; set; }
}

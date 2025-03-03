using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Server : RefObject
{
    [JsonProperty("host", Required = Required.Always)]
    public required string Host { get; set; }

    [JsonProperty("protocol", Required = Required.Always)]
    public required string Protocol { get; set; }

    [JsonProperty("pathname")]
    public string? PathName { get; set; }

    [JsonProperty("protocolVersion")]
    public string? ProtocolVersion { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("variables")]
    public IDictionary<string, Reference<ServerVariable>>? Variables { get; set; }

    [JsonProperty("security")]
    public ICollection<Reference<Security.SecurityScheme>>? Security { get; set; }

    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }

    [JsonProperty("bindings")]
    public Reference<ServerBindings>? Bindings { get; set; }
}

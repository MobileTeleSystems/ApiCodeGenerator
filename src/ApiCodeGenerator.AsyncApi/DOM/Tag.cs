using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Tag : ExtensionRefObject
{
    [JsonProperty("name", Required = Required.Always)]
    public required string Name { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }
}

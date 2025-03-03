using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class CorrelationId : ExtensionRefObject
{
    /// <summary>An optional description of the identifier.</summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>A runtime expression that specifies the location of the correlation ID.</summary>
    [JsonProperty("location", Required = Required.Always)]
    public required string Location { get; set; }
}

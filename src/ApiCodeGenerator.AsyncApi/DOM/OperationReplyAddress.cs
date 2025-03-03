using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class OperationReplyAddress : ExtensionRefObject
{
    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("location", Required = Required.Always)]
    public required string Location { get; set; }
}

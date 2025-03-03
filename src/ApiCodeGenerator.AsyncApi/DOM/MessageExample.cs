using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class MessageExample : JsonExtensionObject
{
    [JsonProperty("headers")]
    public IDictionary<string, object>? Headers { get; set; }

    [JsonProperty("payload")]
    public IDictionary<string, object>? Payload { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }
}

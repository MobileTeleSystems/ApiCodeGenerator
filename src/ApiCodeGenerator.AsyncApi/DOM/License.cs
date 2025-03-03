using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class License : JsonExtensionObject
{
    /// <summary>Gets or sets the name.</summary>
    [JsonProperty(PropertyName = "name", Required = Required.Always)]
    public required string Name { get; set; }

    /// <summary>Gets or sets the license URL.</summary>
    [JsonProperty(PropertyName = "url")]
    public string? Url { get; set; }
}

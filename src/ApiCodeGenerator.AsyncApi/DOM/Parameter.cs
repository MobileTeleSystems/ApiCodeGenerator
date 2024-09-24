using System.Text.Json.Serialization;
using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Parameter : RefObject<Parameter>
{
    [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string? Description { get; set; }

    [JsonProperty("schema", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public JsonSchema Schema { get; set; } = default!;
}

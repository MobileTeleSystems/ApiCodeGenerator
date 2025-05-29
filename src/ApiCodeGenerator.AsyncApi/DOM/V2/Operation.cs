using ApiCodeGenerator.AsyncApi.DOM.Traits;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OperationV3 = ApiCodeGenerator.AsyncApi.DOM.Operation;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal class Operation
{
    [JsonProperty("operationId")]
    public string? OperationId { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }

    [JsonProperty("description")]
    public string? Desciption { get; set; }

    [JsonProperty("security")]
    public SecurityRequirement[]? Security { get; set; }

    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }

    [JsonProperty("bindings")]
    public Reference<OperationBindings>? Bindings { get; set; }

    [JsonProperty("traits")]
    public ICollection<Reference<OperationTraits>>? Traits { get; set; }

    [JsonProperty("message")]
    public JObject? Message { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object?>? ExtensionData { get; set; }
}

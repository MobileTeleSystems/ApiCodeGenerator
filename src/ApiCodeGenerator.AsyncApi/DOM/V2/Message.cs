using ApiCodeGenerator.AsyncApi.DOM.Traits;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal class Message : RefObject
{
    [JsonProperty("messageId")]
    public string? MessageId { get; set; }

    [JsonProperty("headers")]
    public Reference<AsyncApiSchema>? Headers { get; set; }

    [JsonProperty("payload")]
    public JToken? Payload { get; set; }

    [JsonProperty("correlationId")]
    public Reference<CorrelationId>? CorrelationId { get; set; }

    [JsonProperty("schemaFormat")]
    public string? SchemaFormat { get; set; }

    [JsonProperty("contentType")]
    public string? ContentType { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("tags")]
    public Tag[]? Tags { get; set; }

    [JsonProperty("externalDocs")]
    public ExternalDocumentation? ExternalDocs { get; set; }

    [JsonProperty("bindings")]
    public Reference<MessageBindings>? Bindings { get; set; }

    [JsonProperty("examples")]
    public ICollection<MessageExample>? Examples { get; set; }

    [JsonProperty("traits")]
    public ICollection<Reference<MessageTraits>>? Traits { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object?>? ExtensionData { get; set; }
}

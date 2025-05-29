using ApiCodeGenerator.AsyncApi.DOM.Traits;
using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Message : ExtensionRefObject, ITraitsAware<Message, MessageTraits>
{
    /// <summary>Schema definition of the application headers. </summary>
    [JsonProperty("headers")]
    public Reference<AsyncApiSchema>? Headers { get; set; }

    /// <summary>Definition of the message payload.</summary>
    [JsonProperty("payload")]
    public Reference<AsyncApiSchema>? Payload { get; set; }

    /// <summary>Definition of the correlation ID used for message tracing or matching.</summary>
    [JsonProperty("correlationId")]
    public Reference<CorrelationId>? CorrelationId { get; set; }

    /// <summary>The content type to use when encoding/decoding a message's payload.</summary>
    [JsonProperty("contentType")]
    public string? ContentType { get; set; }

    /// <summary>A machine-friendly name for the message.</summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>A human-friendly title for the message.</summary>
    [JsonProperty("title")]
    public string? Title { get; set; }

    /// <summary>A short summary of what the message is about.</summary>
    [JsonProperty("summary")]
    public string? Summary { get; set; }

    /// <summary>A verbose explanation of the message.</summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>A list of tags for logical grouping and categorization of messages.</summary>
    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    /// <summary>Additional external documentation for this message.</summary>
    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }

    /// <summary>A map where the keys describe the name of the protocol and the values describe protocol-specific definitions for the message.</summary>
    [JsonProperty("bindings")]
    public Reference<MessageBindings>? Bindings { get; set; }

    /// <summary>List of examples.</summary>
    [JsonProperty("examples")]
    public ICollection<MessageExample>? Examples { get; set; }

    [JsonProperty("traits")]
    public ICollection<Reference<MessageTraits>>? Traits { get; set; }
}

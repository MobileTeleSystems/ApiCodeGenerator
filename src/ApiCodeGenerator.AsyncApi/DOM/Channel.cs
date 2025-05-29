using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Channel : ExtensionRefObject
{
    /// <summary>An optional string representation of this channel's address.</summary>
    /// <remarks>The address is typically the "topic name", "routing key", "event type", or "path".</remarks>
    [JsonProperty("address")]
    public string? Address { get; set; }

    /// <summary>A map of the messages that will be sent to this channel by any application at any time.</summary>
    [JsonProperty("messages")]
    public IDictionary<string, NamedReference<Message>> Messages { get; } = new Internal.NamedReferenceDictionary<Message>();

    /// <summary>A human-friendly title for the channel.</summary>
    [JsonProperty("title")]
    public string? Title { get; set; }

    /// <summary>A short summary of the channel.</summary>
    [JsonProperty("summary")]
    public string? Summary { get; set; }

    /// <summary>An optional description of this channel.</summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>An array of $ref pointers to the definition of the servers in which this channel is available.</summary>
    [JsonProperty("servers")]
    public ICollection<Reference<Server>>? Servers { get; set; }

    /// <summary>A map of the parameters included in the channel address. </summary>
    [JsonProperty("parameters")]
    public IDictionary<string, NamedReference<Parameter>> Parameters { get; } = new Internal.NamedReferenceDictionary<Parameter>();

    /// <summary>A list of tags for logical grouping of channels.</summary>
    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    /// <summary>Additional external documentation for this channel.</summary>
    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }

    /// <summary>A map where the keys describe the name of the protocol and the values describe protocol-specific definitions for the channe.</summary>
    [JsonProperty("bindings")]
    public Reference<ChannelBindings>? Bindings { get; set; }
}

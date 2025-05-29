using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class AsyncApiDocument : JsonExtensionObject, IDocumentPathProvider
{
    /// <summary>Specifies the AsyncAPI Specification version being used.</summary>
    [JsonProperty(PropertyName = "asyncApi", Order = 1, Required = Required.Always)]
    public string AsyncApi { get; set; } = "3.0.0";

    /// <summary>Identifier of the application the AsyncAPI document is defining.</summary>
    [JsonProperty(PropertyName = "id")]
    public string? Id { get; set; }

    /// <summary>Provides metadata about the API. The metadata can be used by the clients if needed.</summary>
    [JsonProperty(PropertyName = "info", Required = Required.Always)]
    public Info Info { get; set; } = new();

    /// <summary>Provides connection details of servers.</summary>
    [JsonProperty(PropertyName = "servers")]
    public IDictionary<string, Reference<Server>>? Servers { get; set; }

    /// <summary>Default content type to use when encoding/decoding a message's payload.</summary>
    [JsonProperty("defaultContentType")]
    public string? DefaultContentType { get; set; }

    /// <summary>The channels used by this application.</summary>
    [JsonProperty("channels")]
    public IDictionary<string, NamedReference<Channel>> Channels { get; } = new Internal.NamedReferenceDictionary<Channel>();

    /// <summary>The operations this application MUST implement.</summary>
    [JsonProperty("operations")]
    public IDictionary<string, NamedReference<Operation>> Operations { get; } = new Internal.NamedReferenceDictionary<Operation>();

    /// <summary>An element to hold various reusable objects for the specification.</summary>
    [JsonProperty("components", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
    public Components Components { get; } = new();

    /// <inheritdoc/>
    [JsonIgnore]
    public string? DocumentPath { get; set; }
}

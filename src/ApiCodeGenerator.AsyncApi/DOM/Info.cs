using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Info : JsonExtensionObject
{
    /// <summary>Gets or sets the title.</summary>
    [JsonProperty(PropertyName = "title", Required = Required.Always)]
    public string Title { get; set; } = "Swagger specification";

    /// <summary>Gets or sets the description.</summary>
    [JsonProperty(PropertyName = "description")]
    public string? Description { get; set; }

    /// <summary>Gets or sets the terms of service.</summary>
    [JsonProperty(PropertyName = "termsOfService")]
    public string? TermsOfService { get; set; }

    /// <summary>Gets or sets the contact information.</summary>
    [JsonProperty(PropertyName = "contact")]
    public Contact? Contact { get; set; }

    /// <summary>Gets or sets the license information.</summary>
    [JsonProperty(PropertyName = "license")]
    public License? License { get; set; }

    /// <summary>Gets or sets the API version.</summary>
    [JsonProperty(PropertyName = "version", Required = Required.Always)]
    public string Version { get; set; } = "1.0.0";

    /// <summary>A list of tags used by the specification with additional metadata.</summary>
    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    /// <summary>Additional external documentation of the exposed API.</summary>
    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }
}

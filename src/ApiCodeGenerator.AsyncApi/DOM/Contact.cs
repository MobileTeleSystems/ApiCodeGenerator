using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Contact : JsonExtensionObject
{
    /// <summary>Gets or sets the name.</summary>
    [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string? Name { get; set; }

    /// <summary>Gets or sets the contact URL.</summary>
    [JsonProperty(PropertyName = "url", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string? Url { get; set; }

    /// <summary>Gets or sets the contact email.</summary>
    [JsonProperty(PropertyName = "email", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string? Email { get; set; }
}

using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

/// <summary>
/// External documentation.
/// </summary>
public class ExternalDocumentation : ExtensionRefObject
{
    /// <summary>
    /// A short description of the target documentation.
    /// </summary>
    /// <remarks>
    /// CommonMark syntax can be used for rich text representation.
    /// </remarks>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The URL for the target documentation.
    /// </summary>
    [JsonProperty("url", Required = Required.Always)]
    public required Uri Url { get; set; }
}

using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Parameter : ExtensionRefObject
{
    /// <summary>An enumeration of string values to be used if the substitution options are from a limited set.</summary>
    [JsonProperty("enum")]
    public ICollection<string>? Enum { get; set; }

    /// <summary>The default value to use for substitution, and to send, if an alternate value is not supplied.</summary>
    [JsonProperty("default")]
    public string? Default { get; set; }

    /// <summary>An optional description for the parameter.</summary>
    [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string? Description { get; set; }

    /// <summary>An array of examples of the parameter value.</summary>
    [JsonProperty("examples")]
    public ICollection<string>? Examples { get; set; }

    /// <summary>A runtime expression that specifies the location of the parameter value.</summary>
    [JsonProperty("location")]
    public string? Location { get; set; }
}

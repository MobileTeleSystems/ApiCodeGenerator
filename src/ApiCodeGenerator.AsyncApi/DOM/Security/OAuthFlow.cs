using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class OAuthFlow
{
    [JsonProperty("refreshUrl")]
    public string? RefreshUrl { get; set; }

    [JsonProperty("availableScopes", Required = Required.Always)]
    public required IDictionary<string, string> AvailableScopes { get; set; }
}

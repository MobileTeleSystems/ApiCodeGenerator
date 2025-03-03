using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class ClientCredentialsOAuthFlow : OAuthFlow
{
    [JsonProperty("tokenUrl", Required = Required.Always)]
    public required string TokenUrl { get; set; }
}

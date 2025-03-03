using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

[JsonConverter(typeof(Serialization.InheritanceConverter<SecurityScheme>), nameof(Type))]
[Serialization.KnownType(ApiKeySecurityScheme.SchemaType, typeof(ApiKeySecurityScheme))]
[Serialization.KnownType(HttpApiKeySecurityScheme.SchemaType, typeof(HttpApiKeySecurityScheme))]
[Serialization.KnownType(HttpSecurityScheme.SchemaType, typeof(HttpSecurityScheme))]
[Serialization.KnownType(OAuth2SecurityScheme.SchemaType, typeof(OAuth2SecurityScheme))]
[Serialization.KnownType(OpenIdConnectSecurityScheme.SchemaType, typeof(OpenIdConnectSecurityScheme))]
[Serialization.KnownType("*", typeof(SecurityScheme))]
public class SecurityScheme : ExtensionRefObject
{
    public SecurityScheme(string type)
    {
        Type = type;
    }

    [JsonProperty("type")]
    public string Type { get; }

    [JsonProperty("description")]
    public string? Description { get; set; }
}

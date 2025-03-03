using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class HttpSecurityScheme : SecurityScheme
{
    internal const string SchemaType = "http";

    internal HttpSecurityScheme()
        : base(SchemaType)
    {
    }

    [JsonProperty("scheme", Required = Required.Always)]
    public required string Scheme { get; set; }

    [JsonProperty("bearerFormat")]
    public string? BearerFormat { get; set; }
}

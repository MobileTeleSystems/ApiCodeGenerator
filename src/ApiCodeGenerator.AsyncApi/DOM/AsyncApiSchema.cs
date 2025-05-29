using System.ComponentModel;
using ApiCodeGenerator.AsyncApi.DOM.Serialization;
using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

[JsonConverter(typeof(AsyncApiSchemaConverter))]
public class AsyncApiSchema : JsonSchema
{
    public const string AsyncApi = "application/vnd.aai.asyncapi";
    public const string AsyncApi3 = AsyncApi + ";version=3.0.0";
    public const string JsonSchema07 = "application/schema+json;version=draft-07";
    public const string JsonSchema07Yaml = "application/schema+yaml;version=draft-07";
    public const string OpenApi = "application/vnd.oai.openapi";

    [JsonProperty("schemaFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(AsyncApi3)]
    public required string SchemaFormat { get; set; } = AsyncApi3;
}

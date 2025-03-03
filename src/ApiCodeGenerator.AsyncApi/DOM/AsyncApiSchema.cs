using ApiCodeGenerator.AsyncApi.DOM.Serialization;
using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

[JsonConverter(typeof(AsyncApiSchemaConverter))]
public class AsyncApiSchema : JsonSchema
{
    public required string SchemaFormat { get; set; }
}

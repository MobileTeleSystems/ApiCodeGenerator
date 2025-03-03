using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

internal class AsyncApiSchemaConverter : JsonConverter
{
    public const string AsyncApi = "application/vnd.aai.asyncapi";
    public const string AsyncApi3 = AsyncApi + ";version=3.0.0";
    public const string JsonSchema07 = "application/schema+json;version=draft-07";
    public const string JsonSchema07Yaml = "application/schema+yaml;version=draft-07";
    public const string OpenApi = "application/vnd.oai.openapi";

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => objectType == typeof(AsyncApiSchema);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jobj = (JObject)JToken.ReadFrom(reader);
        var format = AsyncApi3;
        JToken schemaDefinition = jobj;
        if (jobj.Property("schemaFormat") != null)
        {
            format = jobj.GetValue("schemaFormat")!.Value<string?>()
                            ?? throw new JsonSerializationException("SchemaFormat property must contain the value");
            schemaDefinition = jobj.GetValue("schema")
                            ?? throw new JsonSerializationException("Schema property must contain the value");
        }

        if (jobj.Property("$ref") != null)
        {
            var schema = new AsyncApiSchema { SchemaFormat = "$ref" };
            serializer.Populate(jobj.CreateReader(), schema);
            return schema;
        }

        return DeserializeSchema(format, schemaDefinition, serializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => throw new NotImplementedException();

    private AsyncApiSchema DeserializeSchema(string format, JToken schemaDefinition, JsonSerializer serializer)
    {
        if (format.StartsWith(AsyncApi)
          || format.StartsWith(OpenApi)
          || format == JsonSchema07
          || format == JsonSchema07Yaml)
        {
            var aaSchema = new AsyncApiSchema { SchemaFormat = format };
            serializer.Populate(schemaDefinition.CreateReader(), aaSchema);
            return aaSchema;
        }
        else
        {
            throw new NotSupportedException($"'{format}' is not supported format.");
        }
    }
}

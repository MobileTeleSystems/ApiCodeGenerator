using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

internal class AsyncApiSchemaConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => objectType == typeof(AsyncApiSchema);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jobj = (JObject)JToken.ReadFrom(reader);
        var format = AsyncApiSchema.AsyncApi3;
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
        => throw new NotSupportedException();

    private AsyncApiSchema DeserializeSchema(string format, JToken schemaDefinition, JsonSerializer serializer)
    {
        if (format.StartsWith(AsyncApiSchema.AsyncApi)
          || format.StartsWith(AsyncApiSchema.OpenApi)
          || format == AsyncApiSchema.JsonSchema07
          || format == AsyncApiSchema.JsonSchema07Yaml)
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

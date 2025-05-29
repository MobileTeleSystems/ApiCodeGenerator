using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal sealed class MessageConverter : JsonConverter
{
    public override bool CanWrite => false;

    public static bool CanConvert_(Type objectType) => typeof(DOM.Message) == objectType;

    public override bool CanConvert(Type objectType) => CanConvert_(objectType);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var message = serializer.Deserialize<Message>(reader)!;
        return MessageToV3(message, serializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotSupportedException();

    internal static DOM.Message MessageToV3(Message message, JsonSerializer serializer)
    {
        var payloadDef = string.IsNullOrEmpty(message.SchemaFormat) || message.SchemaFormat!.StartsWith(AsyncApiSchema.AsyncApi)
            ? message.Payload
            : new JObject(
                new JProperty("SchemaFormat", message.SchemaFormat),
                new JProperty("Schema", message.Payload));

        var payload = payloadDef?.ToObject<AsyncApiSchema?>(serializer);
        return new()
        {
            Bindings = message.Bindings,
            ContentType = message.ContentType,
            CorrelationId = message.CorrelationId,
            Description = message.Description,
            Examples = message.Examples,
            ExtensionData = message.ExtensionData,
            ExternalDocs = message.ExternalDocs is null ? null : (Reference<ExternalDocumentation>)message.ExternalDocs,
            Headers = message.Headers,
            Name = message.Name,
            Payload = payload is null ? null : (Reference<AsyncApiSchema>)payload,
            Summary = message.Summary,
            Tags = message.Tags is null ? null : [.. message.Tags],
            Title = message.Title,
            Traits = message.Traits,
        };
    }
}

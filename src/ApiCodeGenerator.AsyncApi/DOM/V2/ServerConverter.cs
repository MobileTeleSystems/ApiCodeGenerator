using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerV2 = ApiCodeGenerator.AsyncApi.DOM.V2.Server;
using ServerV3 = ApiCodeGenerator.AsyncApi.DOM.Server;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal class ServerConverter : JsonConverter
{
    private readonly ReferenceResolver _referenceResolver;

    public ServerConverter(ReferenceResolver referenceResolver)
    {
        _referenceResolver = referenceResolver;
    }

    public override bool CanConvert(Type objectType) => typeof(ServerV3) == objectType;

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var serverV2 = serializer.Deserialize<ServerV2>(reader)
            ?? throw new JsonSerializationException(
                $"Required property '{reader.Path.Substring(reader.Path.LastIndexOf('.') + 1)}' " +
                $"expects a value but got null. Path '{reader.Path.Substring(0, reader.Path.LastIndexOf('.'))}'.");

        return ToV3(serverV2, reader.Path);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    private DOM.Server ToV3(ServerV2 serverV2, string path)
    {
        var uri = new Uri(serverV2.Url);
        return new()
        {
            Bindings = serverV2.Bindings,
            Host = serverV2.Url.Contains('/') ? uri.Host : serverV2.Url,
            Protocol = serverV2.Protocol,
            Description = serverV2.Description,
            PathName = uri.PathAndQuery,
            ProtocolVersion = serverV2.ProtocolVersion,
            Tags = serverV2.Tags,
            Variables = serverV2.Variables,
            Security = _referenceResolver.GetSecuritySchemeRefs(serverV2.Security, path),
        };
    }
}

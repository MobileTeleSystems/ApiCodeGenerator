using ApiCodeGenerator.AsyncApi.DOM.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

/// <summary>
/// The AsyncAPI v2 document reader.
/// </summary>
public static partial class AsyncApiSerializer
{
    private static AsyncApiDocument DeserializeV2(JObject jObject, JsonSerializerSettings serializerSettings, out JsonSerializer serializer)
    {
        serializer = JsonSerializer.Create(serializerSettings);
        var document = new AsyncApiDocument();
        ReferenceResolver referenceResolver = new(jObject);
        serializer.ContractResolver = new V2.ContractResolver(new ChannelItemConverter(document, referenceResolver)); // needed for set converters
        serializer.Converters.Add(new ServerConverter(referenceResolver));
        serializer.Converters.Add(new MessageConverter());
        serializer.Populate(jObject.CreateReader(), document);
        MigrateTags(document, serializer);
        MigrateExternalDocs(document, serializer);
        return document;
    }

    private static void MigrateTags(AsyncApiDocument document, JsonSerializer serializer)
    {
        if (document.ExtensionData?.TryGetValue("tags", out var tags) == true && tags is JArray tagsArr)
        {
            document.Info.Tags = tagsArr.ToObject<ICollection<Reference<Tag>>>(serializer);
        }
    }

    private static void MigrateExternalDocs(AsyncApiDocument document, JsonSerializer serialzer)
    {
        if (document.ExtensionData?.TryGetValue("externalDocs", out var externalDocs) == true && externalDocs is JObject extDocsObj)
        {
            document.Info.ExternalDocs = extDocsObj.ToObject<ExternalDocumentation>(serialzer)!;
        }
    }
}

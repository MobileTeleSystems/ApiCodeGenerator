using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Generation;
using NJsonSchema.Yaml;
using YamlDotNet.Serialization;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

public static class AsyncApiSerializer
{
    private static readonly JsonSerializerSettings JSONSERIALIZERSETTINGS = new()
    {
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
    };

    /// <summary>
    /// Load document from JSON text.
    /// </summary>
    /// <param name="data">JSON text.</param>
    /// <returns>AsyncApi document object model.</returns>
    public static Task<AsyncApiDocument> FromJsonAsync(string data)
        => FromJsonAsync(data, null);

    /// <summary>
    /// Load document from JSON text.
    /// </summary>
    /// <param name="data">JSON text.</param>
    /// <param name="documentPath"> Path to document. </param>
    /// <returns>AsyncApi document object model.</returns>
    public static Task<AsyncApiDocument> FromJsonAsync(string data, string? documentPath)
    {
        var jObject = JObject.Parse(data);
        return FromJObject(jObject, documentPath);
    }

    /// <summary>
    /// Load document from YAML text.
    /// </summary>
    /// <param name="data">YAML text.</param>
    /// <returns>AsyncApi document object model.</returns>
    public static Task<AsyncApiDocument> FromYamlAsync(string data)
        => FromYamlAsync(data, null);

    /// <summary>
    /// Load document from YAML text.
    /// </summary>
    /// <param name="data">YAML text.</param>
    /// <param name="documentPath"> Path to document. </param>
    /// <returns>AsyncApi document object model.</returns>
    public static Task<AsyncApiDocument> FromYamlAsync(string data, string? documentPath)
    {
        JObject jObject = ParseYaml(data);
        return FromJObject(jObject, documentPath);
    }

    private static JObject ParseYaml(string data)
    {
        var deserializer = new DeserializerBuilder().Build();
        using var reader = new StringReader(data);
        var yamlDocument = deserializer.Deserialize(reader)!;

        return JObject.FromObject(yamlDocument)!;
    }

    private static Task<AsyncApiDocument> FromJObject(JObject jObject, string? documentPath)
    {
        var serializer = JsonSerializer.Create(JSONSERIALIZERSETTINGS);
        var doc = serializer.Deserialize<AsyncApiDocument>(jObject.CreateReader())!;
        doc.DocumentPath = documentPath;
        return UpdateSchemaReferencesAsync(doc, serializer.ContractResolver);
    }

    private static async Task<AsyncApiDocument> UpdateSchemaReferencesAsync(AsyncApiDocument document, IContractResolver contractResolver)
    {
        await new AsyncApiReferenceUpdater(
            document,
            new JsonAndYamlReferenceResolver(new AsyncApiSchemaResolver(document, new SystemTextJsonSchemaGeneratorSettings())),
            contractResolver)
            .VisitAsync(document, default)
            .ConfigureAwait(false);
        return document;
    }
}

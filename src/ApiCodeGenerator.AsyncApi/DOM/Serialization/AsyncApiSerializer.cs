using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Generation;
using NJsonSchema.Yaml;
using YamlDotNet.Serialization;
using YamlException = YamlDotNet.Core.YamlException;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

/// <summary>
/// The AsyncAPI v3 document reader.
/// </summary>
public static partial class AsyncApiSerializer
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
        try
        {
            var jObject = JObject.Parse(data);
            return FromJObject(jObject, documentPath);
        }
        catch (JsonSerializationException jsonEx)
        {
            throw new AsyncApiSerializationException(
                $"Json parsing failed. {jsonEx.Message}",
                jsonEx);
        }
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
        try
        {
            JObject jObject = ParseYaml(data);
            return FromJObject(jObject, documentPath);
        }
        catch (YamlException yamlEx)
        {
            throw new AsyncApiSerializationException(
                $"Yaml parsing filed. {yamlEx.Message}",
                yamlEx);
        }
        catch (JsonSerializationException jsonEx)
        {
            throw new AsyncApiSerializationException(
                $"Json parsing failed. {jsonEx.Message}",
                jsonEx);
        }
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
        var version = GetDocumentVersion(jObject);
        var majorVersion = new string(version.TakeWhile(x => x != '.').ToArray());
        JsonSerializer serializer;
        var doc = majorVersion switch
        {
            "2" => DeserializeV2(jObject, JSONSERIALIZERSETTINGS, out serializer),
            "3" => DeserializeV3(jObject, JSONSERIALIZERSETTINGS, out serializer),
            _ => throw new AsyncApiSerializationException($"Version '{version}' not supported."),
        };
        doc.DocumentPath = documentPath;
        return UpdateSchemaReferencesAsync(doc, serializer.ContractResolver);
    }

    private static string GetDocumentVersion(JObject jObject)
    {
        string asyncApiVersion;
        if (!jObject.TryGetValue("asyncapi", out var asyncApiVersionToken)
            || asyncApiVersionToken.Type != JTokenType.String
            || string.IsNullOrEmpty(asyncApiVersion = asyncApiVersionToken.ToString()))
        {
            throw new JsonSerializationException($"Required property 'asyncapi' not was found or its value is not a string.");
        }

        return asyncApiVersion;
    }

    private static AsyncApiDocument DeserializeV3(JObject jObject, JsonSerializerSettings serializerSettings, out JsonSerializer serializer)
    {
        serializer = JsonSerializer.Create(serializerSettings);
        return serializer.Deserialize<AsyncApiDocument>(jObject.CreateReader())!;
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

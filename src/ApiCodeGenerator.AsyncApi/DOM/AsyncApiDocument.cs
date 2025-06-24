using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.Generation;
using NJsonSchema.Yaml;
using YamlDotNet.Serialization;

namespace ApiCodeGenerator.AsyncApi.DOM
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class AsyncApiDocument : IDocumentPathProvider
    {
        private static readonly JsonSerializerSettings JSONSERIALIZERSETTINGS = new()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ConstructorHandling = ConstructorHandling.Default,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        };

        [JsonProperty(PropertyName = "asyncApi", Order = 1, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string AsyncApi { get; set; }

        [JsonProperty(PropertyName = "info", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Info? Info { get; set; }

        [JsonProperty(PropertyName = "servers", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IDictionary<string, Server> Servers { get; set; }

        [JsonProperty("defaultContentType")]
        public string? DefaultContentType { get; set; }

        [JsonProperty("channels", DefaultValueHandling = DefaultValueHandling.Populate)]
        public IDictionary<string, Channel>? Channels { get; set; } = new Dictionary<string, Channel>();

        [JsonProperty("components", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual Components? Components { get; set; }

        [JsonProperty("tags", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual ICollection<Tag>? Tags { get; set; }

        [JsonProperty("externalDocs", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ICollection<ExternalDocumentation>? ExternalDocs { get; set; }

        [JsonIgnore]
        public string? DocumentPath { get; set; }

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
        public static async Task<AsyncApiDocument> FromJsonAsync(string data, string? documentPath)
        {
            var document = JsonConvert.DeserializeObject<AsyncApiDocument>(data, JSONSERIALIZERSETTINGS)!;
            document.DocumentPath = documentPath;
            await UpdateSchemaReferencesAsync(document);
            BuildAsyncApiDescriminatorMapping(document);
            return document;
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
        public static async Task<AsyncApiDocument> FromYamlAsync(string data, string? documentPath)
        {
            var deserializer = new DeserializerBuilder().Build();
            using var reader = new StringReader(data);
            var yamlDocument = deserializer.Deserialize(reader)!;

            var jObject = JObject.FromObject(yamlDocument)!;
            var serializer = JsonSerializer.Create(JSONSERIALIZERSETTINGS);
            var doc = jObject.ToObject<AsyncApiDocument>(serializer)!;
            doc.DocumentPath = documentPath;
            await UpdateSchemaReferencesAsync(doc);
            BuildAsyncApiDescriminatorMapping(doc);
            return doc;
        }

        private static Task UpdateSchemaReferencesAsync(AsyncApiDocument document)
        {
            return JsonSchemaReferenceUtilities.UpdateSchemaReferencesAsync(
                       document,
                       new JsonAndYamlReferenceResolver(new AsyncApiSchemaResolver(document, new SystemTextJsonSchemaGeneratorSettings())));
        }

        private static void BuildAsyncApiDescriminatorMapping(AsyncApiDocument document)
        {
            foreach (var schema in document.Components?.Schemas.Values ?? [])
            {
                var discriminatorPropName = schema.DiscriminatorObject?.PropertyName;
                if (discriminatorPropName != null)
                {
                    var derivedSchemas = schema.GetDerivedSchemas(document);
                    foreach (var item in derivedSchemas)
                    {
                        var derivedSchema = item.Key;
                        if ((derivedSchema.Properties.TryGetValue(discriminatorPropName, out var discriminatorProp)
                            || derivedSchema.AllOf?.FirstOrDefault(i => i != schema && i.Properties.ContainsKey(discriminatorPropName))?.Properties.TryGetValue(discriminatorPropName, out discriminatorProp) == true)
                            && discriminatorProp.ExtensionData?.TryGetValue("const", out var constValue) == true)
                        {
                            var constValueStr = constValue!.ToString();
                            discriminatorProp.ParentSchema!.Properties.Remove(discriminatorPropName);
                            schema.DiscriminatorObject!.Mapping.Add(constValueStr, derivedSchema);
                        }
                    }
                }
            }
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

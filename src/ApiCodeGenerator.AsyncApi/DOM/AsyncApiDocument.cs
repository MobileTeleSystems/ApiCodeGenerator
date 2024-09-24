using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using YamlDotNet.Serialization;

namespace ApiCodeGenerator.AsyncApi.DOM
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class AsyncApiDocument
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

        /// <summary>
        /// Load document from JSON text.
        /// </summary>
        /// <param name="data">JSON text.</param>
        /// <returns>AsyncApi document object model.</returns>
        public static Task<AsyncApiDocument> FromJsonAsync(string data)
        {
            var document = JsonConvert.DeserializeObject<AsyncApiDocument>(data, JSONSERIALIZERSETTINGS)!;
            return UpdateSchemaReferencesAsync(document);
        }

        /// <summary>
        /// Load document from YAML text.
        /// </summary>
        /// <param name="data">YAML text.</param>
        /// <returns>AsyncApi document object model.</returns>
        public static Task<AsyncApiDocument> FromYamlAsync(string data)
        {
            var deserializer = new DeserializerBuilder().Build();
            using var reader = new StringReader(data);
            var yamlDocument = deserializer.Deserialize(reader)!;

            var jObject = JObject.FromObject(yamlDocument)!;
            var serializer = JsonSerializer.Create(JSONSERIALIZERSETTINGS);
            var doc = jObject.ToObject<AsyncApiDocument>(serializer)!;
            return UpdateSchemaReferencesAsync(doc);
        }

        private static Task<AsyncApiDocument> UpdateSchemaReferencesAsync(AsyncApiDocument document)
            => JsonSchemaReferenceUtilities.UpdateSchemaReferencesAsync(
                document,
                new(new AsyncApiSchemaResolver(document, new SystemTextJsonSchemaGeneratorSettings())))
                .ContinueWith(t => document);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

using System.Security.Cryptography;
using ApiCodeGenerator.Core.Converters;
using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure;

internal static partial class TestHelpers
{
    public static readonly string VERSION = typeof(TestHelpers).Assembly.GetName().Version?.ToString() ?? string.Empty;

    public static readonly string NEWTON_VERSION = "13.0.0.0";
    public static readonly string NJSON_VERSION = "11.0.2.0 (Newtonsoft.Json v" + NEWTON_VERSION + ")";
    public static readonly string APICODEGEN_VERSION = VERSION + " (NJsonSchema v" + NJSON_VERSION + ")";
    public static readonly string GENERATED_CODE = "[System.CodeDom.Compiler.GeneratedCode(\"NJsonSchema\", \"" + APICODEGEN_VERSION + "\")]";
    public static readonly string GENERATED_CODE_ATTRIBUTE = "[System.CodeDom.Compiler.GeneratedCode(\"ApiCodeGenerator.AsyncApi\", \"" + APICODEGEN_VERSION + "\")]";

    public static readonly string JSON_INHERITANCE_CONVERTER = $$"""
        {{GENERATED_CODE}}
        [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Interface, AllowMultiple = true)]
        internal class JsonInheritanceAttribute : System.Attribute
        {
            public JsonInheritanceAttribute(string key, System.Type type)
            {
                Key = key;
                Type = type;
            }

            public string Key { get; }

            public System.Type Type { get; }
        }

        {{GENERATED_CODE}}
        public class JsonInheritanceConverter : Newtonsoft.Json.JsonConverter
        {
            internal static readonly string DefaultDiscriminatorName = "discriminator";

            private readonly string _discriminatorName;

            [System.ThreadStatic]
            private static bool _isReading;

            [System.ThreadStatic]
            private static bool _isWriting;

            public JsonInheritanceConverter()
            {
                _discriminatorName = DefaultDiscriminatorName;
            }

            public JsonInheritanceConverter(string discriminatorName)
            {
                _discriminatorName = discriminatorName;
            }

            public string DiscriminatorName { get { return _discriminatorName; } }

            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                try
                {
                    _isWriting = true;

                    var jObject = Newtonsoft.Json.Linq.JObject.FromObject(value, serializer);
                    jObject.AddFirst(new Newtonsoft.Json.Linq.JProperty(_discriminatorName, GetSubtypeDiscriminator(value.GetType())));
                    writer.WriteToken(jObject.CreateReader());
                }
                finally
                {
                    _isWriting = false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    if (_isWriting)
                    {
                        _isWriting = false;
                        return false;
                    }
                    return true;
                }
            }

            public override bool CanRead
            {
                get
                {
                    if (_isReading)
                    {
                        _isReading = false;
                        return false;
                    }
                    return true;
                }
            }

            public override bool CanConvert(System.Type objectType)
            {
                return true;
            }

            public override object ReadJson(Newtonsoft.Json.JsonReader reader, System.Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                var jObject = serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(reader);
                if (jObject == null)
                    return null;

                var discriminatorValue = jObject.GetValue(_discriminatorName);
                var discriminator = discriminatorValue != null ? Newtonsoft.Json.Linq.Extensions.Value<string>(discriminatorValue) : null;
                var subtype = GetObjectSubtype(objectType, discriminator);

                var objectContract = serializer.ContractResolver.ResolveContract(subtype) as Newtonsoft.Json.Serialization.JsonObjectContract;
                if (objectContract == null || System.Linq.Enumerable.All(objectContract.Properties, p => p.PropertyName != _discriminatorName))
                {
                    jObject.Remove(_discriminatorName);
                }

                try
                {
                    _isReading = true;
                    return serializer.Deserialize(jObject.CreateReader(), subtype);
                }
                finally
                {
                    _isReading = false;
                }
            }

            private System.Type GetObjectSubtype(System.Type objectType, string discriminator)
            {
                foreach (var attribute in System.Reflection.CustomAttributeExtensions.GetCustomAttributes<JsonInheritanceAttribute>(System.Reflection.IntrospectionExtensions.GetTypeInfo(objectType), true))
                {
                    if (attribute.Key == discriminator)
                        return attribute.Type;
                }

                return objectType;
            }

            private string GetSubtypeDiscriminator(System.Type objectType)
            {
                foreach (var attribute in System.Reflection.CustomAttributeExtensions.GetCustomAttributes<JsonInheritanceAttribute>(System.Reflection.IntrospectionExtensions.GetTypeInfo(objectType), true))
                {
                    if (attribute.Type == objectType)
                        return attribute.Key;
                }

                return objectType.Name;
            }
        }

    """.Replace("\r", string.Empty);

    public static string GetAsyncApiPath(string schemaFile) => Path.Combine("asyncApi", schemaFile);

    public static async Task<TextReader> LoadApiDocumentAsync(string fileName)
    {
        var filePath = GetAsyncApiPath(fileName);
        var data = await File.ReadAllTextAsync(filePath);
        return new StringReader(data);
    }

    public static async Task RunTest(CSharpClientGeneratorSettings settings, string expectedClientDeclartion, string schemaFile, string testOperResponseText, string? usings = null)
    {
        var reader = await LoadApiDocumentAsync(schemaFile);
        await RunTest(settings, expectedClientDeclartion, reader, testOperResponseText, usings);
    }

    public static async Task RunTest(CSharpClientGeneratorSettings settings, string expectedClientDeclartion, TextReader documentReader, string testOperResponseText, string? usings = null)
    {
        var generator = await CreateGenerator(documentReader, settings);
        var expected = GetExpectedCode(expectedClientDeclartion, testOperResponseText, usings: usings);

        //Act
        var actual = generator.Generate();

        //Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    public static GeneratorContext CreateContext(string settingsJson, TextReader docReader, Core.ExtensionManager.Extensions? extensions = null)
    {
        var jReader = new JsonTextReader(new StringReader(settingsJson));

        return new GeneratorContext(
            (t, s, _) => s!.Deserialize(jReader, t),
            extensions ?? new(AcgExtension.CodeGenerators, GetOperationGenerators()),
            new Dictionary<string, string>())
        {
            DocumentReader = docReader,
        };
    }

    public static GeneratorContext CreateContext(string settingsJson, string schemaFile, Core.ExtensionManager.Extensions? extensions = null)
        => CreateContext(settingsJson, File.OpenText(GetAsyncApiPath(schemaFile)), extensions);

    public static string GetExpectedCode(string? expectedClientDeclartion, string? testOperResponseText, string @namespace = "TestNS", string? usings = null)
    {
        if (!string.IsNullOrWhiteSpace(expectedClientDeclartion) && !expectedClientDeclartion.Contains(GENERATED_CODE_ATTRIBUTE))
        {
            expectedClientDeclartion = "    " + GENERATED_CODE_ATTRIBUTE + "\n" + expectedClientDeclartion;
        }

        var expected = "//----------------------\n" +
            "// <auto-generated>\n" +
            "//     Generated using the ApiCodeGenerator.AsyncApi toolchain v" + APICODEGEN_VERSION + "\n" +
            "// </auto-generated>\n" +
            "//----------------------\n" +
            "\n" +
            (usings ?? GetAdditionalUsings()) +
            "\n" +
            "\n" +
           $"namespace {@namespace}\n" +
            "{\n" +
            (expectedClientDeclartion is null ? string.Empty : expectedClientDeclartion) +
            (testOperResponseText is null ? string.Empty : (testOperResponseText + "\n")) +
            "}\n";
        return expected;
    }

    public static string GetExpectedClientCode(string className, string operationsCode, int identCnt = 4, string? baseType = null, string typeKind = "partial class")
    {
        var ident = new string(' ', identCnt);
        return
        ident + "/// <summary>\n" +
        ident + "/// The Smartylighting Streetlights API allows you to remotely manage the city lights.\n" +
        ident + "/// <br/>\n" +
        ident + "/// <br/>### Check out its awesome features:\n" +
        ident + "/// <br/>\n" +
        ident + "/// <br/>* Turn a specific streetlight on/off 🌃\n" +
        ident + "/// <br/>* Dim a specific streetlight 😎\n" +
        ident + "/// <br/>* Receive real-time information about environmental lighting conditions 📈\n" +
        ident + "/// <br/>\n" +
        ident + "/// </summary>\n" +
        ident + GENERATED_CODE_ATTRIBUTE + "\n" +
        ident + $"public {typeKind} {className}" + (string.IsNullOrEmpty(baseType) ? string.Empty : $" : {baseType}") + "\n" +
        ident + "{\n" +
        operationsCode +
        ident + "}\n";
    }

    public static string GetExpectedDtoCode(int identCnt = 4)
    {
        var ident = new string(' ', identCnt);
        return ident + GENERATED_CODE + "\n" +
        ident + "public partial class LightMeasuredPayload\n" +
        ident + "{\n" +
        ident + "    /// <summary>\n" +
        ident + "    /// Light intensity measured in lumens.\n" +
        ident + "    /// </summary>\n" +
        ident + "    [Newtonsoft.Json.JsonProperty(\"lumens\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
        ident + "    public int? Lumens { get; set; }\n" +
        "\n" +
        ident + "    [Newtonsoft.Json.JsonProperty(\"sentAt\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
        ident + "    public System.DateTimeOffset? SentAt { get; set; }\n" +
        GetAdditionalPropertiesCode(identCnt + 4) +
        ident + "}\n" +
        "\n" +
        ident + GENERATED_CODE + "\n" +
        ident + "public partial class TurnOnOffPayload\n" +
        ident + "{\n" +
        ident + "    /// <summary>\n" +
        ident + "    /// Whether to turn on or off the light.\n" +
        ident + "    /// </summary>\n" +
        ident + "    [Newtonsoft.Json.JsonProperty(\"command\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
        ident + "    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]\n" +
        ident + "    public TurnOnOffPayloadCommand? Command { get; set; }\n" +
        "\n" +
        ident + "    [Newtonsoft.Json.JsonProperty(\"sentAt\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
        ident + "    public System.DateTimeOffset? SentAt { get; set; }\n" +
        GetAdditionalPropertiesCode(identCnt + 4) +
        ident + "}\n" +
        "\n" +
        ident + GENERATED_CODE + "\n" +
        ident + "public partial class DimLightPayload\n" +
        ident + "{\n" +
        ident + "    /// <summary>\n" +
        ident + "    /// Percentage to which the light should be dimmed to.\n" +
        ident + "    /// </summary>\n" +
        ident + "    [Newtonsoft.Json.JsonProperty(\"percentage\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
        ident + "    public int? Percentage { get; set; }\n" +
        "\n" +
        ident + "    [Newtonsoft.Json.JsonProperty(\"sentAt\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
        ident + "    public System.DateTimeOffset? SentAt { get; set; }\n" +
        GetAdditionalPropertiesCode(identCnt + 4) +
        ident + "}\n" +
        "\n" +
        ident + GENERATED_CODE + "\n" +
        ident + "public enum TurnOnOffPayloadCommand\n" +
        ident + "{\n" +
        "\n" +
        ident + "    [System.Runtime.Serialization.EnumMember(Value = @\"on\")]\n" +
        ident + "    On = 0,\n" +
        "\n\n" +
        ident + "    [System.Runtime.Serialization.EnumMember(Value = @\"off\")]\n" +
        ident + "    Off = 1,\n" +
        "\n\n" +
        ident + "}\n"
        ;
    }

    public static string GetExpectedSummary(string text, int identCnt)
    {
        var ident = new string(' ', identCnt);
        return
        ident + "/// <summary>\n" +
        string.Join(null, text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(i => ident + "/// " + i + '\n')) +
        ident + "/// </summary>\n";
    }

    public static string GetExpectedPublisherCode(string name, string payloadType, int identCnt)
        => GetExpectedPublisherCode(name, payloadType, identCnt, []);

    /// <summary>
    /// Возвращает ожидаемый код паблишера.
    /// </summary>
    /// <param name="name">Имя метода.</param>
    /// <param name="payloadType">Тип параметра payload.</param>
    /// <param name="identCnt">Количество лидирующих пробелов.</param>
    /// <param name="bodyLines">Тело метода. Если null то тело не формируется. Если пустой массив то в теле пишется 'return Task.CompleetedTask'.</param>
    /// <returns>Строка с кодом паблишера.</returns>
    public static string GetExpectedPublisherCode(string name, string payloadType, int identCnt, string[]? bodyLines)
    {
        var ident = new string(' ', identCnt);
        var body = bodyLines switch
        {
            null => null,
            { Length: 0 } => ident + "    return Task.CompletedTask;\n",
            _ => string.Join(
                string.Empty,
                bodyLines.Select(l => string.IsNullOrEmpty(l) ? "\n" : $"{ident}    {l}\n")),
        };
        var bodyBlock = body is null
            ? ";\n"
            : $"\n{ident}{{\n{body}{ident}}}\n";
        return
        ident + "public Task " + name + "(string streetlightId, " + payloadType + " payload)"
              + bodyBlock;
    }

    public static string GetExpectedSubscriberCode(string name, string payloadType, int identCnt)
        => GetExpectedSubscriberCode(name, payloadType, identCnt, []);

    /// <summary>
    /// Возвращает ожидаемый код подписчика.
    /// </summary>
    /// <param name="name">Имя метода.</param>
    /// <param name="payloadType">Тип параметра payload.</param>
    /// <param name="identCnt">Количество лидирующих пробелов.</param>
    /// <param name="bodyLines">Тело метода. Если null то тело не формируется. Если пустой массив то в теле пишется 'return Task.CompleetedTask'.</param>
    /// <returns>Строка с кодом подписчика.</returns>
    public static string GetExpectedSubscriberCode(string name, string payloadType, int identCnt, string[]? bodyLines)
    {
        var ident = new string(' ', identCnt);
        var body = bodyLines switch
        {
            null => null,
            { Length: 0 } => string.Empty,
            _ => string.Join(
                string.Empty,
                bodyLines.Select(l => string.IsNullOrEmpty(l) ? "\n" : $"{ident}    {l}\n")),
        };
        var bodyBlock = body is null
            ? ";\n"
            : $"\n{ident}{{\n{body}{ident}}}\n";
        return
        ident + "public void " + name + "(string streetlightId, Action<" + payloadType + "> callback)"
              + bodyBlock;
    }

    public static string GetAdditionalPropertiesCode(int identCnt)
    {
        var ident = new string(' ', identCnt);
        return "\n\n\n" +
        ident + "private System.Collections.Generic.IDictionary<string, object> _additionalProperties;\n" +
        "\n" +
        ident + "[Newtonsoft.Json.JsonExtensionData]\n" +
        ident + "public System.Collections.Generic.IDictionary<string, object> AdditionalProperties\n" +
        ident + "{\n" +
        ident + "    get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }\n" +
        ident + "    set { _additionalProperties = value; }\n" +
        ident + "}\n\n";
    }

    public static T LoadSettings<T>(string json)
        where T : CSharpGeneratorBaseSettings
        => JsonConvert.DeserializeObject<T>(
                json,
                new JsonSerializerSettings()
                {
                    Converters =
                    {
                        new SettingsConverter(typeof(T), CSharpClientContentGenerator.UNWRAP_PROPS, null),
                    },
                })!;

    public static IReadOnlyDictionary<string, IReadOnlyCollection<Type>> GetOperationGenerators()
       => AcgExtension.OperationGenerators
            .ToDictionary(
                kv => kv.Key,
                kv => (IReadOnlyCollection<Type>)[kv.Value]);

    private static Task<IContentGenerator> CreateGenerator(TextReader document, CSharpClientGeneratorSettings settings)
    {
        var context = new GeneratorContext((_, _, _) => settings, new Core.ExtensionManager.Extensions(), new Dictionary<string, string>())
        {
            DocumentReader = document,
        };

        return CSharpClientContentGenerator.CreateAsync(context);
    }

    private static string GetAdditionalUsings() => "\n";
}

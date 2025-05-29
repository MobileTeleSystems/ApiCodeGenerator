using System.Collections.ObjectModel;
using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.NameGenerators;
using Moq;
using Newtonsoft.Json.Linq;
using NJsonSchema.References;
using NUnit.Framework.Constraints;

namespace ApiCodeGenerator.AsyncApi.Tests;

public class AsyncApiContentGeneratorTests
{
    [Test]
    public async Task LoadSettingsAsync()
    {
        var json = "{\"testProp\":\"val\", \"anyType\":\"JObject\", \"templateDirectory\":\"Tmpl\"}";

        var extensions = new Core.ExtensionManager.Extensions();
        GeneratorContext context = new(
            settingsFactory: GetSettingsFactory(json),
            extensions,
            variables: new Dictionary<string, string>())
        {
            DocumentReader = await TestHelpers.LoadApiDocumentAsync("asyncapi.json"),
        };

        var contentGenerator = (FakeContentGenerator)await FakeContentGenerator.CreateAsync(context);

        var settings = contentGenerator.FakeGenerator.Settings;
        Assert.NotNull(settings);
        Assert.AreEqual("val", settings.TestProp);
        Assert.AreEqual("JObject", settings.CSharpGeneratorSettings.AnyType);
        Assert.AreEqual("Tmpl", settings.CodeGeneratorSettings.TemplateDirectory);
    }

    [Test]
    public async Task Load_OperationGenerator()
    {
        var settingsJson = "{\"operationGenerationMode\": \"MultipleClientsFromFirstTagAndOperationId\"}";

        var context = TestHelpers.CreateContext(settingsJson, "asyncapi.json");
        var gen = await FakeContentGenerator.CreateAsync(context);

        Assert.NotNull(gen);
        Assert.IsInstanceOf<FakeContentGenerator>(gen);
        var fakeGen = (FakeContentGenerator)gen!;
        var settings = fakeGen.FakeGenerator.Settings;
        Assert.NotNull(settings);
        Assert.NotNull(settings.OperationNameGenerator);
        Assert.IsInstanceOf<MultipleClientsFromFirstTagAndOperationId>(settings.OperationNameGenerator);
    }

    [Test]
    public async Task ParameterNameReplacementOn()
    {
        var settingsJson = "{\"replaceNameCollection\":{\"@\":\"_\"}}";

        var context = TestHelpers.CreateContext(settingsJson, "asyncapi.json");
        var gen = await FakeContentGenerator.CreateAsync(context);

        Assert.NotNull(gen);
        Assert.IsInstanceOf<FakeContentGenerator>(gen);
        var fakeGen = (FakeContentGenerator)gen!;
        var settings = fakeGen.FakeGenerator.Settings;
        Assert.NotNull(settings);
        Assert.NotNull(settings.CSharpGeneratorSettings.PropertyNameGenerator);
        Assert.IsInstanceOf<PropertyNameGeneratorWithReplace>(settings.CSharpGeneratorSettings.PropertyNameGenerator);
        Assert.NotNull(settings.ParameterNameGenerator);
        Assert.IsInstanceOf<ParameterNameGeneratorWithReplace>(settings.ParameterNameGenerator);
    }

    [Test]
    public async Task LoadApiDocument_WithTextPreprocess()
    {
        const string schemaName = nameof(schemaName);
        var settingsJson = new JObject();

        Func<string, string?, string> dlgt = new FakeTextPreprocessor("{}").Process;

        var context = CreateContext(settingsJson);

        context.Preprocessors = new Preprocessors(
            new Dictionary<Type, Delegate[]> { [typeof(string)] = [dlgt] });

        var gen = (FakeContentGenerator)await FakeContentGenerator.CreateAsync(context);

        var apiDocument = gen.Document;

        Assert.NotNull(apiDocument);
        Assert.That(apiDocument.Components.Schemas, Does.ContainKey(schemaName));
        var sch = apiDocument.Components.Schemas[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
        Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"processed\":{}}"));
    }

    [Test]
    public async Task LoadApiDocument_WithTextPreprocess_Log()
    {
        const string schemaName = nameof(schemaName);
        const string filePath = "cd4bed67-1cc0-44a2-8dd1-30a0bd0c1dee";
        var settingsJson = new JObject();

        Func<string, string?, ILogger?, string> dlgt = new FakeTextPreprocessor("{}").Process;

        var logger = new Mock<ILogger>();
        var context = CreateContext(settingsJson);
        context.Logger = logger.Object;
        context.DocumentPath = filePath;

        context.Preprocessors = new Preprocessors(
            new Dictionary<Type, Delegate[]> { [typeof(string)] = [dlgt] });

        var gen = (FakeContentGenerator)await FakeContentGenerator.CreateAsync(context);

        var apiDocument = gen.Document;

        Assert.NotNull(apiDocument);
        Assert.That(apiDocument.Components.Schemas, Does.ContainKey(schemaName));
        var sch = apiDocument.Components.Schemas[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
        Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"processed\":{}}"));
        logger.Verify(l => l.LogWarning(It.IsAny<string>(), filePath, It.IsAny<string>()));
    }

    [Test]
    public async Task LoadApiDocument_WithModelPreprocess()
    {
        const string schemaName = nameof(schemaName);
        var settingsJson = new JObject();

        Func<AsyncApiDocument, string?, AsyncApiDocument> dlgt = new FakeModelPreprocessor("{}").Process;

        var context = CreateContext(settingsJson);
        context.DocumentReader = new StringReader($$"""
            {
              "asyncapi":"3.0.0",
              "info":{"title":"", "version": "1.0"}
              "components":{
                "schemas":{
                    "{{schemaName}}":{
                        "$schema":"http://json-schema.org/draft-04/schema#"
                    }
                }
              }
            }
            """);

        context.Preprocessors = new Preprocessors(
            new Dictionary<Type, Delegate[]>
            {
                [typeof(AsyncApiDocument)] = [dlgt],
            });

        var gen = (FakeContentGenerator)await FakeContentGenerator.CreateAsync(context);

        var apiDocument = gen.Document;

        Assert.NotNull(apiDocument);
        Assert.That(apiDocument?.Components?.Schemas, Does.ContainKey(schemaName));
        var sch = apiDocument?.Components?.Schemas?[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
        Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"properties\":{\"processedModel\":{}}}"));
    }

    [TestCase("externalRef.json")]
    [TestCase("externalRef.yaml")]
    public async Task LoadApiDocument_WithExternalRef(string documentPath)
    {
        var settingsJson = new JObject();
        var context = CreateContext(settingsJson);
        context.DocumentReader = await TestHelpers.LoadApiDocumentAsync(documentPath);
        context.DocumentPath = documentPath;

        var contentGenerator = (FakeContentGenerator)await FakeContentGenerator.CreateAsync(context);

        var document = contentGenerator.Document;

        Assert.NotNull((document.Components?.Messages?["lightMeasured"] as IJsonReference)?.Reference);
    }

    private static Func<Type, Newtonsoft.Json.JsonSerializer?, IReadOnlyDictionary<string, string>?, object?> GetSettingsFactory(string json)
        => (t, s, v) => (s ?? new()).Deserialize(new StringReader(json), t);

    private GeneratorContext CreateContext(JObject settingsJson, Core.ExtensionManager.Extensions? extension = null)
    {
        extension ??= new();
        return new GeneratorContext(
            (t, s, _) => settingsJson.ToObject(t, s ?? new()),
            extension,
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()))
        {
            DocumentReader = new StringReader("""
            {
                "asyncapi":"3.0.0",
                "info":{
                    "title": "",
                    "version": "1.0"
                }
            }
            """),
        };
    }
}

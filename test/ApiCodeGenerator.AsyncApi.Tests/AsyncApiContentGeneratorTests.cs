using System.Collections.ObjectModel;
using ApiCodeGenerator.AsyncApi.DOM;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Constraints;

namespace ApiCodeGenerator.AsyncApi.Tests;

public class AsyncApiContentGeneratorTests
{
    [TestCase("asyncapi.json")]
    [TestCase("asyncapi.yml")]
    public async Task LoadApiDocument(string fileName)
    {
        var extensions = new Core.ExtensionManager.Extensions();

        var context = new GeneratorContext(
            settingsFactory: (t, s, v) => null,
            extensions,
            variables: new Dictionary<string, string>())
        {
            DocumentReader = await TestHelpers.LoadApiDocumentAsync(fileName),
        };

        var contentGenerator = (FakeContentGenerator)await FakeContentGenerator.CreateAsync(context);

        ValidateDocument(contentGenerator.Document);
    }

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
        Assert.IsInstanceOf<OperationNameGenerators.MultipleClientsFromFirstTagAndOperationId>(settings.OperationNameGenerator);
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
        Assert.That(apiDocument?.Components?.Schemas, Does.ContainKey(schemaName));
        var sch = apiDocument?.Components?.Schemas[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
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
        Assert.That(apiDocument?.Components?.Schemas, Does.ContainKey(schemaName));
        var sch = apiDocument?.Components?.Schemas[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
        Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"processed\":{}}"));
        logger.Verify(l => l.LogWarning(filePath, It.IsAny<string>()));
    }

    [Test]
    public async Task LoadApiDocument_WithModelPreprocess()
    {
        const string schemaName = nameof(schemaName);
        var settingsJson = new JObject();

        Func<AsyncApiDocument, string?, AsyncApiDocument> dlgt = new FakeModelPreprocessor("{}").Process;

        var context = CreateContext(settingsJson);
        context.DocumentReader = new StringReader("{\"components\":{\"schemas\":{\"" + schemaName + "\":{\"$schema\":\"http://json-schema.org/draft-04/schema#\"}}}}");

        context.Preprocessors = new Preprocessors(
            new Dictionary<Type, Delegate[]>
            {
                [typeof(AsyncApiDocument)] = [dlgt],
            });

        var gen = (FakeContentGenerator)await FakeContentGenerator.CreateAsync(context);

        var apiDocument = gen.Document;

        Assert.NotNull(apiDocument);
        Assert.That(apiDocument?.Components?.Schemas, Does.ContainKey(schemaName));
        var sch = apiDocument?.Components?.Schemas[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
        Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"properties\":{\"processedModel\":{}}}"));
    }

    private static Func<Type, Newtonsoft.Json.JsonSerializer?, IReadOnlyDictionary<string, string>?, object?> GetSettingsFactory(string json)
        => (t, s, v) => (s ?? new()).Deserialize(new StringReader(json), t);

    private void ValidateDocument(AsyncApiDocument document)
    {
        Assert.NotNull(document);
        Assert.AreEqual("Streetlights Kafka API", document.Info?.Title);

        const string channelPrefix = "smartylighting.streetlights.1.0.";
        Assert.That(document.Channels,
            Is.Not.Null
            .And.ContainKey(channelPrefix + "event.{streetlightId}.lighting.measured")
            .And.ContainKey(channelPrefix + "action.{streetlightId}.turn.on")
            .And.ContainKey(channelPrefix + "action.{streetlightId}.turn.off")
            .And.ContainKey(channelPrefix + "action.{streetlightId}.dim"));

        Assert.NotNull(document.Components);
        Assert.That(document.Components.Messages,
            Is.Not.Null
            .And.ContainKey("lightMeasured")
            .And.ContainKey("turnOnOff")
            .And.ContainKey("dimLight"));

        Assert.That(document.Components.Parameters,
            Is.Not.Null
            .And.ContainKey("streetlightId"));

        Assert.That(document.Components.Schemas,
            Is.Not.Null
            .And.ContainKey("lightMeasuredPayload")
            .And.ContainKey("turnOnOffPayload")
            .And.ContainKey("dimLightPayload")
            .And.ContainKey("sentAt"));

        Assert.That(document.Servers,
            Is.Not.Null
            .And.ContainKey("scram-connections")
            .And.ContainKey("mtls-connections"));

        // Resolve $ref in channel defintion
        var actualChannel = document.Channels[channelPrefix + "event.{streetlightId}.lighting.measured"];
        Assert.That(actualChannel,
            Is.Not.Null
            .And.Property("Publish").Not.Null
            .And.Property("Subscribe").Null);
        Assert.That(actualChannel.Parameters,
            Is.Not.Null
            .And.ContainKey("streetlightId"));
        Assert.That(actualChannel.Parameters["streetlightId"],
            Is.Not.Null
            .And.Property("ReferencePath").EqualTo("#/components/parameters/streetlightId")
            .And.Property("Reference").EqualTo(document.Components.Parameters["streetlightId"]));
        Assert.That(actualChannel.Publish?.Message,
            Is.Not.Null
            .And.Property("ReferencePath").EqualTo("#/components/messages/lightMeasured")
            .And.Property("Reference").EqualTo(document.Components.Messages["lightMeasured"]));

        // Resolve $ref in message definition
        var actualMessage = document.Components.Messages["turnOnOff"];
        Assert.That(actualMessage, Is.Not.Null);
        Assert.That(actualMessage.Payload,
            Is.Not.Null
            .And.Property("Reference").EqualTo(document.Components.Schemas["turnOnOffPayload"]));

        // Resolve $ref in schema definition
        Assert.That(document.Components.Schemas["turnOnOffPayload"]?.ActualProperties,
            Is.Not.Null
            .And.ContainKey("command"));

        //Read server object
        Assert.That(document.Servers["scram-connections"],
            Is.Not.Null
            .And.Property("Url").EqualTo("test.mykafkacluster.org:18092")
            .And.Property("Protocol").EqualTo("kafka-secure")
            .And.Property("Description").EqualTo("Test broker secured with scramSha256"));

        // Resolve $ref in servers
        Assert.That(document.Servers["mtls-connections"],
            Is.Not.Null
            .And.Property("Reference").EqualTo(document.Components.Servers["mtls-connections"]));

        // Resolve $ref in server variables
        Assert.Multiple(() =>
        {
            var variables = document.Components.Servers["mtls-connections"].Variables;
            Assert.That(variables,
                Is.Not.Null
             .And.ContainKey("someRefVariable")
             .And.ContainKey("someVariable"));

            Assert.That(variables!["someRefVariable"],
                Is.Not.Null
                .And.Property("Reference").EqualTo(document.Components.ServerVariables["someRefVariable"]));
        });

        //Read server variables
        Assert.That(document.Components.ServerVariables["someRefVariable"],
        new PredicateConstraint<ServerVariable>(a =>
            a.Description == "Some ref variable"
            && a.Enum?.FirstOrDefault() == "def"
            && a.Default == "def"
            && a.Examples?.FirstOrDefault() == "exam"));
    }

    private GeneratorContext CreateContext(JObject settingsJson, Core.ExtensionManager.Extensions? extension = null)
    {
        extension ??= new();
        return new GeneratorContext(
            (t, s, _) => settingsJson.ToObject(t, s ?? new()),
            extension,
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()))
        {
            DocumentReader = new StringReader("{}"),
        };
    }
}

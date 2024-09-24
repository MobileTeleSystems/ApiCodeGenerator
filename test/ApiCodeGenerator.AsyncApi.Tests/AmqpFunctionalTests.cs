using ApiCodeGenerator.AsyncApi.Amqp.CSharp;
using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ApiCodeGenerator.AsyncApi.Tests.Infrastructure.TestHelpers;

namespace ApiCodeGenerator.AsyncApi.Tests;

public class AmqpFunctionalTests
{
    [Test]
    public async Task Generate()
    {
        const string ns = "MyNS";
        const string className = "LightinService";

        var settings = new CSharpAmqpServiceGeneratorSettings
        {
            ClassName = className,
            CSharpGeneratorSettings =
            {
                Namespace = ns,
                GenerateDataAnnotations = false,
            },
            OperationTypes = OperationTypes.Publish,
        };

        var context = new GeneratorContext((t, s, v) => settings, new Core.ExtensionManager.Extensions(), new Dictionary<string, string>())
        {
            DocumentReader = await LoadApiDocumentAsync("asyncapi.json"),
        };

        var generator = await CSharpAmqpContentGenerator.CreateAsync(context);

        // Act
        var code = generator.Generate();

        // Assert
        var expectedCode = GetExpectedCode(
            GetExpectedAmqpServiceCode(className, identCnt: 4) + "\n" +
                GetExpectedPoolCode(className, identCnt: 4) + "\n",
            GetExpectedDtoCode(),
            ns,
            GetAmqpUsings() + "\n");

        Assert.AreEqual(expectedCode, code);
    }

    [Test]
    public async Task Generate_ChannelBindingPublisher()
    {
        const string ns = "MyNS";
        const string className = "LightinService";

        var settings = new CSharpAmqpServiceGeneratorSettings
        {
            ClassName = className,
            CSharpGeneratorSettings =
            {
                Namespace = ns,
                GenerateDataAnnotations = false,
            },
            OperationTypes = OperationTypes.Publish,
        };
        var channelBinding = new ChannelBindings
        {
            Amqp = new()
            {
                Is = ChannelType.RoutingKey,
                Exchange = new()
                {
                    Name = "exName",
                    Type = ExchangeType.Topic,
                    Durable = true,
                    AutoDelete = true,
                },
                Queue = new()
                {
                    AdditionalProperties =
                    {
                        ["x-prefetch-count"] = 1,
                        ["x-confirm"] = true,
                    },
                },
            },
        };

        var documentReader = await LoadApiDocumentAsync("asyncapi.json");
        var json = JToken.ReadFrom(new JsonTextReader(documentReader));
        ((JProperty)json["channels"]!.First!).Value["bindings"] = JObject.FromObject(channelBinding);

        var context = new GeneratorContext((t, s, v) => settings, new Core.ExtensionManager.Extensions(), new Dictionary<string, string>())
        {
            DocumentReader = new StringReader(json.ToString()),
        };

        var generator = await CSharpAmqpContentGenerator.CreateAsync(context);

        // Act
        var code = generator.Generate();

        // Assert
        string[] expectedPublisherCode = [
                GetExpectedSummary("Inform about environmental lighting conditions of a particular streetlight.", 8) +
                  GetExpectedAmqpPublisherCode("ReceiveLightMeasurement", "LightMeasuredPayload", identCnt: 8, channelBinding.Amqp.Exchange)
            ];
        var expectedCode = GetExpectedCode(
            GetExpectedAmqpServiceCode(className, identCnt: 4, expectedPublisherCode) + "\n" +
                GetExpectedPoolCode(className, identCnt: 4) + "\n",
            GetExpectedDtoCode(),
            ns,
            GetAmqpUsings() + "\n");

        Assert.AreEqual(expectedCode, code);
    }

    [Test]
    public async Task Generate_ChannelBindingSubscriber()
    {
        const string ns = "MyNS";
        const string className = "LightinService";

        var settings = new CSharpAmqpServiceGeneratorSettings
        {
            ClassName = className,
            CSharpGeneratorSettings =
            {
                Namespace = ns,
                GenerateDataAnnotations = false,
            },
            OperationTypes = OperationTypes.Subscribe,
        };
        var channelBinding = new ChannelBindings
        {
            Amqp = new()
            {
                Is = ChannelType.RoutingKey,
                Exchange = new()
                {
                    Name = "exName",
                    Type = ExchangeType.Topic,
                },
                Queue = new()
                {
                    Name = "quName",
                    AutoDelete = true,
                    Durable = true,
                    AdditionalProperties =
                    {
                        ["x-prefetch-count"] = 1,
                        ["x-confirm"] = true,
                    },
                },
            },
        };

        var documentReader = await LoadApiDocumentAsync("asyncapi.json");
        var json = JToken.ReadFrom(new JsonTextReader(documentReader));
        var channelProperty = (JProperty)json["channels"]!.Last!;
        channelProperty.Value["bindings"] = JObject.FromObject(channelBinding); // add binding declaration
        json["channels"] = new JObject { channelProperty }; // for test use only last channel

        var context = new GeneratorContext((t, s, v) => settings, new Core.ExtensionManager.Extensions(), new Dictionary<string, string>())
        {
            DocumentReader = new StringReader(json.ToString()),
        };

        var generator = await CSharpAmqpContentGenerator.CreateAsync(context);

        // Act
        var code = generator.Generate();

        // Assert
        var document = GetDocument(generator);
        var channel = document.Channels!.Values.First();
        string[] expectedSubscriberCode = [
                GetExpectedAmqpSubscriberCode("DimLight", "DimLightPayload", identCnt: 8, channelBinding.Amqp)
            ];
        var expectedCode = GetExpectedCode(
            GetExpectedAmqpServiceCode(className, identCnt: 4, expectedSubscriberCode) + "\n" +
                GetExpectedPoolCode(className, identCnt: 4, channel) + "\n",
            GetExpectedDtoCode(),
            ns,
            GetAmqpUsings() + "\n");

        Assert.AreEqual(expectedCode, code);
    }

    [Test]
    public async Task Generate_OperationBinding()
    {
        const string ns = "MyNS";
        const string className = "LightinService";

        var settings = new CSharpAmqpServiceGeneratorSettings
        {
            ClassName = className,
            CSharpGeneratorSettings =
            {
                Namespace = ns,
                GenerateDataAnnotations = false,
            },
            OperationTypes = OperationTypes.Publish,
        };

        var operationBinding = new OperationV0_3
        {
            BindingVersion = "latest",
            Bcc = ["Bcc"],
            Cc = ["CC"],
            DeliveryMode = DeliviryMode.Transient,
            Expiration = 100,
            Mandatory = true,
            Priority = 1,
        };

        var documentReader = await LoadApiDocumentAsync("asyncapi.json");
        var json = JToken.ReadFrom(new JsonTextReader(documentReader));
        var channelJson = ((JProperty)json["channels"]!.First!).Value;
        channelJson["publish"]!["bindings"] = JObject.FromObject(new OperationBindings { Amqp = operationBinding });

        var context = new GeneratorContext((t, s, v) => settings, new Core.ExtensionManager.Extensions(), new Dictionary<string, string>())
        {
            DocumentReader = new StringReader(json.ToString()),
        };

        var generator = await CSharpAmqpContentGenerator.CreateAsync(context);

        // Act
        var code = generator.Generate();

        // Assert
        string[] expectedPublisherCode = [
                GetExpectedSummary("Inform about environmental lighting conditions of a particular streetlight.", 8) +
                  GetExpectedAmqpPublisherCode("ReceiveLightMeasurement", "LightMeasuredPayload", identCnt: 8, operationBinding: operationBinding)
            ];
        var expectedCode = GetExpectedCode(
            GetExpectedAmqpServiceCode(className, identCnt: 4, expectedPublisherCode) + "\n" +
                GetExpectedPoolCode(className, identCnt: 4) + "\n",
            GetExpectedDtoCode(),
            ns,
            GetAmqpUsings() + "\n");

        Assert.AreEqual(expectedCode, code);
    }

    private static AsyncApiDocument GetDocument(IContentGenerator contentGenerator)
    {
        var generator = contentGenerator.GetType()
            .GetProperty("Generator", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(contentGenerator)!;
        return (AsyncApiDocument)generator.GetType()
            .GetProperty("Document", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(generator)!;
    }
}

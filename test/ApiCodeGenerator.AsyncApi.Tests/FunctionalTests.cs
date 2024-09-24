using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ApiCodeGenerator.AsyncApi.Tests.Infrastructure.TestHelpers;

namespace ApiCodeGenerator.AsyncApi.Tests;

public class FunctionalTests
{
    [Test]
    public Task GenerateDtoTypes()
    {
        CSharpClientGeneratorSettings settings = new()
        {
            CSharpGeneratorSettings =
            {
                Namespace = "TestNS",
                GenerateDataAnnotations = false,
            },
            GenerateClientClasses = false,
            GenerateDtoTypes = true,
        };
        return RunTest(settings, string.Empty, "asyncapi.json", GetExpectedDtoCode());
    }

    [Test]
    public Task GenerateClient()
    {
        const string className = "StreetlightApi";
        CSharpClientGeneratorSettings settings = new()
        {
            CSharpGeneratorSettings =
            {
                Namespace = "TestNS",
                GenerateDataAnnotations = false,
            },
            ClassName = className,
            GenerateClientClasses = true,
            GenerateDtoTypes = false,
        };

        var expectedOperationsCode = GetExpectedOperationsCode([]);
        var expectedCode = GetExpectedClientCode(className, string.Join("\n", expectedOperationsCode) + "\n");
        return RunTest(settings, expectedCode, "asyncapi.json", string.Empty);
    }

    [Test]
    public async Task GenerateInlineDto()
    {
        const string className = "StreetlightApi";
        CSharpClientGeneratorSettings settings = new()
        {
            CSharpGeneratorSettings =
            {
                Namespace = "TestNS",
                GenerateDataAnnotations = false,
            },
            ClassName = className,
            GenerateClientClasses = true,
            GenerateDtoTypes = true,
        };

        using var docReader = new JsonTextReader(await TestHelpers.LoadApiDocumentAsync("asyncapi.json"));
        var jDoc = JObject.ReadFrom(docReader);

        // удаляем тип из components/schema и объявляем его в свойстве сообщения (используем последний тип в списке чтоб не иметь проблем с порядком  Дто)
        var payloadSchema = jDoc["components"]!["schemas"]!["dimLightPayload"]!;
        payloadSchema.Parent!.Remove();
        jDoc["components"]!["messages"]!["dimLight"]!["payload"] = payloadSchema;

        var docJson = jDoc.ToString(Formatting.None);
        jDoc = null;

        var expectedOperationsCode = GetExpectedOperationsCode([]);
        var expectedCode = GetExpectedClientCode(className, string.Join("\n", expectedOperationsCode) + "\n") + "\n";

        await RunTest(settings, expectedCode, new StringReader(docJson), GetExpectedDtoCode());
    }

    [Test]
    public async Task GenerateInterface()
    {
        const string className = "StreetlightApi";
        CSharpClientGeneratorSettings settings = new()
        {
            CSharpGeneratorSettings =
            {
                Namespace = "TestNS",
                GenerateDataAnnotations = false,
            },
            ClassName = className,
            GenerateClientClasses = true,
            GenerateDtoTypes = false,
            GenerateClientInterfaces = true,
        };

        var expectedOperationsCode = GetExpectedOperationsCode(null);

        var expectedInterfaceCode = GetExpectedClientCode('I' + className, string.Join("\n", expectedOperationsCode) + "\n", typeKind: "interface") + "\n";

        expectedOperationsCode = GetExpectedOperationsCode([]);

        var expectedCode = GetExpectedClientCode(className, string.Join("\n", expectedOperationsCode) + "\n", baseType: "I" + className);

        await RunTest(settings, expectedInterfaceCode + expectedCode, "asyncapi.json", string.Empty);
    }

    [Test]
    public async Task ReplaceCharsInNames()
    {
        const string className = "StreetlightApi";
        var settingsJson = $$"""
        {
            "Namespace": "TestNS",
            "GenerateDataAnnotations": false,
            "ClassName": "{{className}}",
            "GenerateClientClasses": true,
            "GenerateDtoTypes": false,
            "GenerateClientInterfaces": true,
            "replaceNameCollection": {"I":"_"}
        }
        """;
        var generationContext = TestHelpers.CreateContext(settingsJson, "asyncapi.json");

        var generator = await CSharpClientContentGenerator.CreateAsync(generationContext);

        var actual = generator.Generate();

        var expectedClassCode = GetExpectedClientCode(className, string.Join("\n", GetExpectedOperationsCode([])) + "\n", 4, "I" + className);
        var expectedInterfaceCode = GetExpectedClientCode("I" + className, string.Join("\n", GetExpectedOperationsCode(null)) + "\n", 4, typeKind: "interface");

        var expected = GetExpectedCode(expectedInterfaceCode + "\n" + expectedClassCode + "\n", null)
            .Replace("string streetlightId", "string streetlight_d")
            ;

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task GenerateMultipleClients()
    {
        const string className = "StreetlightApi";
        var settingsJson = $$"""
        {
            "Namespace": "TestNS",
            "GenerateDataAnnotations": false,
            "ClassName": "{{className}}{controllerName}",
            "GenerateClientClasses": true,
            "GenerateDtoTypes": false,
            "GenerateClientInterfaces": true,
            "OperationGenerationMode": "MultipleClientsFromFirstTagAndOperationId"
        }
        """;
        var generationContext = TestHelpers.CreateContext(settingsJson, "asyncapi.json");

        var generator = await CSharpClientContentGenerator.CreateAsync(generationContext);

        var actual = generator.Generate();

        var clsExpOpersCode = GetExpectedOperationsCode([]);
        var expClsCode1 = GetExpectedClientCode(className + "pub", string.Join("\n", clsExpOpersCode[..1]) + "\n", baseType: "I" + className + "pub");
        var expClsCode2 = GetExpectedClientCode(className + "sub", string.Join("\n", clsExpOpersCode[1..]) + "\n", baseType: "I" + className + "sub");

        var intExpOpersCode = GetExpectedOperationsCode(null);
        var expIntCode1 = GetExpectedClientCode($"I{className}pub", string.Join("\n", intExpOpersCode[..1]) + "\n", typeKind: "interface");
        var expIntCode2 = GetExpectedClientCode($"I{className}sub", string.Join("\n", intExpOpersCode[1..]) + "\n", typeKind: "interface");

        var expected = GetExpectedCode(string.Join("\n", [expIntCode1, expClsCode1, expIntCode2, expClsCode2]) + "\n", null);

        Assert.AreEqual(expected, actual);
    }

    private static string[] GetExpectedOperationsCode(string[]? bodyLines) => [
                TestHelpers.GetExpectedSummary("Inform about environmental lighting conditions of a particular streetlight.", 4 + 4) +
                GetExpectedPublisherCode("ReceiveLightMeasurement", "LightMeasuredPayload", 4 + 4, bodyLines),
            GetExpectedSubscriberCode("TurnOn", "TurnOnOffPayload", 4 + 4, bodyLines),
            GetExpectedSubscriberCode("TurnOff", "TurnOnOffPayload", 4 + 4, bodyLines),
            GetExpectedSubscriberCode("DimLight", "DimLightPayload", 4 + 4, bodyLines),
        ];
}

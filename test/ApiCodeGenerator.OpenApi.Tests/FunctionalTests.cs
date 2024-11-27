using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiCodeGenerator.Core;
using ApiCodeGenerator.Core.NswagDocument;
using Newtonsoft.Json.Linq;
using static ApiCodeGenerator.OpenApi.Tests.Infrastructure.TestHelpers;

namespace ApiCodeGenerator.OpenApi.Tests
{
    public class FunctionalTests
    {
        // Проверяем что при указании подмены символов работает ка ожидалось
        [TestCaseSource(nameof(TestCaseSource))]
        public async Task ReplaceCharsInNames_On(string schema, string replaceParams, string expectedInterface, string expectedDto)
        {
            // Arrange
            // Для уменьшения кол-ва генрируемого (проверяемого) кода отключаем часть опций
            var settingsJson = $@"{{
                ""replaceNameCollection"":{{{replaceParams}}},
                ""generateClientInterfaces"":true,
                ""generateExceptionClasses"":false,
                ""generateOptionalParameters"":true}}";

            string? generatedText = null;
            var fileProviderMock = new Mock<IFileProvider>(MockBehavior.Strict);
            fileProviderMock.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            fileProviderMock.Setup(p => p.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, content) => generatedText = content);

            var task = CreateGenerator(fileProviderMock, settingsJson, schema);

            // Act
            await task.ExecuteAsync("nswag", "in", "out");

            // Assert
            var expected = GetExpectedCode(expectedInterface, testOperResponseText: expectedDto, @namespace: "MyNamespace");
            var startText = expected.Substring(0, expected.IndexOf("$stop$"));
            var endText = expected.Substring(expected.IndexOf("$stop$") + 6);

            // Проверяем только описание интерфейса (в начале сгенерированого) и класса ответа (в конце сгенрированного)
            Assert.That(generatedText, Does
                .StartWith(startText)
                .And
                .EndsWith(endText));
        }

        // проверка генерации по JsonSchema
        [Test]
        public async Task GnerateByJsonSchema()
        {
            var settingsJson = "{\"Namespace\":\"TestNS\"}";

            string? generatedText = null;
            var fileProviderMock = new Mock<IFileProvider>(MockBehavior.Strict);
            fileProviderMock.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            fileProviderMock.Setup(p => p.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((path, content) => generatedText = content);

            var nswagDocument = new NswagDocument()
            {
                DocumentGenerator =
                {
                    JsonSchemaToOpenApi = new()
                    {
                        Name = "TestOperResponse",
                        Schema = File.ReadAllText(GetSwaggerPath("jsonSchema.json")),
                    },
                },
                CodeGenerators = { ["generator"] = JObject.Parse(settingsJson) },
            };

            var documentFactoryMock = new Mock<INswagDocumentFactory>(MockBehavior.Strict);
            documentFactoryMock.Setup(f => f.LoadNswagDocument(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<JObject?>()))
                .Returns(nswagDocument);

            var task = new GenerationTask(
                new Core.ExtensionManager.Extensions(
                    new Dictionary<string, ContentGeneratorFactory>() { ["generator"] = CSharpClientContentGenerator.CreateAsync }),
                documentFactoryMock.Object,
                new ApiDocumentProvider(),
                fileProviderMock.Object,
                Mock.Of<ILogger>());

            var expectedCode = GetExpectedCode(null, "    \n\n" + TestOperResponseText + "\n");

            // Act
            await task.ExecuteAsync("nswag", "in", "out");

            // Assert
            Assert.AreEqual(expectedCode, generatedText);
        }

        [Test]
        public async Task GenerateClientInterface_SingleClientFromLastSegmentOfOperationId()
        {
            //Arrange
            var settings = new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true,
                GenerateDtoTypes = false,
                GenerateOptionalParameters = true,
                OperationNameGenerator = new SingleClientFromLastSegmentOperationIdOperationNameGenerator(),
            };
            settings.CSharpGeneratorSettings.Namespace = "TestNS";

            var expectedClientDeclartion =
                "    public partial interface IClient\n" +
                "    {\n" +
                "\n" +
                "        /// <param name=\"cancellationToken\">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>\n" +
                "        /// <summary>\n" +
                "        /// Test operation\n" +
                "        /// </summary>\n" +
                "        /// <returns>Запрос успешно принят</returns>\n" +
                "        /// <exception cref=\"ApiException\">A server side error occurred.</exception>\n" +
                "        System.Threading.Tasks.Task<TestOperResponse> GetTestOperUsingGETAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));\n" +
                "\n" +
                "    }\n";

            var generator = new CSharpClientGenerator(await LoadSchemaAsync("OperNamingFromLastOperSeg.json"), settings);

            //Act
            var actual = generator.GenerateFile();

            //Assert
            Assert.That(actual, Does.Contain(expectedClientDeclartion));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:Static elements should appear before instance elements", Justification = "After tests")]
        public static IEnumerable<TestCaseData> TestCaseSource()
        {
            const string schemaName = "ReplaceChars.json";
            const string expectedInterfaceTemplate =
               "    public partial interface IClient\n" +
               "    {{\n" +
               "\n" +
               "        /// <param name=\"cancellationToken\">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>\n" +
               "        /// <summary>\n" +
               "        /// Test operation\n" +
               "        /// </summary>\n" +
               "        /// <param name=\"{0}\">Параметр пердаваемый в качестве заголовка</param>\n" +
               "        /// <param name=\"queryParametr\">Параметр передаваемый в строке запроса</param>\n" +
               "        /// <returns>Запрос успешно принят</returns>\n" +
               "        /// <exception cref=\"ApiException\">A server side error occurred.</exception>\n" +
               "        System.Threading.Tasks.Task<TestOperResponse> GetTestOperUsingGETAsync(string {0}, int? queryParametr = null, ComplexType complexParametr = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));\n" +
               "\n" +
               "    }}$stop$\n";
            const string expectedDtoTemplate =
                "    " + GENERATED_CODE + "\n" +
                "    public partial class TestOperResponse\n" +
                "    {{\n" +
                "        [Newtonsoft.Json.JsonProperty(\"{1}\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
                "        public int? {0} {{ get; set; }}\n" +
                "\n" +
                "        [Newtonsoft.Json.JsonProperty(\"complex\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
                "        public ComplexType Complex {{ get; set; }}\n" +
                "\n" +
                "    }}\n" +
                "\n" +
                "    " + GENERATED_CODE + "\n" +
                "    public partial class ComplexType\n" +
                "    {{\n" +
                "        [Newtonsoft.Json.JsonProperty(\"{1}\", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]\n" +
                "        public int? {0} {{ get; set; }}\n" +
                "\n" +
                "    }}" +
                "\n";

            var schema = File.ReadAllText(GetSwaggerPath(schemaName));

            // Case: Замена произвольного символа
            var replaceParmas = "\"d\":\"_\"";
            var expectedInterface = string.Format(expectedInterfaceTemplate, "hea_erParametr");
            var expectedDto = string.Format(expectedDtoTemplate, "I_", "@id");

            yield return new TestCaseData(schema, replaceParmas, expectedInterface, expectedDto)
                .SetName($"{{m}}(\"{replaceParmas}\")");

            // Case: Замена символов которые умеет заменять Nswag
            replaceParmas = "\"@\":\"_\"";
            expectedInterface = string.Format(expectedInterfaceTemplate, "_headerParametr");
            expectedDto = string.Format(expectedDtoTemplate, "_id", "@id");

            yield return new TestCaseData(schema, replaceParmas, expectedInterface, expectedDto)
                .SetName($"{{m}}(\"{replaceParmas}\")");

            // Case: Стандартный механизм Nswag
            replaceParmas = string.Empty;
            expectedInterface = string.Format(expectedInterfaceTemplate, "headerParametr");
            expectedDto = string.Format(expectedDtoTemplate, "Id", "@id");

            yield return new TestCaseData(schema, replaceParmas, expectedInterface, expectedDto)
                .SetName($"{{m}}(\"{replaceParmas}\")");
        }

        private static GenerationTask CreateGenerator(Mock<IFileProvider> fileProviderMock, string settingsJson, string schema)
        {
            var settings = JObject.Parse(settingsJson);
            var nswagDocument = new NswagDocument()
            {
                CodeGenerators = { ["generator"] = settings },
            };
            Func<Task<GetDocumentReaderResult?>> loadOpenApiDocument = ()
                => Task.FromResult<GetDocumentReaderResult?>(GetDocumentReaderResult.Success(new StringReader(schema), null));
            var documentFactory = Mock.Of<INswagDocumentFactory>(f =>
                f.LoadNswagDocument(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>(), null) == nswagDocument,
                MockBehavior.Strict);

            var apiDocumentProvider = Mock.Of<IApiDocumentProvider>(l =>
                l.GetDocumentReaderAsync(It.IsAny<DocumentGenerator>())
                    == loadOpenApiDocument());

            return new GenerationTask(
                new Core.ExtensionManager.Extensions(
                    new Dictionary<string, ContentGeneratorFactory>() { ["generator"] = CSharpClientContentGenerator.CreateAsync }),
                documentFactory,
                apiDocumentProvider,
                fileProviderMock.Object,
                Mock.Of<ILogger>());
        }
    }
}

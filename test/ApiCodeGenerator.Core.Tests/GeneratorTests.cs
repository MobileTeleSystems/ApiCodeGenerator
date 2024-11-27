using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.Core.ExtensionManager;
using ApiCodeGenerator.Core.Tests.Infrastructure;
using Newtonsoft.Json.Linq;
using VariablesDict = System.Collections.Generic.IReadOnlyDictionary<string, string>;

namespace ApiCodeGenerator.Core.Tests
{
    public class GeneratorTests
    {
        private const string NOT_SET = "notset";
        private const string OutFilePath = "outFile.g.cs";
        private readonly IExtensions _extensionsEmpty = ExtensionLoader.LoadExtensions(new string[0]);
        private Mock<IFileProvider> _fileProviderMock = null!;

        [SetUp]
        public void TestInit()
        {
            _fileProviderMock = new();
            _fileProviderMock.Setup(fp => fp.Exists(It.Is<string>(v => v != "notExists.nswag")))
                .Returns(true);
            _fileProviderMock.Setup(fp => fp.Exists(It.Is<string>(v => v == "notExists.nswag")))
                .Returns(false);
            _fileProviderMock.Setup(fp => fp.OpenRead(It.Is<string>(v => v == "empty.nswag")))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("{}")));
            _fileProviderMock.Setup(fp => fp.OpenRead(It.Is<string>(v => v == "emptyDocumentGenerator.nswag")))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("{ \"documentGenerator\": {}}")));
            _fileProviderMock.Setup(fp => fp.OpenRead(It.Is<string>(v => v == "emptyDocumentGenerator2.nswag")))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("{ \"documentGenerator\": null}")));
        }

        // обработка ситуации неорректного пути к файлу nswag
        [Test]
        public async Task NswagFileNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            const string nswagFilePath = "notExists.nswag";
            var task = new GenerationTask(_extensionsEmpty, new NswagDocumentFactory(), null, _fileProviderMock.Object, loggerMock.Object);

            // Act
            var res = await task.ExecuteAsync(
                nswagFilePath: nswagFilePath,
                apiDocumentPath: NOT_SET,
                outFilePath: NOT_SET);

            // Assert
            Assert.False(res);
            loggerMock.Verify(l => l.LogError(LogCodes.FileNotFound, It.IsAny<string>(), "File '{0}' not found.", nswagFilePath));
        }

        // разбор переменных
        [Test]
        public async Task ParseVariables()
        {
            //Arrange
            const string OutFilePath = "Generated.cs";
            var loggerMock = new Mock<ILogger>();
            var documentFactoryMock = new Mock<INswagDocumentFactory>();
            VariablesDict? variables = null;
            documentFactoryMock.Setup(df => df.LoadNswagDocument(It.IsAny<string>(), It.IsAny<VariablesDict>(), null))
                .Callback<string, VariablesDict, JObject?>((p, v, b) => variables = v)
                .Returns(new NswagDocument.NswagDocument());

            var task = new GenerationTask(_extensionsEmpty, documentFactoryMock.Object, null, _fileProviderMock.Object, loggerMock.Object);

            //Act
            _ = await task.ExecuteAsync(
                nswagFilePath: "exists.nswag",
                apiDocumentPath: NOT_SET,
                outFilePath: OutFilePath,
                variables: "TestVar=TestValue,TestVar2 = TestValue2, TestVar_3 =TestValue3");

            //Assert
            Assert.NotNull(variables);
            loggerMock.Verify(be => be.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never());
            Assert.That(variables,
                Does.ContainKey("TestVar").WithValue("TestValue")
                .And.ContainKey("TestVar2").WithValue("TestValue2")
                .And.ContainKey("TestVar_3").WithValue("TestValue3")
                .And.ContainKey("InputJson").WithValue(NOT_SET)
                .And.ContainKey("OutFile").WithValue(OutFilePath)
                .And.Count.EqualTo(5));
        }

        // Пропуск генерации если не установлен файл документа Api
        [TestCase("empty.nswag")]
        [TestCase("emptyDocumentGenerator.nswag")]
        [TestCase("emptyDocumentGenerator2.nswag")]
        public async Task SourceJsonNotSet(string nswagFileName)
        {
            //Arrange
            var loggerMock = new Mock<ILogger>();
            var expectedError = "Source not set. Skip generation.";
            var documentFactoryMock = NswagDocumentFactoryMock(LoadNswag("{\"codeGenerators\":{\"FakeGenerator\":{}}}"));

            var extensions = new ExtensionManager.Extensions(
                FakeGenerator());
            var apiDocumentProvider = ApiDocumentProvider(null);
            var task = new GenerationTask(extensions, documentFactoryMock.Object, apiDocumentProvider, _fileProviderMock.Object, loggerMock.Object);

            //Act
            var result = await task.ExecuteAsync(
                nswagFilePath: nswagFileName,
                apiDocumentPath: NOT_SET,
                outFilePath: NOT_SET);

            //Assert
            Assert.True(result);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), nswagFileName, expectedError, It.IsAny<object[]>()), Times.Once());
        }

        // обработка ошибки загрузки документа Api
        [Test]
        public async Task ApiDocumentLoadError()
        {
            //Arrange
            const string OpenApiFilePath = @"notFound.json";
            var loggerMock = new Mock<ILogger>();
            var nswag = "{\"documentGenerator\":{\"fromDocument\":{\"json\":\"$(InputJson)\"}},\"codeGenerators\":{\"FakeGenerator\":{}}}";
            var expectedError = $"Unable load OpenApi document from '{OpenApiFilePath}'";
            var documentFactoryMock = new Mock<INswagDocumentFactory>();
            documentFactoryMock.Setup(df => df.LoadNswagDocument(It.IsAny<string>(), It.IsAny<VariablesDict>(), null))
                .Returns<string, VariablesDict, JObject?>((_, v, _) => LoadNswag(nswag, v));

            var extensions = new ExtensionManager.Extensions(
                FakeGenerator());
            var apiDocumentProvider = ApiDocumentProvider(GetDocumentReaderResult.Failed(expectedError, null));
            var task = new GenerationTask(extensions, documentFactoryMock.Object, apiDocumentProvider, _fileProviderMock.Object, loggerMock.Object);

            //Act
            var result = await task.ExecuteAsync(
                nswagFilePath: @"exists.nswag",
                apiDocumentPath: OpenApiFilePath,
                outFilePath: NOT_SET);

            //Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), expectedError, new string[0]), Times.Once());
        }

        // проверка загрузки генератора содержимого и его последующего вызова.
        [Test]
        public async Task RunCodeGeneration()
        {
            // Arrange
            var nswag = "{\"documentGenerator\":{\"fromDocument\":{\"json\":\"{}\"}}, \"codeGenerators\":{\"FakeGenerator\":{}}}";
            var document = new NswagDocumentFactory().LoadNswagDocument(new StringReader(nswag));

            var loadDocumentResult = GetDocumentReaderResult.Success(new StringReader("{}"), null);
            var nswagDocumentFactory = NswagDocumentFactoryMock(document).Object;

            var generatorMock = new Mock<FakeCodeGenerator>();
            var extensions = new ExtensionManager.Extensions(
                FakeGenerator(gm => generatorMock = Mock.Get(gm)));
            var apiDocumentProvider = ApiDocumentProvider(loadDocumentResult);
            var task = new GenerationTask(extensions, nswagDocumentFactory, apiDocumentProvider, _fileProviderMock.Object, null);

            // Act
            var result = await task.ExecuteAsync(
                nswagFilePath: @"csharp.nswag",
                apiDocumentPath: NOT_SET,
                outFilePath: OutFilePath);

            // Assert
            Assert.True(result);
            Assert.NotNull(generatorMock.Object.Context);
            Assert.NotNull(generatorMock.Object.Context.DocumentReader);
            Assert.NotNull(generatorMock.Object.Context.GetSettings<JObject>(null, null));
            generatorMock.Verify(g => g.Generate(), Times.Once);
            _fileProviderMock.Verify(fp => fp.WriteAllTextAsync(It.Is<string>(v => v == OutFilePath), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RunCSharpCodeGenerationFromJsonSchema()
        {
            // Arrange
            var nswag = "{\"documentGenerator\":{\"jsonSchemaToOpenApi\":{\"name\":\"name\", \"schema\":\"{}\"}}, \"codeGenerators\":{\"FakeGenerator\":{}}}";
            var document = new NswagDocumentFactory().LoadNswagDocument(new StringReader(nswag));
            var generatorMock = new Mock<FakeCodeGenerator>();

            var loadDocumentResult = GetDocumentReaderResult.Success(new StringReader("{}"), null);
            var nswagDocumentFactory = NswagDocumentFactoryMock(document).Object;

            var extensions = new ExtensionManager.Extensions(
                FakeGenerator(gm => generatorMock = Mock.Get(gm)));
            var apiDocumentProvider = ApiDocumentProvider(loadDocumentResult);
            var task = new GenerationTask(extensions, nswagDocumentFactory, apiDocumentProvider, _fileProviderMock.Object, null);

            // Act
            var result = await task.ExecuteAsync(
                nswagFilePath: @"csharp.nswag",
                apiDocumentPath: NOT_SET,
                outFilePath: OutFilePath);

            // Assert
            Assert.True(result);
            generatorMock.Verify(g => g.Generate(), Times.Once);
            _fileProviderMock.Verify(fp => fp.WriteAllTextAsync(It.Is<string>(v => v == OutFilePath), FakeCodeGenerator.FileContent), Times.Once);
        }

        [Test]
        public async Task RunCustomCodeGenerator()
        {
            var loggerMock = new Mock<ILogger>();

            var nswag = "{\"documentGenerator\":{\"jsonSchemaToOpenApi\":{\"name\":\"name\", \"schema\":\"{}\"}}, \"codeGenerators\":{\"FakeGenerator\":{}}}";
            var document = LoadNswag(nswag);

            var loadDocumentResult = GetDocumentReaderResult.Success(new StringReader("{}"), null);
            var nswagDocumentFactory = NswagDocumentFactoryMock(document).Object;
            var extension = new ExtensionManager.Extensions(
                FakeGenerator());
            var apiDocumentProvider = ApiDocumentProvider(loadDocumentResult);

            var task = new GenerationTask(extension, nswagDocumentFactory, apiDocumentProvider, _fileProviderMock.Object, null);

            // Act
            var result = await task.ExecuteAsync(
                nswagFilePath: @"csharp.nswag",
                apiDocumentPath: NOT_SET,
                outFilePath: OutFilePath);

            // Assert
            loggerMock.Verify(be => be.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.True(result);
            var expectedCode = FakeCodeGenerator.FileContent;
            _fileProviderMock.Verify(fp => fp.WriteAllTextAsync(It.Is<string>(v => v == OutFilePath), expectedCode), Times.Once);
        }

        // проверка подмены переменных в настройках
        [Test]
        public async Task UseVariablesInSettings()
        {
            var loggerMock = new Mock<ILogger>();

            const string expectedClassName = nameof(expectedClassName);
            var nswag = "{\"documentGenerator\":{\"jsonSchemaToOpenApi\":{\"name\":\"name\", \"schema\":\"{}\"}}, \"codeGenerators\":{\"FakeGenerator\":{\"className\":\"$(var)\"}}}";
            var document = LoadNswag(nswag);

            var loadDocumentResult = GetDocumentReaderResult.Success(new StringReader("{}"), null);
            var nswagDocumentFactory = NswagDocumentFactoryMock(document).Object;
            object? codeGen = null;
            var extension = new ExtensionManager.Extensions(
                FakeGenerator(gm => codeGen = gm));

            var task = new GenerationTask(
                extension,
                nswagDocumentFactory,
                ApiDocumentProvider(loadDocumentResult),
                _fileProviderMock.Object,
                loggerMock.Object);

            // Act
            var result = await task.ExecuteAsync(
                nswagFilePath: @"csharp.nswag",
                apiDocumentPath: NOT_SET,
                outFilePath: OutFilePath,
                variables: $"var={expectedClassName}");

            // Assert
            loggerMock.Verify(be => be.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.True(result);
            Assert.NotNull(codeGen);
            Assert.IsInstanceOf<FakeCodeGenerator>(codeGen);
            var generator = (FakeCodeGenerator)codeGen!;
            Assert.AreEqual(expectedClassName, generator.Settings?.ClassName);
        }

        // проверка подстановки переменных переданных контент-генератором
        [Test]
        public async Task UseAdditionalVariablesInSettings()
        {
            var loggerMock = new Mock<ILogger>();

            const string expectedClassName = "val1";
            var nswag = "{\"documentGenerator\":{\"jsonSchemaToOpenApi\":{\"name\":\"name\", \"schema\":\"{}\"}}, \"codeGenerators\":{\"FakeGenerator\":{\"className\":\"$(var)\"}}}";
            var document = LoadNswag(nswag);

            var loadDocumentResult = GetDocumentReaderResult.Success(new StringReader("{}"), null);
            var nswagDocumentFactory = NswagDocumentFactoryMock(document).Object;

            var vars = new Dictionary<string, string>
            {
                ["var"] = expectedClassName,
            };
            object? codeGen = null;
            var extension = new ExtensionManager.Extensions(
                FakeGenerator(
                    gm => codeGen = gm,
                    vars));

            var task = new GenerationTask(
                extension,
                nswagDocumentFactory,
                ApiDocumentProvider(loadDocumentResult),
                _fileProviderMock.Object,
                loggerMock.Object);

            // Act
            var result = await task.ExecuteAsync(
                nswagFilePath: @"csharp.nswag",
                apiDocumentPath: NOT_SET,
                outFilePath: OutFilePath);

            // Assert
            loggerMock.Verify(be => be.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.True(result);
            Assert.NotNull(codeGen);
            Assert.IsInstanceOf<FakeCodeGenerator>(codeGen);
            var generator = (FakeCodeGenerator)codeGen!;
            Assert.AreEqual(expectedClassName, generator.Settings?.ClassName);
        }

        private static NswagDocument.NswagDocument LoadNswag(string nswag, VariablesDict? variables = null)
            => new NswagDocumentFactory().LoadNswagDocument(new StringReader(nswag), variables);

        private static IApiDocumentProvider ApiDocumentProvider(GetDocumentReaderResult? result)
            => Mock.Of<IApiDocumentProvider>(l =>
                    l.GetDocumentReaderAsync(It.IsAny<DocumentGenerator>())
                        == Task.FromResult(result));

        private static Dictionary<string, ContentGeneratorFactory> FakeGenerator(
            Action<FakeCodeGenerator>? callback = null,
            Dictionary<string, string>? additionalVars = null)
        =>
            new()
            {
                ["FakeGenerator"] = context =>
                {
                    var instance = new Mock<FakeCodeGenerator>(context, additionalVars) { CallBase = true }.Object;
                    callback?.Invoke(instance);
                    return Task.FromResult<IContentGenerator>(instance);
                },
            };

        private static Mock<INswagDocumentFactory> NswagDocumentFactoryMock(NswagDocument.NswagDocument document)
        => Mock.Get(
            Mock.Of<INswagDocumentFactory>(df =>
                df.LoadNswagDocument(It.IsAny<string>(), It.IsAny<VariablesDict>(), null) == document));
    }
}

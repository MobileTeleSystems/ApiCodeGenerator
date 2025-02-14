﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ApiCodeGenerator.OpenApi.Tests.Infrastructure;
using Newtonsoft.Json.Linq;
using NSwag.CodeGeneration.OperationNameGenerators;
using NUnit.Framework.Internal;

namespace ApiCodeGenerator.OpenApi.Tests
{
    public class ContentGeneratorFactoryTests
    {
        private readonly ReadOnlyDictionary<string, string> _variables = new(new Dictionary<string, string>());

        /// <summary>
        /// Проверяем что настройки вложенные в свойства (CSharpGeneratorSettings,CodeGeneratorSettings) грузятся.
        /// </summary>
        /// <returns>Task.</returns>
        [Test]
        public async Task Load_NestedSettings()
        {
            const string clName = "592d31f7-bfd6-48af-91ca-87b4e91c990f";
            const string namesp = "688a3869-d259-4d12-bd04-602eeacdf09c";
            const string tmpl = "39e9bad3-e9ed-4a0c-9884-a7fb9869f072";

            var settingsJson = JObject.Parse($"{{\"className\": \"{clName}\", \"namespace\":\"{namesp}\", \"templateDirectory\":\"{tmpl}\"}}");
            var context = CreateContext(settingsJson);
            var gen = await CSharpClientContentGenerator.CreateAsync(context);

            Assert.NotNull(gen);
            Assert.IsInstanceOf<CSharpClientContentGenerator>(gen);
            var fakeGen = (CSharpClientContentGenerator)gen!;
            var settings = fakeGen.Generator.Settings;
            Assert.NotNull(settings);
            Assert.AreEqual(clName, settings.ClassName);
            Assert.AreEqual(namesp, settings.CSharpGeneratorSettings.Namespace);
            Assert.AreEqual(tmpl, settings.CodeGeneratorSettings.TemplateDirectory);
        }

        [Test]
        public async Task Load_OperationGenerator()
        {
            var settingsJson = JObject.Parse("{\"operationGenerationMode\": \"SingleClientFromOperationId\"}");

            var extensions = new Core.ExtensionManager.Extensions(operationGenerators: Infrastructure.TestHelpers.GetOperationGenerators());
            var context = CreateContext(settingsJson, extensions);
            var gen = await CSharpClientContentGenerator.CreateAsync(context);

            Assert.NotNull(gen);
            Assert.IsInstanceOf<CSharpClientContentGenerator>(gen);
            var fakeGen = (CSharpClientContentGenerator)gen!;
            var settings = fakeGen.Generator.Settings;
            Assert.NotNull(settings);
            Assert.NotNull(settings.OperationNameGenerator);
            Assert.IsInstanceOf<SingleClientFromOperationIdOperationNameGenerator>(settings.OperationNameGenerator);
        }

        [Test]
        public async Task ParameterNameReplacementOn()
        {
            var settingsJson = JObject.Parse("{\"replaceNameCollection\":{\"@\":\"_\"}}");

            var context = CreateContext(settingsJson);
            var gen = await CSharpClientContentGenerator.CreateAsync(context);

            Assert.NotNull(gen);
            Assert.IsInstanceOf<CSharpClientContentGenerator>(gen);
            var fakeGen = (CSharpClientContentGenerator)gen!;
            var settings = fakeGen.Generator.Settings;
            Assert.NotNull(settings);
            Assert.NotNull(settings.CSharpGeneratorSettings.PropertyNameGenerator);
            Assert.IsInstanceOf<PropertyNameGeneratorWithReplace>(settings.CSharpGeneratorSettings.PropertyNameGenerator);
            Assert.NotNull(settings.ParameterNameGenerator);
            Assert.IsInstanceOf<ParameterNameGeneratorWithReplace>(settings.ParameterNameGenerator);
        }

        [Test]
        public async Task LoadOpenApiDocument_WithTextPreprocess()
        {
            const string schemaName = nameof(schemaName);
            var settingsJson = new JObject();

            Func<string, string?, string> dlgt = new FakeTextPreprocessor("{}").Process;

            var context = CreateContext(settingsJson);

            context.Preprocessors = new Preprocessors(
                new Dictionary<Type, Delegate[]> { [typeof(string)] = [dlgt] });

            var gen = (CSharpClientContentGenerator)await CSharpClientContentGenerator.CreateAsync(context);

            var openApiDocument = GetDocument(gen.Generator);

            Assert.NotNull(openApiDocument);
            Assert.That(openApiDocument?.Definitions, Does.ContainKey(schemaName));
            var sch = openApiDocument?.Definitions[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
            Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"additionalProperties\":false,\"processed\":{}}"));
        }

        [Test]
        public async Task LoadOpenApiDocument_WithTextPreprocess_Log()
        {
            const string schemaName = nameof(schemaName);
            const string filePath = "cd4bed67-1cc0-44a2-8dd1-30a0bd0c1dee";
            var settingsJson = new JObject();

            Func<string, string?, Abstraction.ILogger?, string> dlgt = new FakeTextPreprocessor("{}").Process;

            var logger = new Mock<Abstraction.ILogger>();
            var context = CreateContext(settingsJson);
            context.Logger = logger.Object;
            context.DocumentPath = filePath;

            context.Preprocessors = new Preprocessors(
                new Dictionary<Type, Delegate[]> { [typeof(string)] = [dlgt] });

            var gen = (CSharpClientContentGenerator)await CSharpClientContentGenerator.CreateAsync(context);

            var openApiDocument = GetDocument(gen.Generator);

            Assert.NotNull(openApiDocument);
            Assert.That(openApiDocument?.Definitions, Does.ContainKey(schemaName));
            var sch = openApiDocument?.Definitions[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
            Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"additionalProperties\":false,\"processed\":{}}"));
            logger.Verify(l => l.LogWarning(It.IsAny<string>(), filePath, It.IsAny<string>(), It.IsAny<object[]>()));
        }

        [Test]
        public async Task LoadOpenApiDocument_WithModelPreprocess()
        {
            const string schemaName = nameof(schemaName);
            var settingsJson = new JObject();

            Func<OpenApiDocument, string?, OpenApiDocument> dlgt = new FakeModelPreprocessor("{}").Process;

            var context = CreateContext(settingsJson);
            context.DocumentReader = new StringReader("{\"definitions\":{\"" + schemaName + "\":{\"$schema\":\"http://json-schema.org/draft-04/schema#\"}}}");

            context.Preprocessors = new Preprocessors(
                new Dictionary<Type, Delegate[]>
                {
                    [typeof(OpenApiDocument)] = [dlgt],
                });

            var gen = (CSharpClientContentGenerator)await CSharpClientContentGenerator.CreateAsync(context);

            var openApiDocument = GetDocument(gen.Generator);

            Assert.NotNull(openApiDocument);
            Assert.That(openApiDocument?.Definitions, Does.ContainKey(schemaName));
            var sch = openApiDocument?.Definitions[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
            Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"additionalProperties\":false,\"properties\":{\"processedModel\":{}}}"));
        }

        [Test]
        public async Task LoadOpenApiDocument_FromYaml()
        {
            var context = CreateContext(new());
            context.DocumentPath = TestHelpers.GetSwaggerPath("testSchema.yaml");
            var schemaText = await File.ReadAllTextAsync(context.DocumentPath);
            context.DocumentReader = new StringReader(schemaText);

            var gen = (CSharpClientContentGenerator)await CSharpClientContentGenerator.CreateAsync(context);

            var openApiDocument = GetDocument(gen.Generator);

            Assert.NotNull(openApiDocument);
            Assert.NotNull(openApiDocument!.DocumentPath);
        }

        [TestCase("externalRef.json")]
        public async Task LoadOpenApiDocument_ExternalRef(string documentPath)
        {
            var context = CreateContext(new());
            context.DocumentPath = TestHelpers.GetSwaggerPath(documentPath);
            var documentContent = await File.ReadAllTextAsync(context.DocumentPath);
            context.DocumentReader = new StringReader(documentContent);

            var gen = (CSharpClientContentGenerator)await CSharpClientContentGenerator.CreateAsync(context);

            var openApiDocument = GetDocument(gen.Generator);

            Assert.NotNull(openApiDocument);
            Assert.NotNull(openApiDocument!.Definitions["test"].AllOf.Single().Reference);
        }

        private GeneratorContext CreateContext(JObject settingsJson, Core.ExtensionManager.Extensions? extension = null)
        {
            extension ??= new();
            return new GeneratorContext(
                (t, s, _) => settingsJson.ToObject(t, s ?? new()),
                extension,
                _variables)
            {
                DocumentReader = new StringReader("{}"),
            };
        }

        private OpenApiDocument? GetDocument(CSharpClientGenerator generator)
        {
            var field = typeof(CSharpClientGenerator)
                .GetField("_document",
                          System.Reflection.BindingFlags.Instance |
                          System.Reflection.BindingFlags.NonPublic);
            return field?.GetValue(generator) as OpenApiDocument;
        }
    }
}

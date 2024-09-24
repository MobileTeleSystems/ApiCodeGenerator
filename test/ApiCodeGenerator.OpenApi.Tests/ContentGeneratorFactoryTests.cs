using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NSwag.CodeGeneration.OperationNameGenerators;

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

            Func<string, string?, string> dlgt =
            (doc, fileName) => new Infrastructure.FakeTextPreprocessor("{}").Process(doc, fileName);

            var context = CreateContext(settingsJson);

            context.Preprocessors = new Preprocessors(
                new Dictionary<Type, Delegate[]> { [typeof(string)] = new Delegate[] { dlgt } });

            var gen = (CSharpClientContentGenerator)await CSharpClientContentGenerator.CreateAsync(context);

            var openApiDocument = GetDocument(gen.Generator);

            Assert.NotNull(openApiDocument);
            Assert.That(openApiDocument?.Definitions, Does.ContainKey(schemaName));
            var sch = openApiDocument?.Definitions[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
            Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"additionalProperties\":false,\"processed\":{}}"));
        }

        [Test]
        public async Task LoadOpenApiDocument_WithModelPreprocess()
        {
            const string schemaName = nameof(schemaName);
            var settingsJson = new JObject();

            Func<OpenApiDocument, string?, OpenApiDocument> dlgt =
            (doc, fileName) => new Infrastructure.FakeModelPreprocessor("{}").Process(doc, fileName);

            var context = CreateContext(settingsJson);
            context.DocumentReader = new StringReader("{\"definitions\":{\"" + schemaName + "\":{\"$schema\":\"http://json-schema.org/draft-04/schema#\"}}}");

            context.Preprocessors = new Preprocessors(
                new Dictionary<Type, Delegate[]> { [typeof(OpenApiDocument)] = new Delegate[] { dlgt } });

            var gen = (CSharpClientContentGenerator)await CSharpClientContentGenerator.CreateAsync(context);

            var openApiDocument = GetDocument(gen.Generator);

            Assert.NotNull(openApiDocument);
            Assert.That(openApiDocument?.Definitions, Does.ContainKey(schemaName));
            var sch = openApiDocument?.Definitions[schemaName].ToJson(Newtonsoft.Json.Formatting.None);
            Assert.That(sch, Is.EqualTo("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"additionalProperties\":false,\"properties\":{\"processedModel\":{}}}"));
        }

        private GeneratorContext CreateContext(JObject settingsJson, Core.ExtensionManager.Extensions? extension = null)
        {
            extension ??= new();
            return new GeneratorContext(
                (t, s, v) => settingsJson.ToObject(t, s ?? new()),
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

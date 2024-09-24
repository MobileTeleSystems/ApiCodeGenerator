using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.Core.Tests
{
    public class NswagDocumentFactoryTests
    {
        private readonly Mock<IFileProvider> _fileProviderMock = new();

        [Test]
        public void LoadNswagDocument_Empty()
        {
            var factory = new NswagDocumentFactory(_fileProviderMock.Object);
            using var stream = new MemoryStream();
            var reader = new StreamReader(stream);

            var actual = Assert.Throws<InvalidOperationException>(() => factory.LoadNswagDocument(reader, null));

            Assert.NotNull(actual);
            Assert.That(actual.Message, Is.EqualTo("Nswag document is empty."));
        }

        [Test]
        public void LoadNswagDocument_WithBase_Preprocessors()
        {
            var factory = new NswagDocumentFactory(_fileProviderMock.Object);
            const string ppName = "mergeProcessor";
            const string cgName = "openApiToCSharpClient";

            var baseDoc = JObject.Parse(
            "{" +
              "\"documentGenerator\": {" +
                "\"preprocessors\": {" +
                  $"\"{ppName}\": {{}}" +
                "}" +
              "}" +
            "}");

            var nswagDocText =
            "{" +
              "\"documentGenerator\": {" +
                "\"fromDocument\": {" +
                    "\"json\": \"$(InputJson)\"," +
                    "\"url\": \"http://redocly.github.io/redoc/openapi.yaml\"," +
                    "\"output\": null" +
                "}" +
              "}," +
              "\"codeGenerators\": {" +
                "\"" + cgName + "\": {}" +
              "}" +
            "}";

            using var stream = new MemoryStream();
            var reader = new StringReader(nswagDocText);

            var actual = factory.LoadNswagDocument(reader, new Dictionary<string, string> { ["InputJson"] = "{}" }, baseDoc);

            Assert.That(actual.DocumentGenerator.Preprocessors, Is.Not.Null.And.ContainKey(ppName).And.Exactly(1).Items);
            Assert.That(actual.DocumentGenerator.FromDocument?.Json, Is.EqualTo("{}"));
            Assert.That(actual.CodeGenerators, Is.Not.Null.And.ContainKey(cgName).And.Exactly(1).Items);
        }

        [Test]
        public void LoadNswagDocument_WithBase_CodeGenerators()
        {
            var factory = new NswagDocumentFactory(_fileProviderMock.Object);
            const string cgName = "openApiToCSharpClient";
            const string className = nameof(LoadNswagDocument_WithBase_CodeGenerators);

            var baseDoc = JObject.Parse(
            "{" +
              "\"codeGenerators\": {" +
                "\"" + cgName + "\": {" +
                    "\"generateClientInterfaces\": true" +
                "}," +
                "\"secondGenerator\": {" +
                    "\"className\": \"someClass\"" +
                "}" +
              "}" +
            "}");

            var nswagDocText =
            "{" +
              "\"documentGenerator\": {" +
                "\"fromDocument\": {" +
                    "\"json\": \"$(InputJson)\"," +
                    "\"url\": \"http://redocly.github.io/redoc/openapi.yaml\"," +
                    "\"output\": null" +
                "}" +
              "}," +
              "\"codeGenerators\": {" +
                "\"" + cgName + "\": { \"className\": \"" + className + "\"}" +
              "}" +
            "}";

            using var stream = new MemoryStream();
            var reader = new StringReader(nswagDocText);

            var actual = factory.LoadNswagDocument(reader, null, baseDoc);

            Assert.That(actual.CodeGenerators, Is.Not.Null.And.ContainKey(cgName).And.Exactly(1).Items);
            var settings = actual.CodeGenerators.Values.First();

            Assert.AreEqual(className, settings.Value<string>("className"));
            Assert.True(settings.Value<bool>("generateClientInterfaces"));
        }

        [Test]
        public void LoadNswagDocument_WithBase_DefaultVariables()
        {
            var factory = new NswagDocumentFactory(_fileProviderMock.Object);
            const string cgName = "openApiToCSharpClient";
            const string className = nameof(LoadNswagDocument_WithBase_CodeGenerators);

            var baseDoc = JObject.Parse(
            "{" +
                "\"defaultVariables\":{" +
                    "\"var1\":\"" + className + "\"" +
                "}" +
            "}");

            var nswagDocText =
            "{" +
              "\"codeGenerators\": {" +
                "\"" + cgName + "\": {}" +
              "}" +
            "}";

            using var stream = new MemoryStream();
            var reader = new StringReader(nswagDocText);

            var actual = factory.LoadNswagDocument(reader, null, baseDoc);

            Assert.That(actual.DefaultVariables, Is.Not.Null.And.ContainKey("var1"));
            Assert.That(actual.DefaultVariables["var1"], Is.EqualTo(className));
        }
    }
}

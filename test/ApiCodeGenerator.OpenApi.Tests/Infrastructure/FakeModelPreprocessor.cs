using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace ApiCodeGenerator.OpenApi.Tests.Infrastructure
{
    internal class FakeModelPreprocessor
    {
        public FakeModelPreprocessor(string settingsJson)
        {
            Settings = settingsJson;
        }

        public static List<(JToken Settings, bool AsText, object?[] Arguments)> Invocactions { get; } = new();

        public JToken Settings { get; }

        public OpenApiDocument Process(OpenApiDocument document, string? fileName)
        {
            Invocactions.Add(new(Settings, true, new object?[] { document, fileName }));
            document.Definitions.Values.First().Properties.Add(
                "processedModel",
                new JsonSchemaProperty());
            return document;
        }
    }
}

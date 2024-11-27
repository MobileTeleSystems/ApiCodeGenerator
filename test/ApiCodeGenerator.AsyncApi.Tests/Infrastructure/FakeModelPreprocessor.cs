using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiCodeGenerator.AsyncApi.DOM;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure
{
    internal class FakeModelPreprocessor
    {
        public FakeModelPreprocessor(string settingsJson)
        {
            Settings = settingsJson;
        }

        public static List<(JToken Settings, bool AsText, object?[] Arguments)> Invocactions { get; } = new();

        public JToken Settings { get; }

        public AsyncApiDocument Process(AsyncApiDocument document, string? fileName)
        {
            Invocactions.Add(new(Settings, true, [document, fileName]));
            document.Components?.Schemas.Values.First().Properties.Add(
                "processedModel",
                new JsonSchemaProperty());
            return document;
        }
    }
}

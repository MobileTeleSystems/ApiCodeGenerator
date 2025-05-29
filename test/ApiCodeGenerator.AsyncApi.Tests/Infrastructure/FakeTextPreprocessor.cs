using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure
{
    internal class FakeTextPreprocessor
    {
        public FakeTextPreprocessor(string settingsJson)
        {
            Settings = settingsJson;
        }

        public static List<(JToken Settings, bool AsText, object?[] Arguments)> Invocactions { get; } = new();

        public JToken Settings { get; }

        public string Process(string data, string? fileName)
        {
            Invocactions.Add(new(Settings, true, [data, fileName]));
            return """
            {
                "asyncapi":"3.0.0",
                "info":{
                    "title": "",
                    "version": "1.0"
                },
                "components":{
                    "schemas":{
                        "schemaName":{
                            "$schema":"http://json-schema.org/draft-04/schema#",
                            "processed":{}
                        }
                    }
                }
            }
            """;
        }

        public string Process(string data, string? fileName, ILogger? logger)
        {
            logger?.LogWarning(null, fileName, "test");
            return Process(data, fileName);
        }
    }
}

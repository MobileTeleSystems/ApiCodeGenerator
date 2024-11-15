using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

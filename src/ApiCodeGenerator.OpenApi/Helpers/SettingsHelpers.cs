using System;
using System.Collections.Generic;
using System.Linq;
using ApiCodeGenerator.Abstraction;
using Newtonsoft.Json.Linq;

#if ASYNC_API
using ApiCodeGenerator.AsyncApi.OperationNameGenerators;

namespace ApiCodeGenerator.AsyncApi.Helpers;
#else
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace ApiCodeGenerator.OpenApi.Helpers;
#endif

internal static class SettingsHelpers
{
    public static void SetSpecialSettings(IExtensions? extensions, ClientGeneratorBaseSettings settings, string propertyName, object? value)
    {
        if (propertyName.Equals("OperationGenerationMode", StringComparison.OrdinalIgnoreCase))
        {
            var mode = ((JToken?)value)?.Value<string>();
            if (mode is not null)
                SetOperationMode(extensions, settings, mode);
        }
        else if (propertyName.Equals("replaceNameCollection", StringComparison.OrdinalIgnoreCase))
        {
            if (((JToken?)value)?.Type == JTokenType.Object)
            {
                var replacementData = ((JObject)value).ToObject<IDictionary<string, string>>();
                if (replacementData is not null)
                    SetReplaceCollection(settings, replacementData);
            }
            else
            {
                throw new InvalidOperationException($"Property '{propertyName}' must contains object declaration (e.g. {{\"@\",\"_\"}})");
            }
        }
    }

    private static void SetReplaceCollection(ClientGeneratorBaseSettings settings, IDictionary<string, string> replaceNamePairs)
    {
        settings.ParameterNameGenerator = new ParameterNameGeneratorWithReplace(replaceNamePairs, settings.ParameterNameGenerator);
        settings.CodeGeneratorSettings.PropertyNameGenerator = new PropertyNameGeneratorWithReplace(replaceNamePairs);
    }

    private static void SetOperationMode(IExtensions? extensions, ClientGeneratorBaseSettings settings, string v)
    {
        if (extensions?.OperationGenerators.TryGetValue(v, out var operationGeneratorTypes) == true)
        {
            var targetType = operationGeneratorTypes.FirstOrDefault(ogt => typeof(IOperationNameGenerator).IsAssignableFrom(ogt))
                ?? throw new InvalidOperationException($"OperationeGenerator '{v}' not found");

            settings.OperationNameGenerator = (IOperationNameGenerator)Activator.CreateInstance(targetType);
        }
    }
}

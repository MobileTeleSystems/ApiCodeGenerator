using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.NameGenerators;

public class DefaultParameterNameGenerator : IParameterNameGenerator
{
    public static string GetVariableName(string parameterName)
    {
        return ConversionUtilities.ConvertToLowerCamelCase(
            new string(
                parameterName.Select(c => c switch
                    {
                        '-' or '.' => '_',
                        '$' or '@' or '[' or ']' => '\n',
                        _ => c,
                    })
                    .Where(c => c != '\n')
                    .ToArray()),
            firstCharacterMustBeAlpha: true);
    }

    public string Generate(string parameterName, Parameter parameter, IEnumerable<NamedReference<Parameter>> allParameters)
        => GetVariableName(parameterName);
}

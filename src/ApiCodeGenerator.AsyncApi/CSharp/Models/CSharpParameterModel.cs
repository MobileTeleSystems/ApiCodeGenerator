using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.CSharp.Models;

public class CSharpParameterModel
{
    private readonly string _parameterName;
    private readonly Parameter _parameter;

    public CSharpParameterModel(string parameterName, Parameter parameter, string parameterType)
    {
        _parameterName = parameterName;
        _parameter = parameter;
        ParameterType = parameterType;
    }

    public string CamelCaseParameterName => ConversionUtilities.ConvertToLowerCamelCase(_parameterName, true);

    public string ParameterType { get; }
}

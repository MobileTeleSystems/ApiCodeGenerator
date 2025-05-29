using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.CSharp.Models;

public class CSharpParameterModel
{
    private readonly string _parameterName;
    private readonly Parameter _parameter;

    public CSharpParameterModel(string parameterName, Parameter parameter)
    {
        _parameterName = parameterName;
        _parameter = parameter;
    }

    public string CamelCaseParameterName => ConversionUtilities.ConvertToLowerCamelCase(_parameterName, true);

    public virtual string ParameterType => "string";
}

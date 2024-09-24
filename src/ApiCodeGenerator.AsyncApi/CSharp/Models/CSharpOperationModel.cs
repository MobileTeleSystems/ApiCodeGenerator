using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.CSharp.Models;

public class CSharpOperationModel
{
    private readonly CSharpTypeResolver _typeResolver;
    private string? _payloadType;

    public CSharpOperationModel(
        string operationName,
        string channelName,
        Channel channel,
        Operation operation,
        CSharpGeneratorBaseSettings settings,
        CSharpTypeResolver typeResolver)
    {
        ChannelName = channelName;
        Channel = channel;
        Operation = operation;
        OperationName = ConversionUtilities.ConvertToUpperCamelCase(operationName, true);
        _typeResolver = typeResolver;
        Parameters = Channel.Parameters
            .Select(cp =>
                new CSharpParameterModel(
                    settings.ParameterNameGenerator.Generate(cp.Key, cp.Value, Channel.Parameters.Values),
                    cp.Value,
                    ResolveParameterType(cp.Key, cp.Value)))
            .ToArray();

        Description = !string.IsNullOrEmpty(operation.Summary)
            ? operation.Summary
            : operation.Description;
        HasDescription = !string.IsNullOrEmpty(Description);
    }

    public string ChannelName { get; }

    public string ControllerName { get; set; } = string.Empty;

    public string? Description { get; }

    public bool HasDescription { get; }

    public bool HasPublish => Channel.Publish == Operation;

    public string OperationId => Operation.OperationId ?? string.Empty;

    public string OperationName { get; }

    public CSharpParameterModel[] Parameters { get; }

    public string PayloadType => _payloadType ??= ResolvePayloadType(Operation.Message.ActualObject.Payload.ActualSchema);

    protected Channel Channel { get; }

    protected Operation Operation { get; }

    protected virtual string ResolvePayloadType(JsonSchema jsonSchema, string? hint = null)
    {
        if (!jsonSchema.HasTypeNameTitle && string.IsNullOrEmpty(hint))
        {
            hint = ConversionUtilities.ConvertToUpperCamelCase($"{Operation.Message.ActualObject.Name}Payload", false);
        }

        return _typeResolver.Resolve(jsonSchema, false, hint);
    }

    protected virtual string ResolveParameterType(string parameterName, Parameter operationParameter)
    {
        var schema = operationParameter.ActualObject.Schema.ActualSchema;
        var typeNameHint = !schema.HasTypeNameTitle
            ? parameterName
            : null;

        var isNullable = schema.IsNullable(SchemaType.OpenApi3);

        return _typeResolver.Resolve(schema, isNullable, typeNameHint);
    }
}

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
        Operation operation,
        CSharpGeneratorBaseSettings settings,
        CSharpTypeResolver typeResolver)
    {
        Channel = operation.Channel.ActualObject;
        ChannelAddress = Channel.Address;
        Operation = operation;
        OperationName = ConversionUtilities.ConvertToUpperCamelCase(operationName, true);
        _typeResolver = typeResolver;

        var actualParameters = Channel.Parameters?.Values.ToArray() ?? [];
        Parameters = actualParameters
            .Select((cp, ind) =>
                new CSharpParameterModel(
                    settings.ParameterNameGenerator.Generate(
                        cp.ObjectId ?? $"param{ind}",
                        cp,
                        actualParameters),
                    cp))
            .ToArray();

        Description = !string.IsNullOrEmpty(operation.Summary)
            ? operation.Summary
            : operation.Description;
        HasDescription = !string.IsNullOrEmpty(Description);
    }

    public string? ChannelAddress { get; }

    public string ControllerName { get; set; } = string.Empty;

    public string? Description { get; }

    public bool HasDescription { get; }

    public bool HasSend => Operation.Action == OperationAction.Send;

    public string OperationName { get; }

    public CSharpParameterModel[] Parameters { get; }

    public string PayloadType => _payloadType ??= ResolvePayloadType(Operation.Messages.First().ActualObject.Payload?.ActualObject.ActualSchema, hint: null);

    protected Channel Channel { get; }

    protected Operation Operation { get; }

    protected virtual string ResolvePayloadType(JsonSchema jsonSchema, string? hint)
    {
        if (!jsonSchema.HasTypeNameTitle && string.IsNullOrEmpty(hint))
        {
            hint = ConversionUtilities.ConvertToUpperCamelCase($"{Operation.Messages.First().ActualObject.Name}Payload", false);
        }

        return _typeResolver.Resolve(jsonSchema, false, hint);
    }
}

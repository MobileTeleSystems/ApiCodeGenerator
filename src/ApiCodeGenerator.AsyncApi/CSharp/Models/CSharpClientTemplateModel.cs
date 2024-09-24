using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.CSharp.Models;

public class CSharpClientTemplateModel
{
    private readonly CSharpGeneratorBaseSettings _settings;

    public CSharpClientTemplateModel(string className, AsyncApiDocument document, CSharpGeneratorBaseSettings settings, CSharpOperationModel[] operationModels)
    {
        _settings = settings;
        Class = className;
        OperationModels = operationModels;
        HasDescription = !string.IsNullOrEmpty(document.Info?.Description);
        if (HasDescription)
        {
            Description = document.Info!.Description;
        }
    }

    public string BaseTypes => _settings.GenerateClientInterfaces ? Interface : string.Empty;

    public string Class { get; }

    public string Interface => 'I' + Class;

    public bool HasBaseTypes => _settings.GenerateClientInterfaces;

    public bool HasDescription { get; }

    public bool HasNewtonsoftJsonLibrary
        => _settings.CSharpGeneratorSettings.JsonLibrary == NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary.NewtonsoftJson;

    public string? Description { get; }

    public string ClientClassAccessModifier => _settings.ClientClassAccessModifier;

    public CSharpOperationModel[] OperationModels { get; protected set; }
}

namespace ApiCodeGenerator.OpenApi;

public class CSharpControllerGeneratorSettings : NSwag.CodeGeneration.CSharp.CSharpControllerGeneratorSettings
{
    public CSharpControllerGeneratorSettings()
        : base()
    {
        CSharpGeneratorSettings.TemplateFactory = new DefaultTemplateFactory(
                CodeGeneratorSettings,
                [
                    typeof(NSwag.CodeGeneration.CSharp.CSharpClientGenerator).Assembly,
                    typeof(NJsonSchema.CodeGeneration.CSharp.CSharpGenerator).Assembly
                ]);
    }
}

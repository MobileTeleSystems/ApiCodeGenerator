namespace ApiCodeGenerator.OpenApi;

public class CSharpClientGeneratorSettings : NSwag.CodeGeneration.CSharp.CSharpClientGeneratorSettings
{
    public CSharpClientGeneratorSettings()
        : base()
    {
        CSharpGeneratorSettings.TemplateFactory = new DefaultTemplateFactory(
                CSharpGeneratorSettings,
                [
                    typeof(NSwag.CodeGeneration.CSharp.CSharpClientGenerator).Assembly,
                    typeof(NJsonSchema.CodeGeneration.CSharp.CSharpGenerator).Assembly]);
    }
}

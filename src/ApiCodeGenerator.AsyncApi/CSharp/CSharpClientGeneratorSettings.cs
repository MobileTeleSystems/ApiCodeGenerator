namespace ApiCodeGenerator.AsyncApi.CSharp;

public class CSharpClientGeneratorSettings : CSharpGeneratorBaseSettings
{
    public CSharpClientGeneratorSettings()
    {
        CSharpGeneratorSettings.TemplateFactory = new DefaultTemplateFactory(
            CodeGeneratorSettings,
            [
                GetType().Assembly,
                typeof(NJsonSchema.CodeGeneration.CSharp.CSharpGenerator).Assembly,
            ]);
    }
}

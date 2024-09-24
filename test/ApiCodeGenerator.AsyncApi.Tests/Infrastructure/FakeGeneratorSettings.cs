namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure;

public sealed class FakeGeneratorSettings : CSharpGeneratorBaseSettings
{
    public FakeGeneratorSettings()
    {
        CodeGeneratorSettings.TemplateFactory = new DefaultTemplateFactory(
            CodeGeneratorSettings,
            [
                typeof(DefaultTemplateFactory).Assembly,
                typeof(NJsonSchema.CodeGeneration.CSharp.CSharpGenerator).Assembly,
            ]);
    }

    public string? TestProp { get; set; }
}

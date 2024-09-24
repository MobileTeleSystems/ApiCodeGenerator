using ApiCodeGenerator.AsyncApi.CSharp;

namespace ApiCodeGenerator.AsyncApi.Amqp.CSharp;

public class CSharpAmqpServiceGeneratorSettings : CSharpClientGeneratorSettings
{
    public CSharpAmqpServiceGeneratorSettings()
    {
        var asmName = GetType().Assembly.GetName().Name;
        CSharpGeneratorSettings.TemplateFactory = new DefaultTemplateFactory(
            CodeGeneratorSettings,
            [
                new EmbededResourceTemplateProvider(GetType().Assembly, $"{asmName}.TemplatesAmqp"),
                .. DefaultTemplateFactory.CreateProviders(CodeGeneratorSettings,
                [
                    typeof(CSharpClientGeneratorSettings).Assembly,
                    typeof(NJsonSchema.CodeGeneration.CSharp.CSharpGenerator).Assembly
                ]),
            ]);
    }

    public DOM.Bindings.Amqp.Channel? ChannelBindingDefaults { get; set; }

    public DOM.Bindings.Amqp.OperationBase? OperationBindingDefaults { get; set; }

    public DOM.Bindings.Amqp.Message? MessageBindingDefaults { get; set; }
}

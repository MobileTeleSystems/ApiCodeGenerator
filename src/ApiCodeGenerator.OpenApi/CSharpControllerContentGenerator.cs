using System;
using System.IO;
using System.Threading.Tasks;
using NSwag.CodeGeneration.CSharp;

namespace ApiCodeGenerator.OpenApi
{
    internal sealed class CSharpControllerContentGenerator
        : ContentGeneratorBase<CSharpControllerContentGenerator, CSharpControllerGenerator, CSharpControllerGeneratorSettings>
    {
        public override string Generate()
        {
            Generator.Settings.CodeGeneratorSettings.TemplateFactory =
                new DefaultTemplateFactory(
                Generator.Settings.CodeGeneratorSettings,
                [typeof(CSharpClientGenerator).Assembly, typeof(NJsonSchema.CodeGeneration.CSharp.CSharpGenerator).Assembly]);
            return base.Generate();
        }
    }
}

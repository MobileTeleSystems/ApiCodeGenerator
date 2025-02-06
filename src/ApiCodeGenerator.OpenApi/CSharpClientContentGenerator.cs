using System;
using System.IO;
using System.Threading.Tasks;
using NSwag.CodeGeneration.CSharp;

namespace ApiCodeGenerator.OpenApi
{
    internal sealed class CSharpClientContentGenerator
        : ContentGeneratorBase<CSharpClientContentGenerator, CSharpClientGenerator, CSharpClientGeneratorSettings>
    {
    }
}

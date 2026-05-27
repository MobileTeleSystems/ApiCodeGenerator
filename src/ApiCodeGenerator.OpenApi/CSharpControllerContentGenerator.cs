using System;
using System.IO;
using System.Threading.Tasks;
using NSwag.CodeGeneration.CSharp;

namespace ApiCodeGenerator.OpenApi
{
    internal sealed class CSharpControllerContentGenerator
        : CSharpContentGenerator<CSharpControllerContentGenerator, CSharpControllerGenerator, CSharpControllerGeneratorSettings>
    {
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApiCodeGenerator.Abstraction
{
    internal interface IGenerationTask
    {
        Task<bool> ExecuteAsync(string nswagFilePath, string openApiFilePath, string outFilePath, string? variables = null, string? baseNswagFilePath = null);
    }
}

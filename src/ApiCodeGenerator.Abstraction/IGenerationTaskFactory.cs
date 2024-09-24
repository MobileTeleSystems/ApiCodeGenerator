using System;
using System.Collections.Generic;
using System.Text;

namespace ApiCodeGenerator.Abstraction
{
    internal interface IGenerationTaskFactory
    {
        IGenerationTask Create(IExtensions extensions, ILogger log);

        IGenerationTask Create(string[]? extensionPaths, ILogger log);

        IExtensions LoadExtensions(string[] extensionPaths);
    }
}

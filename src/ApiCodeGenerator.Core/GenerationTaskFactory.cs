using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.Core.ExtensionManager;

namespace ApiCodeGenerator.Core
{
    [ExcludeFromCodeCoverage]
    internal class GenerationTaskFactory : IGenerationTaskFactory
    {
        public IGenerationTask Create(IExtensions extensions, ILogger? log)
            => new GenerationTask(extensions, log);

        public IGenerationTask Create(string[]? extensionPaths, ILogger log)
        {
            var extensions = LoadExtensions(extensionPaths ?? []);
            return Create(extensions, log);
        }

        public IExtensions LoadExtensions(string[] extensionPaths)
            => ExtensionLoader.LoadExtensions(extensionPaths);
    }
}

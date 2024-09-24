using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiCodeGenerator.Abstraction
{
    public delegate Task<IContentGenerator> ContentGeneratorFactory(GeneratorContext context);

    public interface IExtensions
    {
        IReadOnlyDictionary<string, ContentGeneratorFactory> CodeGenerators { get; }

        IReadOnlyDictionary<string, IReadOnlyCollection<Type>> OperationGenerators { get; }

        IReadOnlyDictionary<string, Type> Preprocessors { get; }
    }
}

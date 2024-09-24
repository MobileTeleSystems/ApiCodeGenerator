using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ApiCodeGenerator.Abstraction;

namespace ApiCodeGenerator.Core.ExtensionManager
{
    /// <summary>
    /// Информация о расширениях.
    /// </summary>
    public class Extensions : IExtensions
    {
        public Extensions(
            IReadOnlyDictionary<string, ContentGeneratorFactory>? codeGenerators = null,
            IReadOnlyDictionary<string, IReadOnlyCollection<Type>>? operationGenerators = null,
            IReadOnlyDictionary<string, Type>? preprocessors = null)
        {
            CodeGenerators = codeGenerators ?? new ReadOnlyDictionary<string, ContentGeneratorFactory>(new Dictionary<string, ContentGeneratorFactory>());
            OperationGenerators = operationGenerators ?? new ReadOnlyDictionary<string, IReadOnlyCollection<Type>>(new Dictionary<string, IReadOnlyCollection<Type>>());
            Preprocessors = preprocessors ?? new ReadOnlyDictionary<string, Type>(new Dictionary<string, Type>());
        }

        /// <summary>
        /// Дополнительные генераторы кода.
        /// </summary>
        public IReadOnlyDictionary<string, ContentGeneratorFactory> CodeGenerators { get; }

        /// <summary>
        /// Дополнительные генераторы операций.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyCollection<Type>> OperationGenerators { get; }

        /// <summary>
        /// Дополнительные обработчики документа OpenApi.
        /// </summary>
        public IReadOnlyDictionary<string, Type> Preprocessors { get; }
    }
}

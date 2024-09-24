using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace ApiCodeGenerator.OpenApi
{
    /// <summary>
    /// Генерирует имена методов используя последний сегмент operationId.
    /// </summary>
    public class SingleClientFromLastSegmentOperationIdOperationNameGenerator : IOperationNameGenerator
    {
        private readonly char _delemiter;

        /// <summary>
        /// Создает и нициализирует экземпляр. Разделитель точка.
        /// </summary>
        public SingleClientFromLastSegmentOperationIdOperationNameGenerator()
            : this('.')
        {
        }

        /// <summary>
        /// Создает и нициализирует экземпляр.
        /// </summary>
        /// <param name="delemiter">Символ-разделитель сегментов.</param>
        public SingleClientFromLastSegmentOperationIdOperationNameGenerator(char delemiter)
        {
            _delemiter = delemiter;
        }

        /// <inheritdoc/>
        public bool SupportsMultipleClients => false;

        /// <inheritdoc/>
        public string GetClientName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public string GetOperationName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
        {
            return operation.OperationId
                .Split(new[] { _delemiter }, StringSplitOptions.RemoveEmptyEntries)
                .Last();
        }
    }
}

using System;
using System.Collections.Generic;
using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.OpenApi;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace ApiCodeGenerator
{
    public static class AcgExtension
    {
        public static Dictionary<string, ContentGeneratorFactory> CodeGenerators { get; } = new()
        {
            ["OpenApiToCSharpClient"] = CSharpClientContentGenerator.CreateAsync,
            ["OpenApiToCSharpController"] = CSharpControllerContentGenerator.CreateAsync,
        };

        public static Dictionary<string, Type> OperationGenerators { get; } = new()
        {
            ["MultipleClientsFromOperationId"] = typeof(MultipleClientsFromOperationIdOperationNameGenerator),
            ["MultipleClientsFromPathSegments"] = typeof(MultipleClientsFromPathSegmentsOperationNameGenerator),
            ["MultipleClientsFromFirstTagAndPathSegments"] = typeof(MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator),
            ["MultipleClientsFromFirstTagAndOperationId"] = typeof(MultipleClientsFromFirstTagAndOperationIdGenerator),
            ["SingleClientFromOperationId"] = typeof(SingleClientFromOperationIdOperationNameGenerator),
            ["SingleClientFromPathSegments"] = typeof(SingleClientFromPathSegmentsOperationNameGenerator),
            ["SingleClientFromLastSegmentOfOperationId"] = typeof(SingleClientFromLastSegmentOperationIdOperationNameGenerator),
        };
    }
}

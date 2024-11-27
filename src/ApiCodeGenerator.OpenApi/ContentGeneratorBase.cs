using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.Core.Converters;
using Newtonsoft.Json;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using YamlDotNet.Core;

namespace ApiCodeGenerator.OpenApi
{
    public class ContentGeneratorBase<TContentGenerator, TGenerator, TSettings> : IContentGenerator
        where TContentGenerator : ContentGeneratorBase<TContentGenerator, TGenerator, TSettings>, new()
        where TGenerator : CSharpGeneratorBase
        where TSettings : CSharpGeneratorBaseSettings, new()
    {
        protected ContentGeneratorBase()
        {
        }

        public TGenerator Generator { get; private set; } = null!;

        protected GeneratorContext Context { get; private set; } = null!;

        /// <inheritdoc/>
        public virtual string Generate() => Generator.GenerateFile();

#pragma warning disable SA1204 // Static elements should appear before instance elements
        public static async Task<IContentGenerator> CreateAsync(GeneratorContext context)
#pragma warning restore SA1204 // Static elements should appear before instance elements
        {
            var apiDocument = await ReadAndProcessOpenApiDocument(context);
            var variables = GetAdditionalVariables(apiDocument);
            var settings = ParseSettings(context, variables);
            var resolver = CSharpGeneratorBase.CreateResolverWithExceptionSchema(settings.CSharpGeneratorSettings, apiDocument);
            var csharpGenerator = (TGenerator)Activator.CreateInstance(typeof(TGenerator), apiDocument, settings, resolver);

            var contentGenerator = new TContentGenerator
            {
                Generator = csharpGenerator,
                Context = context,
            };

            return contentGenerator;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "near the usage")]
        private static readonly Regex SEM_VER_PARSER = new(
            @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<prerelease>[0-9A-Za-z-]+))?(?:\+(?<buildmetadata>[0-9A-Za-z-]+))?$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        protected static IReadOnlyDictionary<string, string>? GetAdditionalVariables(OpenApiDocument openApiDocument)
        {
            var version = openApiDocument.Info?.Version;
            if (!string.IsNullOrEmpty(version))
            {
                var match = SEM_VER_PARSER.Match(version);
                if (match.Success)
                {
                    return new Dictionary<string, string>
                    {
                        ["Version.Major"] = match.Groups["major"].Value,
                        ["Version.Minor"] = match.Groups["minor"].Value,
                        ["Version.Patch"] = match.Groups["patch"].Value,
                        ["Version.Prerelease"] = match.Groups["Prerelease"].Value,
                        ["Version.Build"] = match.Groups["buildmetadata"].Value,
                    };
                }
            }

            return null;
        }

        protected static T InvokePreprocessors<T>(T data,
            Preprocessors? preprocessors,
            string? filePath,
            ILogger? logger)
        {
            if (preprocessors?.TryGetValue(typeof(T), out var openApiDocumentPreprocessors) == true)
            {
                foreach (var processor in openApiDocumentPreprocessors)
                {
                    data = processor switch
                    {
                        Func<T, string?, T> p => p.Invoke(data, filePath),
                        Func<T, string?, ILogger?, T> p => p.Invoke(data, filePath, logger),
                        _ => data,
                    };
                }
            }

            return data;
        }

        protected static TSettings ParseSettings(GeneratorContext context, IReadOnlyDictionary<string, string>? variables)
        {
            var unwrapProps = new[]
            {
                nameof(CSharpClientGeneratorSettings.CSharpGeneratorSettings),
                nameof(ClientGeneratorBaseSettings.CodeGeneratorSettings),
            };
            var settingsType = typeof(TSettings);
            var serializer = JsonSerializer.Create(
                    new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter>
                        {
                            new SettingsConverter(
                                settingsType,
                                unwrapProps,
                                (s, p, v) => Helpers.SettingsHelpers.SetSpecialSettings(context.Extensions, (ClientGeneratorBaseSettings)s, p, v)),
                        },
                    });

            var settings = context.GetSettings<TSettings>(serializer, variables) ?? new();
            return settings;
        }

        protected static async Task<OpenApiDocument> ReadAndProcessOpenApiDocument(GeneratorContext context)
        {
            var documentStr = context.DocumentReader!.ReadToEnd();
            documentStr = InvokePreprocessors<string>(documentStr, context.Preprocessors, context.DocumentPath, context.Logger);

            OpenApiDocument openApiDocument;

            try
            {
                openApiDocument = await OpenApiDocument.FromJsonAsync(documentStr, context.DocumentPath);
            }
            catch (JsonException ex)
            {
                try
                {
                    openApiDocument = await OpenApiYamlDocument.FromYamlAsync(documentStr, context.DocumentPath);
                }
                catch (YamlException ex2)
                {
                    throw new InvalidOperationException(
                        $"Can not read document as JSON ({ex.Message}) or YAML ({ex2.Message}).");
                }
            }

            openApiDocument = InvokePreprocessors<OpenApiDocument>(openApiDocument, context.Preprocessors, context.DocumentPath, context.Logger);
            return openApiDocument;
        }
    }
}

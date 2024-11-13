using System.Text.RegularExpressions;
using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.AsyncApi.CSharp;
using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.Core.Converters;
using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi
{
    public abstract class AsyncApiContentGenerator<TContentGenerator, TGenerator, TSettings> : IContentGenerator
        where TContentGenerator : AsyncApiContentGenerator<TContentGenerator, TGenerator, TSettings>, new()
        where TGenerator : CSharpGeneratorBase<TSettings>
        where TSettings : CSharpGeneratorBaseSettings, new()
    {
        internal static readonly string[] UNWRAP_PROPS = [
            nameof(CSharpGeneratorBaseSettings.CSharpGeneratorSettings),
        ];

        protected TGenerator Generator { get; private set; } = null!;

        protected TSettings Settings { get; private set; } = null!;

        public static async Task<IContentGenerator> CreateAsync(GeneratorContext context)
        {
            var document = await LoadDocumentAsync(context);
            var variables = GetAdditionalVariables(document);
            var settings = LoadSettings(context, variables);
            var resolver = CSharpGeneratorBase<TSettings>.CreateResolver(document, settings);

            var generator = (TGenerator)Activator.CreateInstance(typeof(TGenerator), document, settings, resolver);

            var contentGenerator = new TContentGenerator()
            {
                Generator = generator,
                Settings = settings,
            };
            return contentGenerator;
        }

        public virtual string Generate() => Generator.Generate();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Удобнее рядом с использующим кодом")]
        private static readonly Regex SEM_VER_PARSER = new(
            @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<prerelease>[0-9A-Za-z-]+))?(?:\+(?<buildmetadata>[0-9A-Za-z-]+))?$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        protected static IReadOnlyDictionary<string, string>? GetAdditionalVariables(AsyncApiDocument apiDocument)
        {
            var version = apiDocument.Info?.Version;
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
            if (preprocessors?.TryGetValue(typeof(T), out var documentPreprocessors) == true)
            {
                foreach (var processor in documentPreprocessors)
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

        private static async Task<AsyncApiDocument> LoadDocumentAsync(GeneratorContext context)
        {
            var data = await context.DocumentReader!.ReadToEndAsync();
            data = InvokePreprocessors<string>(data, context.Preprocessors, context.DocumentPath, context.Logger);

            var documentTask = data.StartsWith("{")
                ? AsyncApiDocument.FromJsonAsync(data)
                : AsyncApiDocument.FromYamlAsync(data);
            var document = await documentTask.ConfigureAwait(false);

            document = InvokePreprocessors<AsyncApiDocument>(document, context.Preprocessors, context.DocumentPath, context.Logger);
            return document;
        }

        private static TSettings LoadSettings(GeneratorContext context, IReadOnlyDictionary<string, string>? variables)
        {
            var serializer = new JsonSerializer
            {
                Converters =
                {
                    new SettingsConverter(
                        typeof(TSettings),
                        UNWRAP_PROPS,
                        (s, p, v) => Helpers.SettingsHelpers.SetSpecialSettings(context.Extensions, (ClientGeneratorBaseSettings)s, p, v)),
                },
            };
            return context.GetSettings<TSettings>(serializer, variables) ?? new();
        }
    }
}

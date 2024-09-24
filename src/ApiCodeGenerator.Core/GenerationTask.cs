using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.Core.NswagDocument;
using ApiCodeGenerator.Core.NswagDocument.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.Core
{
    /// <summary>
    /// Реализует MSBuild-задачу для генерации клиента на основании файла Nswag.
    /// </summary>
    internal sealed class GenerationTask : IGenerationTask
    {
        private readonly INswagDocumentFactory _documentFactory;
        private readonly IApiDocumentProvider _apiDocumentProvider;
        private readonly IFileProvider _fileProvider;
        private readonly IExtensions _extensions;

        /// <summary>
        /// Создает и инциализирует экземпляр объекта <see cref="GenerationTask"/>.
        /// </summary>
        /// <param name="extensions">Расширения процесса генерации.</param>
        /// <param name="log">Адаптер логирования.</param>
        [ExcludeFromCodeCoverage] // test use other ctor
        public GenerationTask(IExtensions extensions, ILogger? log)
            : this(
                extensions,
                new NswagDocumentFactory(),
                null,
                new PhysicalFileProvider(),
                log)
        {
        }

        /// <summary>
        /// Создает и инциализирует экземпляр объекта <see cref="GenerationTask"/> class.
        /// </summary>
        /// <param name="extensions">Расширения процесса генерации.</param>
        /// <param name="documentFactory">Загрузчик документов.</param>
        /// <param name="apiDocumentProvider">Загрузчик документов Api.</param>
        /// <param name="fileProvider">Интерфейс доступа к ФС.</param>
        /// <param name="log">Адаптер логирования.</param>
        internal GenerationTask(
            IExtensions extensions,
            INswagDocumentFactory documentFactory,
            IApiDocumentProvider? apiDocumentProvider,
            IFileProvider fileProvider,
            ILogger? log)
        {
            _extensions = extensions;
            _documentFactory = documentFactory;
            _apiDocumentProvider = apiDocumentProvider ?? new ApiDocumentProvider(fileProvider, new());
            _fileProvider = fileProvider;
            Log = log;
        }

        private ILogger? Log { get; }

        /// <summary>
        /// Генерирует код и сохраняет его в указанный файл.
        /// </summary>
        /// <param name="nswagFilePath">Путь к файлу настроек генератора.</param>
        /// <param name="openApiFilePath">Путь к файлу документа OpenApi.</param>
        /// <param name="outFilePath">Путь к фалу с результатами генерации.</param>
        /// <param name="variables">Перечень пар ключ=значение разделенныз запятой.</param>
        /// <param name="baseNswagFilePath">Файл базовых настроек.</param>
        /// <returns>True если процесс генерации успешно завершен.</returns>
        public async Task<bool> ExecuteAsync(string nswagFilePath,
                                             string openApiFilePath,
                                             string outFilePath,
                                             string? variables = null,
                                             string? baseNswagFilePath = null)
        {
            if (!_fileProvider.Exists(nswagFilePath))
            {
                Log?.LogError(null, "File '{0}' not found.", nswagFilePath);
                return false;
            }

            // System.Diagnostics.Debugger.Launch();
            var vars = ParseVariables(variables);
            vars["InputJson"] = openApiFilePath;
            vars["OutFile"] = outFilePath;
            var roVariables = new ReadOnlyDictionary<string, string>(vars);

            Log?.LogMessage("Values of nswag variables");
            Log?.LogMessage(string.Join(Environment.NewLine, vars.Select(_ => $"\t[{_.Key}] = {_.Value}")));

            JObject? baseNswagDocument = LoadBaseNswag(baseNswagFilePath);
            var nswagDocument = _documentFactory.LoadNswagDocument(nswagFilePath, roVariables, baseNswagDocument);

            var generatorSettings = nswagDocument.CodeGenerators.FirstOrDefault();
            if (generatorSettings.Key is null)
            {
                Log?.LogWarning(nswagFilePath, "Nswag not contains codeGenerator definition. Skip generation.");
                return true;
            }

            if (!_extensions.CodeGenerators.TryGetValue(generatorSettings.Key, out var contentGeneratorFactory))
            {
                Log?.LogError(nswagFilePath, $"Unable find generator {generatorSettings.Key}. Check package references.");
                return false;
            }

            var context = await CreateGenerationContext(nswagDocument, nswagFilePath, roVariables);

            if (context is not null)
            {
                if (context.DocumentReader is null)
                {
                    Log?.LogWarning(nswagFilePath, "Source not set. Skip generation.");
                    return true;
                }

                try
                {
                    Log?.LogMessage($"Use settings: {generatorSettings.Key}");
                    var contentGenerator = await contentGeneratorFactory.Invoke(context);

                    Log?.LogMessage("Generate content for file '{0}'", outFilePath);
                    var code = contentGenerator.Generate();

                    try
                    {
                        Log?.LogMessage("Write file '{0}'", outFilePath);
                        await _fileProvider.WriteAllTextAsync(outFilePath, code);
                    }
                    catch (Exception ex)
                    {
                        Log?.LogError("Unable write file. Error:", ex.Message);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Log?.LogError(nswagFilePath, ex.Message);
                    return false;
                }

                return true;
            }

            return false;
        }

        private async Task<GeneratorContext?> CreateGenerationContext(
            NswagDocument.NswagDocument nswagDocument,
            string nswagFilePath,
            ReadOnlyDictionary<string, string> variables)
        {
            var documentGenerator = nswagDocument.DocumentGenerator ?? new();

            if (documentGenerator.FromDocument?.Json == null
            && documentGenerator.JsonSchemaToOpenApi == null)
            {
                documentGenerator = new()
                {
                    FromDocument = new()
                    {
                        Json = variables["InputJson"],
                    },
                    Preprocessors = documentGenerator.Preprocessors,
                };
            }

            var result = await _apiDocumentProvider.GetDocumentReaderAsync(documentGenerator);

            if (result is not null && !string.IsNullOrEmpty(result.Error))
            {
                Log?.LogError(result.FilePath ?? nswagFilePath, result.Error!);
                return null;
            }

            var settingsJson = nswagDocument.CodeGenerators.Values.First();

            var variablesWithDefaults = nswagDocument.DefaultVariables is null
                ? variables
                : MargeVariables(nswagDocument.DefaultVariables, variables);

            object? ParseSettings(Type type, JsonSerializer? serializer, IReadOnlyDictionary<string, string>? additionalVars)
            {
                serializer ??= new();
                var mergedVars = additionalVars is null
                    ? variablesWithDefaults
                    : MargeVariables(variables, additionalVars);
                serializer.Converters.Add(new VariableConverter(mergedVars));
                return settingsJson.ToObject(type, serializer);
            }

            return new GeneratorContext(
                ParseSettings,
                _extensions,
                variables)
            {
                DocumentReader = result?.Reader,
                Preprocessors = PreprocessorHelper.GetPreprocessors(_extensions, documentGenerator?.Preprocessors, Log),
                DocumentPath = result?.FilePath,
            };
        }

        private JObject? LoadBaseNswag(string? baseNswagFilePath)
        {
            if (!string.IsNullOrEmpty(baseNswagFilePath))
            {
                if (!_fileProvider.Exists(baseNswagFilePath!))
                {
                    Log?.LogWarning(null, "File '{0}' not found", baseNswagFilePath!);
                    return null;
                }

                using var stream = _fileProvider.OpenRead(baseNswagFilePath!);
                using var reader = new StreamReader(stream);
                using var jreader = new JsonTextReader(reader);
                return JObject.Load(jreader);
            }

            return null;
        }

        private IReadOnlyDictionary<string, string> MargeVariables(IReadOnlyDictionary<string, string> vars, IReadOnlyDictionary<string, string> over)
            => new[] { vars, over }
                .SelectMany(i => i)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(gr => gr.Key, gr => gr.Last());

        private IDictionary<string, string> ParseVariables(string? variables)
        {
            return variables is null
                ? new Dictionary<string, string>()
                : variables
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Split('='))
                    .ToDictionary(i => i[0].Trim(), i => i[1].Trim());
        }
    }
}

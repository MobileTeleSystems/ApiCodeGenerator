using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiCodeGenerator.Core.NswagDocument;
using ApiCodeGenerator.Core.NswagDocument.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Document = ApiCodeGenerator.Core.NswagDocument.NswagDocument;

namespace ApiCodeGenerator.Core
{
    /// <summary>
    /// Предоставляет функции по загрузке документов.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileNameMustMatchTypeName", Justification = "Reviewed.")]
    internal interface INswagDocumentFactory
    {
        /// <summary>
        /// Загружает документ Nswag из файла, заменяя переменные значениями.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="variables">Словарь переменных.</param>
        /// <param name="baseDocument">Документ с базовыми настройками.</param>
        /// <returns>Возвращает загруженный документ.</returns>
        Document LoadNswagDocument(string filePath, IReadOnlyDictionary<string, string>? variables = null, JObject? baseDocument = null);

        /// <summary>
        /// Загружает документ Nswag из ридера, заменяя переменные значениями.
        /// </summary>
        /// <param name="reader">Ридер из которого производится загрузка.</param>
        /// <param name="variables">Словарь переменных.</param>
        /// <param name="baseDocument">Документ с базовыми настройками.</param>
        /// <returns>Возвращает загруженный документ.</returns>
        Document LoadNswagDocument(TextReader reader, IReadOnlyDictionary<string, string>? variables = null, JObject? baseDocument = null);
    }

    /// <summary>
    /// Предоставляет функции по загрузке документов.
    /// </summary>
    internal class NswagDocumentFactory : INswagDocumentFactory
    {
        private readonly IFileProvider _fileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NswagDocumentFactory"/> class.
        /// </summary>
        public NswagDocumentFactory()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NswagDocumentFactory"/> class.
        /// </summary>
        /// <param name="fileProvider">Поставщик данных о файлах.</param>
        /// <param name="apiDocumentloader">Поставщик данных описания API.</param>
        public NswagDocumentFactory(
            IFileProvider? fileProvider)
        {
            _fileProvider = fileProvider ?? new PhysicalFileProvider();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage] // test use other ctor
        public Document LoadNswagDocument(string filePath, IReadOnlyDictionary<string, string>? variables = null, JObject? baseDocument = null)
        {
            using var stream = _fileProvider.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            return LoadNswagDocument(reader, variables, baseDocument);
        }

        /// <inheritdoc />
        public Document LoadNswagDocument(TextReader reader, IReadOnlyDictionary<string, string>? variables = null, JObject? baseDocument = null)
        {
            using var jsonReader = new JsonTextReader(reader);

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>(),
            };

            if (variables?.Any() == true)
                settings.Converters.Add(new VariableConverter(variables));

            var jsonSerializer = JsonSerializer.Create(settings);
            Document? document;
            if (baseDocument is not null)
            {
                var jDocument = JObject.Load(jsonReader);
                MergeDocument(jDocument, baseDocument);
                document = jDocument.ToObject<Document>(jsonSerializer);
            }
            else
            {
                document = jsonSerializer.Deserialize<Document>(jsonReader);
            }

            return document ?? throw new InvalidOperationException("Nswag document is empty.");
        }

        private static void MergeDocument(JObject document, JObject baseDocument)
        {
            var contractResolver = new DefaultContractResolver();

            MergeObject(contractResolver, document, baseDocument, p => nameof(Document.DocumentGenerator), p => nameof(DocumentGenerator.Preprocessors));
            MergeObject(contractResolver, document, baseDocument, p => nameof(Document.CodeGenerators), p => ((JProperty?)p.First)?.Name);
            MergeObject(contractResolver, document, baseDocument, p => nameof(Document.DefaultVariables));

            void MergeObject(IContractResolver contractResolver, JObject doc, JObject baseDoc, params Func<JObject, string?>[] properties)
            {
                var curTargToken = doc;
                var curSrcToken = baseDoc;
                JObject? prevTargObj = null;
                (string? PropertyName, Type? PropertyType) curProp = default;
                var curContract = contractResolver.ResolveContract(typeof(Document));
                foreach (var prop in properties)
                {
                    if (curContract is JsonObjectContract jo)
                    {
                        var jp = jo.Properties.First(jp => jp.UnderlyingName == prop(curTargToken));
                        curProp = (jp.PropertyName, jp.PropertyType);
                    }
                    else if (curContract is JsonDictionaryContract jd)
                    {
                        curProp = (prop(curTargToken), jd.DictionaryValueType);
                    }

                    prevTargObj = curTargToken;
                    if (curProp.PropertyName is not null && curProp.PropertyType is not null)
                    {
                        curTargToken = curTargToken[curProp.PropertyName] as JObject;
                        curSrcToken = curSrcToken[curProp.PropertyName] as JObject;

                        if (curSrcToken is null || curTargToken is null)
                            break;

                        curContract = contractResolver.ResolveContract(curProp.PropertyType);
                    }
                }

                if (prevTargObj is not null && curProp.PropertyName is not null)
                {
                    if (curTargToken is null && curSrcToken is not null)
                    {
                        prevTargObj[curProp.PropertyName] = curSrcToken;
                    }
                    else if (curTargToken is not null && curSrcToken is not null)
                    {
                        curSrcToken.Merge(curTargToken);
                        prevTargObj[curProp.PropertyName] = curSrcToken;
                    }
                }
            }
        }
    }
}

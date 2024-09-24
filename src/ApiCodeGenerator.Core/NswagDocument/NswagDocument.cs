using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.Core.NswagDocument
{
    /// <summary>
    /// Представляет реализацию документа Nswag.
    /// </summary>
    public class NswagDocument
    {
        /// <summary>
        /// Получает и устанавливает значения переменных по умолчанию.
        /// </summary>
        [JsonProperty("defaultVariables", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> DefaultVariables { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Получает и устанавливает настройки получения документа OpenApi.
        /// </summary>
        [JsonProperty("documentGenerator", NullValueHandling = NullValueHandling.Ignore)]
        public DocumentGenerator DocumentGenerator { get; } = new DocumentGenerator();

        /// <summary>
        /// Получает и устанавливает настройки генерации кода.
        /// </summary>
        [JsonProperty("codeGenerators", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JObject> CodeGenerators { get; } = new Dictionary<string, JObject>();
    }
}

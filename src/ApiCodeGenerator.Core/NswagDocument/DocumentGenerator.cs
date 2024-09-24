using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.Core.NswagDocument
{
    /// <summary>
    /// Поддержка секции documentGenerator файла Nswag.
    /// </summary>
    public class DocumentGenerator
    {
        /// <summary>
        /// Поддержка декларации загрузки описания Api из документа.
        /// </summary>
        [JsonProperty("fromDocument", NullValueHandling = NullValueHandling.Ignore)]
        public FromDocument? FromDocument { get; set; }

        /// <summary>
        /// Поддержка загрузки описания из Json-схемы.
        /// </summary>
        [JsonProperty("jsonSchemaToOpenApi", NullValueHandling = NullValueHandling.Ignore)]
        public JsonSchemaToOpenApi? JsonSchemaToOpenApi { get; set; }

        [JsonProperty("preprocessors")]
        public IDictionary<string, JObject?>? Preprocessors { get; set; }
    }
}

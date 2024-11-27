using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiCodeGenerator.Core.NswagDocument
{
    public class FromDocument
    {
        [JsonProperty("json", NullValueHandling = NullValueHandling.Ignore)]
        public string? Json { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string? Url { get; set; }
    }
}

using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class Tag
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; } = default!;

        [JsonProperty("description")]
        public string? Description { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

using ApiCodeGenerator.Core.NswagDocument.Converters;
using Newtonsoft.Json;

namespace ApiCodeGenerator.Core.Tests
{
    public class VariablesConverterTest
    {
        [Test]
        public void ReplaceInStringProperties()
        {
            const string json = "{\"prop\":\"$(var)\"}";
            const string expectedValue = "a1f9d0de-194e-4967-8862-b99dbb9e27a7";
            var variables = new Dictionary<string, string>
            {
                ["var"] = expectedValue,
            };
            var actual = JsonConvert.DeserializeObject<Entity>(json, new VariableConverter(variables));

            Assert.NotNull(actual);
            Assert.AreEqual(expectedValue, actual.Prop);
        }

        internal class Entity
        {
            public string? Prop { get; set; }
        }
    }
}

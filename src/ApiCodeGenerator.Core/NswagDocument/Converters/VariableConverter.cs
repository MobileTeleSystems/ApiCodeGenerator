using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ApiCodeGenerator.Core.NswagDocument.Converters
{
    /// <summary>
    /// Реализует поддержку переменных в процессе загрузки файла Nswag.
    /// </summary>
    internal class VariableConverter : JsonConverter
    {
        private readonly IReadOnlyDictionary<string, string> _variables;
        private readonly Regex _replaceExpression;

        /// <summary>
        /// Создает новый экземпляр класса <see cref="VariableConverter"/>.
        /// </summary>
        /// <param name="variables">Словарь переменных.</param>
        public VariableConverter(IReadOnlyDictionary<string, string> variables)
        {
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));
            _replaceExpression = new Regex($@"\$\(({string.Join("|", variables.Keys)})\)", RegexOptions.Compiled);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
                return _replaceExpression.Replace((string)reader.Value, Replace);

            return null;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private string Replace(Match match)
        {
            var name = match.Groups[1].Value;
            return _variables[name];
        }
    }
}

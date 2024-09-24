using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.Core.Converters
{
    /// <summary>
    /// Реализует загрузку настроек генератора из JSON.
    /// </summary>
    internal class SettingsConverter : JsonConverter
    {
        private readonly Type _type;
        private readonly string[] _unwrapProps;
        private readonly Action<object, string, object?>? _customSetters;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsConverter"/> class.
        /// </summary>
        /// <param name="type">Тип настроек.</param>
        /// <param name="unwrapProps">Свойства которые объекты которых будут равернуты в корневой.</param>
        /// <param name="customSetters">Кастомное действие на обработку свойства настройки. Первый параметр объект настройки, второй имя свойства, третий значение.</param>
        public SettingsConverter(Type type, string[] unwrapProps, Action<object, string, object?>? customSetters)
        {
            _type = type;
            _unwrapProps = unwrapProps;
            _customSetters = customSetters;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == _type;
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var result = Activator.CreateInstance(objectType);
                var props = GetProperties(result);

                while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value?.ToString() ?? string.Empty;
                    props.TryGetValue(propertyName, out (PropertyInfo PropertyInfo, object TargetObject) p);

                    reader.Read();
                    object? value;
                    if (p.PropertyInfo != null)
                    {
                        var converterAttr = p.PropertyInfo.GetCustomAttribute<JsonConverterAttribute>();

                        if (converterAttr != null)
                        {
                            var converter = (JsonConverter)Activator.CreateInstance(converterAttr.ConverterType, converterAttr.ConverterParameters);
                            value = converter.ReadJson(reader, p.PropertyInfo.PropertyType, null, serializer);
                        }
                        else
                        {
                            value = serializer.Deserialize(reader, p.PropertyInfo.PropertyType);
                        }
                    }
                    else
                    {
                        value = serializer.Deserialize(reader, typeof(JToken));
                    }

                    p.PropertyInfo?.SetValue(p.TargetObject, value);
                    _customSetters?.Invoke(result, propertyName, value);
                }

                return result;
            }

            return null;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private IDictionary<string, (PropertyInfo PropertyInfo, object TargetObject)> GetProperties(object target)
        {
            var props = new Dictionary<string, (PropertyInfo PropertyInfo, object TargetObject)>(StringComparer.OrdinalIgnoreCase);
            FillProperties(
                target,
                _unwrapProps);

            return props;

            void FillProperties(object targetObject, params string[] unwrapProp)
            {
                if (targetObject != null)
                {
                    var forUnwrap = new PropertyInfo[unwrapProp.Length];
                    foreach (var p in targetObject.GetType().GetProperties())
                    {
                        var pos = Array.IndexOf(unwrapProp, p.Name);
                        if (pos > -1)
                            forUnwrap[pos] = p;
                        else
                            props[p.Name] = (p, targetObject);
                    }

                    foreach (var item in forUnwrap.Where(pi => pi != null))
                        FillProperties(item.GetValue(targetObject), unwrapProp);
                }
            }
        }
    }
}

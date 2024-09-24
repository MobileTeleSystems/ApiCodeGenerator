using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization
{
    /// <summary>
    /// Абстрактынй конвертер типов в JSON.
    /// </summary>
    /// <typeparam name="T">Конвертируемый тип.</typeparam>
    public class InheritanceConverter<T> : JsonConverter
    {
        private static readonly Dictionary<string, Func<T>> _factories = GetFactories();
        private readonly string _discriminator;
        private readonly string _defaultValue;

        public InheritanceConverter(string discriminator, string defaultValue)
        {
            _discriminator = discriminator;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Не может писать JSON.
        /// </summary>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) => typeof(T) == objectType;

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jobj = Newtonsoft.Json.Linq.JObject.Load(reader);
            var contract = serializer.ContractResolver.ResolveContract(objectType) as JsonObjectContract;
            var discriminatorProperty = contract?.Properties.FirstOrDefault(jp => jp.UnderlyingName == _discriminator);
            if (discriminatorProperty == null)
            {
                throw new InvalidOperationException($"Property '{_discriminator}' not found in type '{objectType}'.");
            }

            var discriminatorValue =
                jobj.GetValue(discriminatorProperty.PropertyName, StringComparison.OrdinalIgnoreCase)?.ToString()
                ?? _defaultValue;

            if (discriminatorValue is not null && _factories.TryGetValue(discriminatorValue, out var factory))
            {
                var target = factory()!;
                serializer.Populate(jobj.CreateReader(), target);
                return target;
            }

            throw new NotSupportedException($"Not supported item type: {discriminatorValue}");
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
        }

        private static Dictionary<string, Func<T>> GetFactories()
        {
            return typeof(T)
                .GetCustomAttributes<KnownTypeAttribute>()
                .ToDictionary(
                    i => i.DiscriminatorValue,
                    GetFactory);

            static Func<T> GetFactory(KnownTypeAttribute attr)
             => () => (T)Activator.CreateInstance(attr.Type);
        }
    }
}

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
        private const BindingFlags CtorBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly Dictionary<string, Func<string, T>> _factories = GetFactories();
        private readonly string _discriminator;
        private readonly string? _defaultValue;

        public InheritanceConverter(string discriminator)
            : this(discriminator, null)
        {
        }

        public InheritanceConverter(string discriminator, string? defaultValue)
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

            // if its ref skip discriminator logic
            if (jobj.TryGetValue("$ref", out var @ref))
            {
                var ctor = objectType.GetConstructor(Type.EmptyTypes)
                        ?? objectType.GetConstructor([typeof(string)])
                        ?? throw new InvalidOperationException($"Ctor not found. Type: {objectType.FullName}");
                var target = ctor.Invoke(ctor.GetParameters().Length == 1 ? [string.Empty] : []);
                serializer.Populate(jobj.CreateReader(), target);
                return target;
            }

            var discriminatorProperty = contract?.Properties.FirstOrDefault(jp => jp.UnderlyingName == _discriminator);
            if (discriminatorProperty is null)
            {
                throw new InvalidOperationException($"Property '{_discriminator}' not found in type '{objectType}'.");
            }

            var discriminatorValue =
                jobj.GetValue(discriminatorProperty.PropertyName, StringComparison.OrdinalIgnoreCase)?.ToString()
                ?? _defaultValue;

            if (discriminatorValue is null)
            {
                throw new JsonSerializationException($"Required property '{discriminatorProperty.PropertyName}' not found in JSON. Path '{reader.Path}'.");
            }

            if (!discriminatorProperty.Writable && discriminatorProperty.PropertyName is not null)
            {
                jobj.Remove(discriminatorProperty.PropertyName);
            }

            if (_factories.TryGetValue(discriminatorValue, out var factory)
                || _factories.TryGetValue("*", out factory))
            {
                var target = factory(discriminatorValue)!;
                serializer.Populate(jobj.CreateReader(), target);
                return target;
            }
            else
            {
                throw new NotSupportedException($"Not supported item type: {discriminatorValue}");
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private static Dictionary<string, Func<string, T>> GetFactories()
        {
            return typeof(T)
                .GetCustomAttributes<KnownTypeAttribute>()
                .ToDictionary(
                    i => i.DiscriminatorValue,
                    GetFactory);

            static Func<string, T> GetFactory(KnownTypeAttribute attr)
            {
                var ctor = attr.Type.GetConstructor(CtorBindingFlags, null, [typeof(string)], null);
                if (ctor is not null)
                {
                    return (t) => (T)ctor.Invoke([t]);
                }
                else
                {
                    ctor = attr.Type.GetConstructor(CtorBindingFlags, null, Type.EmptyTypes, null);
                    return (t) => (T)ctor.Invoke([]);
                }
            }
        }
    }
}

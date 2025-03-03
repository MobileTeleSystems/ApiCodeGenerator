using Newtonsoft.Json;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

internal class RefObjectConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => objectType.IsGenericType &&
                    (objectType.GetGenericTypeDefinition() == typeof(Reference<>) ||
                    objectType.GetGenericTypeDefinition() == typeof(NamedReference<>));

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        reader.Read();
        var refObj = (IJsonReference)Activator.CreateInstance(objectType);
        if (reader.TokenType == JsonToken.PropertyName
            && reader.Value?.ToString() == "$ref")
        {
            var refPath = reader.ReadAsString();
            refObj.ReferencePath = refPath;
            reader.Read();
        }
        else
        {
            var targetType = objectType.GetGenericArguments().Single();
            var target = serializer.Deserialize(new CustomJsonReader(reader), targetType);
            refObj.Reference = (IJsonReference?)target;
        }

        return refObj;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    // Special reader. Wraps original reader and emulate state before call 'Read' in converter
    private sealed class CustomJsonReader : JsonReader
    {
        private readonly JsonReader _reader;
        private bool _readed = false;

        public CustomJsonReader(JsonReader reader)
        {
            _reader = reader;
        }

        public override JsonToken TokenType => _readed ? _reader.TokenType : JsonToken.StartObject;

        public override int Depth => _reader.Depth;

        public override string Path => _reader.Path;

        public override char QuoteChar { get => _reader.QuoteChar; protected set => base.QuoteChar = value; }

        public override object? Value => _reader.Value;

        public override Type? ValueType => _reader.ValueType;

        public override bool Read() => _readed ? _reader.Read() : _readed = true;

        public override void Close() => _reader.Close();

        protected override void Dispose(bool disposing) => ((IDisposable)_reader).Dispose();
    }
}

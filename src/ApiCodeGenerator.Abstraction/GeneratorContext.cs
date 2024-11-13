using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ApiCodeGenerator.Abstraction
{
    public class GeneratorContext
    {
        private readonly Func<Type, JsonSerializer?, IReadOnlyDictionary<string, string>?, object?> _settingsFactory;

        internal GeneratorContext(
            Func<Type, JsonSerializer?, IReadOnlyDictionary<string, string>?, object?> settingsFactory,
            IExtensions extensions,
            IReadOnlyDictionary<string, string> variables)
        {
            _settingsFactory = settingsFactory;
            Extensions = extensions;
            Variables = variables;
        }

        public IReadOnlyDictionary<string, string> Variables { get; }

        public IExtensions Extensions { get; }

        public TextReader? DocumentReader { get; internal set; }

        public Preprocessors? Preprocessors { get; internal set; }

        public string? DocumentPath { get; internal set; }

        public ILogger? Logger { get; internal set; }

        public T? GetSettings<T>(JsonSerializer? jsonSerializer = null, IReadOnlyDictionary<string, string>? additionalVariables = null)
            where T : class
            => (T?)_settingsFactory(typeof(T), jsonSerializer, additionalVariables);
    }
}

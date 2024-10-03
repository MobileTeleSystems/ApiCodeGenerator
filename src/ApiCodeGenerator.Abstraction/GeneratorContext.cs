using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public IReadOnlyDictionary<string, string> Variables { get; set; }

        public IExtensions Extensions { get; set; }

        public TextReader? DocumentReader { get; set; }

        public Preprocessors? Preprocessors { get; set; }

        public string? DocumentPath { get; internal set; }

        public T? GetSettings<T>(JsonSerializer? jsonSerializer, IReadOnlyDictionary<string, string>? additionalVariables)
            where T : class
            => (T?)_settingsFactory(typeof(T), jsonSerializer, additionalVariables);
    }
}

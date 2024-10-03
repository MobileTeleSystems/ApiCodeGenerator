using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiCodeGenerator.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.Core.NswagDocument
{
    internal static class PreprocessorHelper
    {
        public static Preprocessors GetPreprocessors(IExtensions? extensions, IDictionary<string, JObject?>? processors, ILogger? log)
        {
            if (processors == null)
            {
                return new Preprocessors(new Dictionary<Type, Delegate[]>());
            }

            var result = new Dictionary<Type, List<Delegate>>();
            foreach (var name in processors.Keys)
            {
                var value = processors[name]?.ToString(Formatting.None);

                if (extensions?.Preprocessors.TryGetValue(name, out var type) != true)
                {
                    throw new InvalidOperationException($"Preprocessor with name '{name}' not registred.");
                }

                foreach (var (dataType, dlgt) in CreatePreprocessors(name, type, value, log))
                    if (result.TryGetValue(dataType, out var list))
                    {
                        list.Add(dlgt);
                    }
                    else
                    {
                        result[dataType] = new List<Delegate> { dlgt };
                    }
            }

            return new Preprocessors(result.ToDictionary(i => i.Key, i => i.Value.ToArray()));
        }

        private static IEnumerable<(Type DataType, Delegate Processor)> CreatePreprocessors(string? name, Type type, string? procSettings, ILogger? log)
        {
            var methods = type.GetMethods().Where(m => m.Name == "Process");
            if (methods.Any())
            {
                var ctor = type.GetConstructors().First();
                var ctorParams = ctor.GetParameters();
                if (ctorParams.Length > 1 || (ctorParams.Length == 1 && ctorParams[0].ParameterType != typeof(string)))
                {
                    throw new InvalidOperationException($"Constructor with one or zero parameters not found for preprocessor '{name}'");
                }

                var processor = ctor.GetParameters().Length == 0
                    ? Activator.CreateInstance(type)
                    : Activator.CreateInstance(type, procSettings);

                foreach (var method in methods)
                {
                    var tuple = CreatePreprocessor(processor, method);
                    if (tuple is null)
                    {
                        log?.LogWarning(null, "Method '{0}' skiped, because his signature not like Func<T,string,T>.", method.ToString());
                    }
                    else
                    {
                        yield return tuple.Value;
                    }
                }
            }
            else
            {
                log?.LogWarning(null, "Preprocessor '{0}' skiped, because method 'Process' not found.", name!);
            }
        }

        private static (Type DataType, Delegate Processor)? CreatePreprocessor(object processor, MethodInfo method)
        {
            var param = method.GetParameters();
            var retType = method.ReturnType;

            if (retType != typeof(void)
                && param.Length == 2
                && param[0].ParameterType == retType
                && param[1].ParameterType == typeof(string))
            {
                var delegateType = typeof(Func<,,>).MakeGenericType(retType, typeof(string), retType);
                return (retType, method.CreateDelegate(delegateType, processor));
            }

            return null;
        }
    }
}

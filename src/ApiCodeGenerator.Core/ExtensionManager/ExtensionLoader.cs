using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiCodeGenerator.Abstraction;

namespace ApiCodeGenerator.Core.ExtensionManager
{
    /// <summary>
    /// Загрузщик расширений.
    /// </summary>
    internal static class ExtensionLoader
    {
        private const string EXTENSION_CLASS_NAME = "AcgExtension";

        /// <summary>
        /// Загружает расширения из указанных сборок.
        /// </summary>
        /// <param name="asmPaths">Пути к сборкам расширения.</param>
        /// <returns>Возвращает информацию о загруженных расширениях.</returns>
        /// <exception cref="InvalidOperationException">Ошибка инициализации расширения.</exception>
        public static Extensions LoadExtensions(IEnumerable<string> asmPaths)
            => LoadExtensions(asmPaths, LoadAssembly, GetExtensionType);

        // For test.
        internal static Extensions LoadExtensions(
            IEnumerable<string> asmPaths,
            Func<string, Assembly?> assemblyLoader,
            Func<string, Assembly, Type> extensionTypeProvider)
        {
            var codeGenerators = new Dictionary<string, ContentGeneratorFactory>(StringComparer.OrdinalIgnoreCase);
            var operationGenerators = new Dictionary<string, List<Type>>(StringComparer.OrdinalIgnoreCase);
            var preprocessors = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            foreach (var asmPath in asmPaths)
            {
                var asm = assemblyLoader(asmPath)!;
                Type type = extensionTypeProvider.Invoke(asmPath, asm);

                GetAndMergeDict(type, "CodeGenerators", codeGenerators);
                GetAndMergeDictOfList(type, "OperationGenerators", operationGenerators);
                GetAndMergeDict(type, "Preprocessors", preprocessors);
            }

            var result = new Extensions(
                new ReadOnlyDictionary<string, ContentGeneratorFactory>(codeGenerators),
                new ReadOnlyDictionary<string, IReadOnlyCollection<Type>>(
                    operationGenerators.ToDictionary(kv => kv.Key, kv => (IReadOnlyCollection<Type>)new ReadOnlyCollection<Type>(kv.Value))),
                new ReadOnlyDictionary<string, Type>(preprocessors));

            return result;
        }

        private static Assembly? LoadAssembly(string assemblyPath)
            => AssemblyResolver.LoadAssembly(AssemblyName.GetAssemblyName(assemblyPath));

        private static Type GetExtensionType(string asmPath, Assembly asm) => asm.ExportedTypes.FirstOrDefault(t => t.Name == EXTENSION_CLASS_NAME)
                                ?? throw new InvalidOperationException($"Type {EXTENSION_CLASS_NAME} not found in assembly {asmPath}");

        private static void GetAndMergeDict<T>(Type type, string propName, Dictionary<string, T> dict)
        {
            var propInfo = type.GetProperty(propName, BindingFlags.Static | BindingFlags.Public);
            if (propInfo != null)
            {
                if (!typeof(IDictionary<string, T>).IsAssignableFrom(propInfo.PropertyType))
                    throw new InvalidOperationException($"Property {propName} in assembly {type.Assembly.FullName} must return type IDictionary<string, {typeof(T)}>");

                var value = (IDictionary<string, T>)propInfo.GetValue(type, null);
                foreach (var kv in value)
                    dict.Add(kv.Key, kv.Value);
            }
        }

        private static void GetAndMergeDictOfList<T>(Type type, string propName, Dictionary<string, List<T>> dict)
        {
            var propInfo = type.GetProperty(propName, BindingFlags.Static | BindingFlags.Public);
            if (propInfo != null)
            {
                if (!typeof(IDictionary<string, T>).IsAssignableFrom(propInfo.PropertyType))
                    throw new InvalidOperationException($"Property {propName} in assembly {type.Assembly.FullName} must return type IDictionary<string, {typeof(T)}>");

                var value = (IDictionary<string, T>)propInfo.GetValue(type, null);
                if (value.Any())
                {
                    foreach (var kv in value)
                    {
                        if (!dict.TryGetValue(kv.Key, out var target))
                        {
                            dict[kv.Key] = target = new List<T>();
                        }

                        target.Add(kv.Value);
                    }
                }
            }
        }
    }
}

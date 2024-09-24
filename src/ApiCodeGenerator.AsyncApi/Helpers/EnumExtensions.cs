using System.Reflection;
using System.Runtime.Serialization;

namespace ApiCodeGenerator.AsyncApi.Helpers;

public static class EnumExtensions
{
    private static readonly Dictionary<Type, Dictionary<ValueType, string?>> _enumMappings = new();

    public static string ToEnumMemberString<T>(this T @enum)
        where T : struct, Enum
    {
        var mapping = GetMapping(typeof(T));
        mapping.TryGetValue(@enum, out var enumMemberStr);

        return enumMemberStr ?? @enum.ToString();
    }

    private static Dictionary<ValueType, string?> GetMapping(Type type)
    {
        if (!_enumMappings.TryGetValue(type, out var mapping))
        {
            mapping = Enum.GetNames(type)
                .Select(n => type.GetField(n, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .ToDictionary(
                    f => (ValueType)f.GetValue(null)!,
                    f => f.GetCustomAttribute<EnumMemberAttribute>(false)?.Value);

            _enumMappings.Add(type, mapping);
        }

        return mapping;
    }
}

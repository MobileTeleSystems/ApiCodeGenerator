using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

#if ASYNC_API
namespace ApiCodeGenerator.AsyncApi;
#else
namespace ApiCodeGenerator.OpenApi;
#endif

/// <summary>
/// Генератор названий свойств с заменой.
/// </summary>
public class PropertyNameGeneratorWithReplace : IPropertyNameGenerator
{
    private readonly IDictionary<string, string> _replaceOptions;
    private readonly IPropertyNameGenerator _generator = new CSharpPropertyNameGenerator();

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="replaceOptions">Коллекция для замены.</param>
    public PropertyNameGeneratorWithReplace(IDictionary<string, string> replaceOptions)
    {
        _replaceOptions = replaceOptions;
    }

    /// <summary>
    /// Генерация названий свойств с дополнительной заменой.
    /// </summary>
    /// <param name="property">Текущее свойство.</param>
    /// <returns>Название параметра.</returns>
    public string Generate(JsonSchemaProperty property)
    {
        var res = _replaceOptions.Aggregate(property.Name, (current, replaceOption) =>
            current.Replace(replaceOption.Key, replaceOption.Value));

        var j = $"{{\"{res}\":{{ }} }}";
        var schema = JsonSchema.FromSampleJson(j);
        var tempProp = schema.Properties[res];

        return _generator.Generate(tempProp);
    }
}

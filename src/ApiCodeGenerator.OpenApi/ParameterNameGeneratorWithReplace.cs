using System.Collections.Generic;
using System.Linq;
using NSwag;
using NSwag.CodeGeneration;

namespace ApiCodeGenerator.OpenApi;

/// <summary>
/// Заменяет части названий параметров.
/// </summary>
public class ParameterNameGeneratorWithReplace : IParameterNameGenerator
{
    private readonly IDictionary<string, string> _replaceMap;
    private readonly IParameterNameGenerator _generator;

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="replaceMap">коллекция параметров для замены.</param>
    /// <param name="baseGenerator">Базовый генератор которому передается наименование после замены.</param>
    public ParameterNameGeneratorWithReplace(IDictionary<string, string> replaceMap, IParameterNameGenerator baseGenerator)
    {
        _replaceMap = replaceMap;
        _generator = baseGenerator;
    }

    /// <summary>
    /// Генерация названий параметров с дополнительной заменой.
    /// </summary>
    /// <param name="parameter">Текущий параметр.</param>
    /// <param name="allParameters">Все параметры.</param>
    /// <returns>Название параметра.</returns>
    public string Generate(OpenApiParameter parameter, IEnumerable<OpenApiParameter> allParameters)
    {
        var res = _replaceMap.Aggregate(parameter.Name, (current, replaceOption) =>
            current.Replace(replaceOption.Key, replaceOption.Value));
        var tempParam = new OpenApiParameter() { Name = res };
        return _generator.Generate(tempParam, allParameters);
    }
}

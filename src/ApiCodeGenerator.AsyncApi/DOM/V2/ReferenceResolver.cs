using ApiCodeGenerator.AsyncApi.DOM.Serialization;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal sealed class ReferenceResolver
{
    private readonly JObject _rootObject;

    internal ReferenceResolver(JObject rootObject)
    {
        _rootObject = rootObject;
    }

    internal string? GetServerRefByName(string name)
    {
        var serverDefinitions1 = _rootObject["servers"]?.Children<JProperty>();
        var serverDefinitions2 = _rootObject["components"]?["servers"]?.Children<JProperty>();
        var serverDefinitions = (serverDefinitions1, serverDefinitions2) switch
        {
            (null, not null) => serverDefinitions2,
            (not null, null) => serverDefinitions1,
            (not null, not null) => serverDefinitions1.Value.Concat(serverDefinitions2),
            _ => null,
        };

        return GetRef(serverDefinitions.FirstOrDefault(d => d.Name == name));
    }

    /// <summary>
    /// Search channel referenced to channel component.
    /// </summary>
    /// <param name="refPath">Reference path.</param>
    /// <returns>Name of channel item</returns>
    internal string GetChannelIdByRefPath(string refPath)
    {
        var refs = _rootObject["channels"]?
            .Children<JProperty>()
            .Where(p =>
            {
                var chDef = p.Parent![p.Name]!;
                return chDef.Type == JTokenType.Object && chDef["$ref"]?.ToString() == refPath;
            })
            .ToArray();

        if (refs?.Any() != true)
        {
            throw new AsyncApiSerializationException($"Channel with reference '{refPath}' was not found.");
        }
        else if (refs.Length > 1)
        {
            throw new AsyncApiSerializationException($"More than one channel with the '{refPath}' link was found.");
        }
        else
        {
            return refs[0].Name;
        }
    }

    internal Reference<Security.SecurityScheme>[]? GetSecuritySchemeRefs(SecurityRequirement[]? requirements, string path)
    {
        if (requirements is null)
        {
            return null;
        }

        var names = requirements.SelectMany(i => i.Keys).Distinct();
        var result = new List<Reference<Security.SecurityScheme>>(requirements.Length);
        var securitySchemes = _rootObject["components"]?["securitySchemes"]?.Children<JProperty>().Select(p => p.Name).ToArray();
        foreach (var name in names)
        {
            if (securitySchemes?.Contains(name) == true)
            {
                result.Add(new() { ReferencePath = $"#/components/securitySchemes/{name}" });
            }
            else
            {
                throw new AsyncApiSerializationException($"Security scheme '{name}' not found. Path: {path}");
            }
        }

        return result.ToArray();
    }

    private string? GetRef(JToken? token)
    {
        var segments = GetSegments();
        if (segments.Any())
        {
            return string.Join("/", segments.Reverse().Prepend("#"));
        }

        return null;

        IEnumerable<string> GetSegments()
        {
            while (token is not null)
            {
                if (token is JProperty prop)
                {
                    yield return prop.Name;
                }

                token = token.Parent;
            }
        }
    }
}

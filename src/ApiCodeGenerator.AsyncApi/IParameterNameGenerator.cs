namespace ApiCodeGenerator.AsyncApi;

/// <summary>The parameter name generator interface.</summary>
public interface IParameterNameGenerator
{
    /// <summary>Generates the parameter name for the given parameter.</summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="allParameters">All parameters.</param>
    /// <returns>Generated parameter name.</returns>
    string Generate(string parameterName, DOM.Parameter parameter, IEnumerable<DOM.Parameter> allParameters);
}

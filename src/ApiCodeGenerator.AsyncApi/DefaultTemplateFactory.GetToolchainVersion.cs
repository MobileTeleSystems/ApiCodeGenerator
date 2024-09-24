using System;

namespace ApiCodeGenerator.AsyncApi;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Reviewed.")]
public partial class DefaultTemplateFactory
{
    private static readonly string VERSION = typeof(DefaultTemplateFactory).Assembly.GetName().Version.ToString();

    protected virtual string GetToolchainVersion(Func<string> @base)
        => $"{VERSION} (NJsonSchema v{NJsonSchema.JsonSchema.ToolchainVersion})";
}

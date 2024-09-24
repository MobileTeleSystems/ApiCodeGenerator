using System;

namespace ApiCodeGenerator.OpenApi;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Reviewed.")]
public partial class DefaultTemplateFactory
{
    protected virtual string GetToolchainVersion(Func<string> @base)
        => @base();
}

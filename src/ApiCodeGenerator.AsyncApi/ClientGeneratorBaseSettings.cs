using ApiCodeGenerator.AsyncApi.OperationNameGenerators;
using NJsonSchema;
using NJsonSchema.CodeGeneration;

namespace ApiCodeGenerator.AsyncApi
{
    /// <summary>Settings for the ClientGeneratorBase.</summary>
    public abstract class ClientGeneratorBaseSettings
    {
        /// <summary>Initializes a new instance of the <see cref="ClientGeneratorBaseSettings"/> class.</summary>
        protected ClientGeneratorBaseSettings()
        {
            OperationNameGenerator = new SingleClientFromOperationId();
            ParameterNameGenerator = new DefaultParameterNameGenerator();

            ExcludedParameterNames = new string[0];
        }

        /// <summary>Gets the code generator settings.</summary>
        public abstract CodeGeneratorSettingsBase CodeGeneratorSettings { get; }

        /// <summary>Gets or sets the class name of the service client or controller.</summary>
        public string ClassName { get; set; } = "Client";

        /// <summary>Gets or sets a value indicating whether to generate DTO classes (default: true).</summary>
        public bool GenerateDtoTypes { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether to generate interfaces for the client classes (default: false).</summary>
        public bool GenerateClientInterfaces { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate client types (default: true).</summary>
        public bool GenerateClientClasses { get; set; } = true;

        /// <summary>Gets or sets the operation name generator.</summary>
        public IOperationNameGenerator OperationNameGenerator { get; set; }

        /// <summary>Gets or sets a value indicating whether to reorder parameters (required first, optional at the end) and generate optional parameters.</summary>
        public bool GenerateOptionalParameters { get; set; }

        /// <summary>Gets or sets the parameter name generator.</summary>
        public IParameterNameGenerator ParameterNameGenerator { get; set; }

        /// <summary>Gets or sets the globally excluded parameter names.</summary>
        public string[] ExcludedParameterNames { get; set; }
    }
}

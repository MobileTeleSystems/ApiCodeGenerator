using ApiCodeGenerator.AsyncApi.CSharp.Models;
using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.CSharp
{
    public abstract class CSharpGeneratorBase<TSettings> : AsyncApiGeneratorBase
        where TSettings : CSharpGeneratorBaseSettings
    {
        protected CSharpGeneratorBase(AsyncApiDocument document, CSharpGeneratorBaseSettings settings, CSharpTypeResolver resolver)
            : base(document, settings, resolver)
        {
        }

        protected new CSharpTypeResolver Resolver => (CSharpTypeResolver)base.Resolver;

        protected TSettings Settings => (TSettings)BaseSettings;

        protected IList<string> Usages { get; } = [];

        public static TypeResolverBase CreateResolver(AsyncApiDocument apiDocument, TSettings settings)
        {
            var schemas = apiDocument.Components?.Schemas
                ?? new Dictionary<string, NJsonSchema.JsonSchema>();
            schemas.TryGetValue("Exception", out var exceptionSchema);
            var resolver = new CSharpTypeResolver(settings.CSharpGeneratorSettings, exceptionSchema);
            resolver.RegisterSchemaDefinitions(exceptionSchema is null
                ? schemas
                : schemas.Where(i => i.Value != exceptionSchema).ToDictionary(i => i.Key, i => i.Value));
            return resolver;
        }

        protected override IEnumerable<CodeArtifact> GenerateAllClientTypes()
        {
            var operations = CreateOperationModels().ToArray();
            var operationsByClients = from o in operations
                                      group o by o.ControllerName into gr
                                      select (ClassName: CreateControllerName(gr.Key), Operations: gr.ToArray());

            foreach (var ca in operationsByClients.SelectMany(d => GenerateClientTypes(d.ClassName, d.Operations)))
            {
                yield return ca;
            }
        }

        protected virtual IEnumerable<CodeArtifact> GenerateClientTypes(string className, CSharpOperationModel[] operations)
        {
            var model = CreateClientModel(className, operations);
            if (Settings.GenerateClientInterfaces)
            {
                yield return GenerateInterfaceType(model);
            }

            if (Settings.GenerateClientClasses)
            {
                yield return GenerateClientType(model);
            }
        }

        protected virtual CodeArtifact GenerateClientType(CSharpClientTemplateModel model)
        {
            var template = Settings.CodeGeneratorSettings.TemplateFactory.CreateTemplate("CSharp", "Client.Class", model);

            return new CodeArtifact(model.Class, CodeArtifactType.Class, CodeArtifactLanguage.CSharp, CodeArtifactCategory.Client, template);
        }

        protected virtual CodeArtifact GenerateInterfaceType(CSharpClientTemplateModel model)
        {
            var template = Settings.CodeGeneratorSettings.TemplateFactory.CreateTemplate("CSharp", "Client.Interface", model);

            return new CodeArtifact(model.Interface, CodeArtifactType.Interface, CodeArtifactLanguage.CSharp, CodeArtifactCategory.Client, template);
        }

        protected abstract CSharpClientTemplateModel CreateClientModel(string className, CSharpOperationModel[] operations);

        protected virtual string CreateControllerName(string controllerName)
            => Settings.ClassName.Replace("{controllerName}", controllerName);

        protected override IEnumerable<CodeArtifact> GenerateDtoTypes()
        {
            var generator = new CSharpGenerator(Document, Settings.CSharpGeneratorSettings, Resolver);
            return generator.GenerateTypes();
        }

        protected IEnumerable<CSharpOperationModel> CreateOperationModels()
        {
            if (Document.Channels is not null)
            {
                foreach (var channel in Document.Channels)
                {
                    if (channel.Value.Publish is not null && Settings.OperationTypes.HasFlag(OperationTypes.Publish))
                    {
                        yield return CreateOperationModelInternal(channel.Key, channel.Value, channel.Value.Publish);
                    }

                    if (channel.Value.Subscribe is not null && Settings.OperationTypes.HasFlag(OperationTypes.Subscribe))
                    {
                        yield return CreateOperationModelInternal(channel.Key, channel.Value, channel.Value.Subscribe);
                    }
                }
            }
        }

        protected virtual CSharpOperationModel CreateOperationModel(string name, string channelName, Channel channel, Operation operation)
        {
            return new CSharpOperationModel(name, channelName, channel, operation, Settings, Resolver);
        }

        protected override string GenerateFile(IEnumerable<CodeArtifact> clientTypes, IEnumerable<CodeArtifact> dtoTypes, ClientGeneratorOutputType outputType)
        {
            if (Usages.Any())
            {
                if (outputType == ClientGeneratorOutputType.Contracts)
                {
                    Settings.AdditionalContractNamespaceUsages = Usages.Union(Settings.AdditionalContractNamespaceUsages).ToArray();
                }
                else
                {
                    Settings.AdditionalNamespaceUsages = Usages.Union(Settings.AdditionalNamespaceUsages).ToArray();
                }
            }

            var model = new CSharpFileTemplateModel(clientTypes, dtoTypes, outputType, Document, Settings, this, Resolver);
            FillDisabledWarningsList(model.DisabledWarnings);
            var template = Settings.CodeGeneratorSettings.TemplateFactory.CreateTemplate("CSharp", "File", model);
            return template.Render();
        }

        /// <summary>
        /// Заполняет список отключаемых предупреждений.
        /// </summary>
        /// <param name="warnings">Список пар Идентификатор - Описание.</param>
        protected virtual void FillDisabledWarningsList(IDictionary<string, string> warnings)
        {
        }

        private CSharpOperationModel CreateOperationModelInternal(string channelPath, Channel channel, Operation operation)
        {
            var operationName = Settings.OperationNameGenerator.GetOperationName(Document, channelPath, channel.Subscribe == operation, operation);
            var operationModel = CreateOperationModel(operationName, channelPath, channel, operation);
            operationModel.ControllerName = Settings.OperationNameGenerator.GetClientName(Document, channelPath, channel.Subscribe == operation, operation);
            return operationModel;
        }
    }
}

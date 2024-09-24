using System;
using System.Collections.Generic;
using System.Text;
using ApiCodeGenerator.Abstraction;

namespace ApiCodeGenerator.Core.Tests.Infrastructure
{
    internal class FakeCodeGenerator : IContentGenerator
    {
        public const string FileContent = nameof(FakeCodeGenerator);

        public FakeCodeGenerator(GeneratorContext context, Dictionary<string, string>? additionalVars = null)
        {
            Context = context;
            Settings = context.GetSettings<FakeCodeGeneratorSettings>(additionalVariables: additionalVars);
        }

        public GeneratorContext Context { get; }

        public FakeCodeGeneratorSettings? Settings { get; }

        public static Task<IContentGenerator> CreateAsync(GeneratorContext context) => Task.FromResult<IContentGenerator>(new FakeCodeGenerator(context));

        public virtual string Generate()
        {
            return FileContent;
        }
    }
}

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using ApiCodeGenerator.Abstraction;

namespace ApiCodeGenerator.MSBuild
{
    internal class GenerateCommand : RootCommand
    {
        private const string NswagToolsDirName = "NSwag.Tool";
        private readonly Option<string?> _baseNswagOption;
        private readonly Option<string[]> _extensionOption;
        private readonly Argument<string> _openApiArgument;
        private readonly Argument<string> _outputArg;
        private readonly Argument<string> _nswagArgument;
        private readonly Option<string?> _nswagToolPath;
        private readonly Option<string?> _variablesOption;

        public GenerateCommand()
            : base()
        {
            _baseNswagOption = new("-b", "Base nswag file");
            AddOption(_baseNswagOption);

            _openApiArgument = new("openApi", "Path to OpenApi document.");
            AddArgument(_openApiArgument);

            _nswagArgument = new("nswag", "Path to nswag file");
            AddArgument(_nswagArgument);

            _outputArg = new("output", "Path to output file");
            AddArgument(_outputArg);

            _extensionOption = new("-e", "Path to extension file");
            AddOption(_extensionOption);

            _variablesOption = new("-v", "variables");
            AddOption(_variablesOption);

            _nswagToolPath = new("--nswagTool", "Path to NSwag.Tool directory");
            AddOption(_nswagToolPath);

            this.SetHandler(ExecuteAsync);
        }

        private Task ExecuteAsync(InvocationContext context)
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
            var nswagFile = context.ParseResult.GetValueForArgument(_nswagArgument);
            var baseNswagFile = context.ParseResult.GetValueForOption(_baseNswagOption);
            var nswagToolPath = context.ParseResult.GetValueForOption(_nswagToolPath);
            var openApiFile = context.ParseResult.GetValueForArgument(_openApiArgument);
            var outFile = context.ParseResult.GetValueForArgument(_outputArg);
            var variables = context.ParseResult.GetValueForOption(_variablesOption);
            var extPaths = context.ParseResult.GetValueForOption(_extensionOption);

            var factory = GetGenerationTaskFactory(nswagToolPath);
            AddExtesionsProbingPaths(extPaths);
            var generator = factory.Create(extPaths, new ConsoleLogAdapter());
            return generator.ExecuteAsync(nswagFile, openApiFile, outFile, variables, baseNswagFile);
        }

        private IGenerationTaskFactory GetGenerationTaskFactory(string? nswagToolsPath)
        {
            var context = AssemblyLoadContext.GetLoadContext(Assembly.GetCallingAssembly())!;

            // регистриуем процесс резолва сборок
            AssemblyResolver.Register(context);

            var thisAssemblyPath = GetThisAssemblyPath();
            AssemblyResolver.AddProbingPath(nswagToolsPath ?? Path.Combine(thisAssemblyPath, NswagToolsDirName));
            AssemblyResolver.AddProbingPath(thisAssemblyPath);

            var coreAsm = context.LoadFromAssemblyName(new AssemblyName("ApiCodeGenerator.Core"));
            var factoryType = coreAsm.GetType("ApiCodeGenerator.Core.GenerationTaskFactory")!;
            return (IGenerationTaskFactory)Activator.CreateInstance(factoryType)!;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:Static elements should appear before instance elements", Justification = "Not need")]
        private static void AddExtesionsProbingPaths(string[]? extensions)
        {
            if (extensions?.Any() == true)
            {
                var dirs = extensions.Select(i => Path.GetDirectoryName(Path.GetFullPath(i))!).Distinct();
                foreach (var dir in dirs)
                {
                    AssemblyResolver.AddProbingPath(dir);
                }
            }
        }

        private static string GetThisAssemblyPath()
        {
            var thisAsmPath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(thisAsmPath)!;
        }
    }
}

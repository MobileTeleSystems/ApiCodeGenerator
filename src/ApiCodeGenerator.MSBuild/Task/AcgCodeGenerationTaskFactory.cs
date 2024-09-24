using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using ApiCodeGenerator.Abstraction;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ApiCodeGenerator.MSBuild;

/// <summary>
/// Загрузщик задачи генерации, управляет процессом поиска и загрузки тулинга и расширений.
/// </summary>
public class AcgCodeGenerationTaskFactory : ITaskFactory
{
    private IGenerationTaskFactory? _generationTaskFactory;
    private string[]? _extensions;

    public string FactoryName => nameof(AcgCodeGenerationTaskFactory);

    public Type TaskType { get; set; } = typeof(ApiCodeGenerationTask);

    public void CleanupTask(ITask task)
    {
        AssemblyResolver.Unregister();
    }

    public ITask CreateTask(IBuildEngine taskFactoryLoggingHost)
    {
        var extensions = _generationTaskFactory!.LoadExtensions(_extensions ?? []);

        var task = Activator.CreateInstance(TaskType, new object?[] { _generationTaskFactory, extensions });
        return task! as ITask ?? throw new InvalidOperationException($"'{TaskType}' not implement ITask");
    }

    public TaskPropertyInfo[] GetTaskParameters()
    {
        var query = from propertyInfo in TaskType.GetProperties()
                    let output = propertyInfo.GetCustomAttribute<OutputAttribute>()
                    let required = propertyInfo.GetCustomAttribute<RequiredAttribute>()
                    select new TaskPropertyInfo(propertyInfo.Name, propertyInfo.PropertyType, output != null, required != null);
        return query.ToArray();
    }

    public bool Initialize(string taskName, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost)
    {
        var log = new TaskLoggingHelper(taskFactoryLoggingHost, taskName);
        if (taskName != "ApiCodeGenerationTask")
        {
            log.LogError("Unknown task: {0}", taskName);
            return false;
        }

        var (nswagToolsPath, extensions) = GetFactoryParameters(taskBody);
        if (string.IsNullOrEmpty(nswagToolsPath))
        {
            log.LogError("Add Nswag.MSBuild to project");
            return false;
        }

        if (!Directory.Exists(nswagToolsPath))
        {
            log.LogError("Directory {0} not found.", nswagToolsPath);
            return false;
        }

        // регистриуем процесс резолва сборок
        AssemblyResolver.Register(null);

        // добавляем в пути поиска папку инструментов Nswag и папку с задачами
        AssemblyResolver.AddProbingPath(SelectToolsFrameworkFolder(nswagToolsPath!));
        AssemblyResolver.AddProbingPath(GetTasksPath());
        _generationTaskFactory = GetGenerationTaskFactory(nswagToolsPath!);

        TaskType = typeof(ApiCodeGenerator.MSBuild.ApiCodeGenerationTask);

        // добавляем папки с расширениями
        if (!string.IsNullOrEmpty(extensions))
        {
            var items = extensions!.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var dirs = items.Select(i => Path.GetDirectoryName(Path.GetFullPath(i))).Distinct();
            foreach (var dir in dirs)
            {
                AssemblyResolver.AddProbingPath(dir);
            }

            _extensions = [.. items];
        }

        return true;
    }

    private static string GetThisAssemblyPath()
    {
        var thisAsmPath = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(thisAsmPath)!;
    }

    private IGenerationTaskFactory GetGenerationTaskFactory(string nswagToolsPath)
    {
        var assemblyLoadContextType = Type.GetType("System.Runtime.Loader.AssemblyLoadContext, System.Runtime.Loader");
        var context = assemblyLoadContextType == null
            ? AppDomain.CurrentDomain
            : assemblyLoadContextType.GetMethod("GetLoadContext").Invoke(null, [Assembly.GetCallingAssembly()]);

        // регистриуем процесс резолва сборок
        AssemblyResolver.Register(context);

        var thisAssemblyPath = GetThisAssemblyPath();
        AssemblyResolver.AddProbingPath(SelectToolsFrameworkFolder(nswagToolsPath));
        AssemblyResolver.AddProbingPath(thisAssemblyPath);

        var coreAsm = AssemblyResolver.LoadAssembly(new AssemblyName("ApiCodeGenerator.Core"))!;
        var factoryType = coreAsm.GetType("ApiCodeGenerator.Core.GenerationTaskFactory")!;
        return (IGenerationTaskFactory)Activator.CreateInstance(factoryType)!;
    }

    private (string? NswagToolsPath, string? Extensions) GetFactoryParameters(string taskBody)
    {
        var doc = XDocument.Parse($"<Task>{taskBody}</Task>");
        var @params = doc.Root.Elements();
        var nsawgToolsPath = doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "NswagToolsPath")?.Value;
        var extensions = doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "Extensions")?.Value;
        return (nsawgToolsPath, extensions);
    }

    private string GetTasksPath()
    {
        var thisAsmPath = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(thisAsmPath);
    }

    private string[] SelectToolsFrameworkFolder(string path)
    {
        // Для NET >= 5 добавляем в список источников папку с фрейворком AspNetCore
        // т.к. nswag подключает его, а msbuild нет.
        if (RuntimeInformation.FrameworkDescription.StartsWith(".NET 6"))
        {
            return new[]
            {
                Path.Combine(path, "tools", "Net60"),
                GetAspSharedFrameworkFolder("6.0.*"),
            };
        }
        else if (RuntimeInformation.FrameworkDescription.StartsWith(".NET 5"))
        {
            return new[]
            {
                Path.Combine(path, "tools", "Net50"),
                GetAspSharedFrameworkFolder("6.0.*"),
            };
        }

        return new[] { Path.Combine(path, "tools", "Win") };

        string GetAspSharedFrameworkFolder(string version)
        {
            var asm = Assembly.Load("System.Threading");
            var shared = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(asm.Location)));
            var asp = Path.Combine(shared, "Microsoft.AspNetCore.App");
            return Directory.GetDirectories(asp, version).OrderBy(_ => _).Last();
        }
    }
}

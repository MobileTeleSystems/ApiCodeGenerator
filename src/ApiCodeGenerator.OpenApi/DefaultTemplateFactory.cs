using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NJsonSchema.CodeGeneration;

#if ASYNC_API
namespace ApiCodeGenerator.AsyncApi;
#else
namespace ApiCodeGenerator.OpenApi;
#endif

/// <summary>
/// Реализация фабрики шаблонов.
/// </summary>
/// <remarks>
/// Данная фабрика кэширует шаблоны не только по имени но и по источнику (это исключает влияние одноименных шаблонов в разных генераторах друг на друга).
/// Так же добавлена поддержка ввызова базового шаблона при перегрузки добавлением суфикса ".base" к имени шаблона.
/// </remarks>
public partial class DefaultTemplateFactory : ITemplateFactory
{
    private const string BASE_TMPL_SUFFIX = ".base";

    private readonly ITemplateProvider[] _providers;

    private readonly InternalTemplateFactory _internalTemplateFactory;

    public DefaultTemplateFactory(CodeGeneratorSettingsBase settings, params Assembly[] assemblies)
        : this(settings, CreateProviders(settings, assemblies).ToArray())
    {
    }

    public DefaultTemplateFactory(CodeGeneratorSettingsBase settings, ITemplateProvider[] providers)
    {
        _providers = providers;
        _internalTemplateFactory = new(settings, GetLiquidTemplate, GetToolchainVersion);
    }

    public static IEnumerable<ITemplateProvider> CreateProviders(CodeGeneratorSettingsBase settings, Assembly[] assemblies)
    {
        if (!string.IsNullOrEmpty(settings.TemplateDirectory))
        {
            yield return new DirectoryTemplateProvider(settings.TemplateDirectory!);
        }

        foreach (var assembly in assemblies)
        {
            yield return new EmbededResourceTemplateProvider(assembly, $"{assembly.GetName().Name}.Templates");
        }
    }

    public ITemplate CreateTemplate(string language, string template, object model)
    {
        IEnumerable<ITemplateProvider> providers = _providers;
        if (template.EndsWith(BASE_TMPL_SUFFIX))
        {
            if (model is Fluid.TemplateContext templateContext)
            {
                var currentTemplate = (string)templateContext.AmbientValues["__template"];
                template = template.Substring(0, template.Length - BASE_TMPL_SUFFIX.Length);
                if (!template.EndsWith("!"))
                {
                    template += "!";
                }

                providers = providers.SkipWhile(p => currentTemplate != p.GetFullName(template, language)).Skip(1);
            }
        }

        var fullName = providers.Select(p => p.GetFullName(template, language)).FirstOrDefault(n => !string.IsNullOrEmpty(n));
        fullName ??= string.Empty;

        if (!fullName.EndsWith("!"))
        {
            fullName += '!';
        }

        return _internalTemplateFactory.CreateTemplate(language, fullName, model);
    }

    private string GetLiquidTemplate(string language, string name)
    {
        var text = _providers
            .Select(p => p.GetTemplateText(name.TrimEnd('!'), language))
            .FirstOrDefault(t => t is not null);

        return text ?? string.Empty;
    }

    private sealed class InternalTemplateFactory
#if ASYNC_API
        : NJsonSchema.CodeGeneration.DefaultTemplateFactory
#else
        : NSwag.CodeGeneration.DefaultTemplateFactory
#endif
    {
        private readonly Func<string, string, string> _getLiquidTemplate;
        private readonly Func<Func<string>, string>? _getToolchainVersion;

        public InternalTemplateFactory(
            CodeGeneratorSettingsBase settings,
            Func<string, string, string> getLiquidTemplate,
            Func<Func<string>, string>? getToolchainVersion)
                : base(settings, [])
        {
            _getLiquidTemplate = getLiquidTemplate;
            _getToolchainVersion = getToolchainVersion;
        }

        protected override string GetEmbeddedLiquidTemplate(string language, string template)
        {
            return _getLiquidTemplate(language, template);
        }

        protected override string GetToolchainVersion()
            => _getToolchainVersion is null
                ? base.GetToolchainVersion()
                : _getToolchainVersion(base.GetToolchainVersion);
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Ordering Rules", "SA1201", Justification = "none")]
public interface ITemplateProvider
{
    string? GetFullName(string name, string language);

    string? GetTemplateText(string fullName, string language);
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Ordering Rules", "SA1402", Justification = "none")]
public class EmbededResourceTemplateProvider : ITemplateProvider
{
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;

    public EmbededResourceTemplateProvider(Assembly assembly, string resourcePrefix)
    {
        _assembly = assembly;
        _resourcePrefix = resourcePrefix;
    }

    public string? GetFullName(string name, string language)
    {
        var resourceName = $"{_resourcePrefix}.{name}.liquid";
        return _assembly.GetManifestResourceNames().Contains(resourceName)
            ? resourceName
            : null;
    }

    public string? GetTemplateText(string resourceName, string language)
    {
        var resource = _assembly.GetManifestResourceStream(resourceName);
        if (resource != null)
        {
            using var reader = new StreamReader(resource);
            return reader.ReadToEnd();
        }

        return null;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Ordering Rules", "SA1402", Justification = "none")]
public class DirectoryTemplateProvider : ITemplateProvider
{
    private readonly string _path;

    public DirectoryTemplateProvider(string path)
    {
        _path = path;
    }

    public string? GetFullName(string name, string language)
    {
        if (!name.EndsWith("!"))
        {
            var templateFilePath = Path.Combine(_path, name + ".liquid");
            return File.Exists(templateFilePath)
                ? templateFilePath
                : null;
        }

        return null;
    }

    public string? GetTemplateText(string templateFilePath, string language)
    {
        if (File.Exists(templateFilePath))
        {
            return File.ReadAllText(templateFilePath);
        }

        return null;
    }
}

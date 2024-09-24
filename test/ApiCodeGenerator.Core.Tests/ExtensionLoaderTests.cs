using System.Reflection;
using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.Core.ExtensionManager;

namespace ApiCodeGenerator.Core.Tests;

public class ExtensionLoaderTests
{
    [Test]
    public void LoadExtensions()
    {
        var extensions = ExtensionLoader.LoadExtensions(
            [nameof(AcgExt1), nameof(AcgExt2)],
            a => Assembly.GetCallingAssembly(),
            (ap, asm) => GetType().GetNestedType(ap, BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Type {ap} not found"));

        Assert.That(extensions.CodeGenerators, Does.ContainKey("gen1"));
        Assert.AreEqual(new ContentGeneratorFactory(Gen1).Method, extensions.CodeGenerators["gen1"].Method);
        Assert.That(extensions.CodeGenerators, Does.ContainKey("gen2"));
        Assert.AreEqual(new ContentGeneratorFactory(Gen2).Method, extensions.CodeGenerators["gen2"].Method);

        Assert.That(extensions.Preprocessors, Does.ContainKey("pp1"));
        Assert.AreEqual(typeof(Preproc1), extensions.Preprocessors["pp1"]);
        Assert.That(extensions.Preprocessors, Does.ContainKey("pp2"));
        Assert.AreEqual(typeof(Preproc2), extensions.Preprocessors["pp2"]);

        Assert.That(extensions.OperationGenerators, Does.ContainKey("og1"));
        Assert.That(extensions.OperationGenerators["og1"], Has.Exactly(2).EqualTo(typeof(OperGen1)));
        Assert.That(extensions.OperationGenerators, Does.ContainKey("og2"));
        Assert.That(extensions.OperationGenerators["og2"], Has.Exactly(1).EqualTo(typeof(OperGen2)));
    }

#pragma warning disable IDE0060 // Remove unused parameter
    internal static Task<IContentGenerator> Gen1(GeneratorContext context) => default!;

    internal static Task<IContentGenerator> Gen2(GeneratorContext context) => default!;
#pragma warning restore IDE0060 // Remove unused parameter

    internal static class AcgExt1
    {
        public static Dictionary<string, ContentGeneratorFactory> CodeGenerators { get; } = new()
        {
            ["gen1"] = Gen1,
        };

        public static Dictionary<string, Type> OperationGenerators { get; } = new()
        {
            ["og1"] = typeof(OperGen1),
        };
    }

    internal static class AcgExt2
    {
        public static Dictionary<string, ContentGeneratorFactory> CodeGenerators { get; } = new()
        {
            ["gen2"] = Gen2,
        };

        public static Dictionary<string, Type> Preprocessors { get; } = new()
        {
            ["pp1"] = typeof(Preproc1),
            ["pp2"] = typeof(Preproc2),
        };

        public static Dictionary<string, Type> OperationGenerators { get; } = new()
        {
            ["og1"] = typeof(OperGen1),
            ["og2"] = typeof(OperGen2),
        };
    }

#pragma warning disable SA1202 // Elements should be ordered by access
    public class Preproc1
    {
    }

    public class Preproc2
    {
    }

    public class OperGen1
    {
    }

    public class OperGen2
    {
    }
#pragma warning restore SA1202 // Elements should be ordered by access
}

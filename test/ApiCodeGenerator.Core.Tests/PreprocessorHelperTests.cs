using System.Diagnostics;
using System.Reflection;
using Moq;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.Core.Tests;

public class PreprocessorHelperTests
{
    [Test]
    public void GetPreprocessors_Success()
    {
        var extensions = new ExtensionManager.Extensions(preprocessors: new Dictionary<string, Type>
        {
            ["pp1"] = typeof(Preprocessor),
        });

        var ppDeclartaions = new Dictionary<string, JObject?>
        {
            ["pp1"] = new JObject(),
        };

        var preprocessors = PreprocessorHelper.GetPreprocessors(extensions, ppDeclartaions, null);

        Assert.AreEqual(1, preprocessors.Count);
        Assert.That(preprocessors, Does.ContainKey(typeof(string)));
        Assert.AreEqual(1, preprocessors[typeof(string)].Length);
        Assert.AreEqual(typeof(Preprocessor).GetMethod("Process"), preprocessors[typeof(string)][0].Method);
    }

    [Test]
    public void GetPreprocessors_WithTwoProcessMethod()
    {
        var extensions = new ExtensionManager.Extensions(preprocessors: new Dictionary<string, Type>
        {
            ["pp"] = typeof(Preprocessor2),
        });

        var ppDeclartaions = new Dictionary<string, JObject?>
        {
            ["pp"] = new JObject(),
        };

        var preprocessors = PreprocessorHelper.GetPreprocessors(extensions, ppDeclartaions, null);

        Assert.AreEqual(2, preprocessors.Count);
        Assert.Multiple(() =>
        {
            Assert.That(preprocessors, Does.ContainKey(typeof(string)));
            Assert.AreEqual(1, preprocessors[typeof(string)].Length);
            Assert.IsInstanceOf<Preprocessor2>(preprocessors[typeof(string)][0].Target);
            Assert.AreEqual(Preprocessor2.ProcessStringMethod, preprocessors[typeof(string)][0].Method);
        });
        Assert.Multiple(() =>
        {
            Assert.That(preprocessors, Does.ContainKey(typeof(JObject)));
            Assert.AreEqual(1, preprocessors[typeof(JObject)].Length);
            Assert.IsInstanceOf<Preprocessor2>(preprocessors[typeof(JObject)][0].Target);
            Assert.AreEqual(Preprocessor2.ProcessJObjectMethod, preprocessors[typeof(JObject)][0].Method);
        });
        Assert.AreEqual(preprocessors[typeof(string)][0].Target, preprocessors[typeof(JObject)][0].Target);
    }

    [Test]
    public void GetPreprocessors_Settings()
    {
        var extensions = new ExtensionManager.Extensions(preprocessors: new Dictionary<string, Type>
        {
            ["pp"] = typeof(Preprocessor),
        });

        var settings = """{"prop":"val"}""";
        var ppDeclartaions = new Dictionary<string, JObject?>
        {
            ["pp"] = JObject.Parse(settings),
        };

        var preprocessors = PreprocessorHelper.GetPreprocessors(extensions, ppDeclartaions, null);

        Assert.AreEqual(1, preprocessors.Count);
        Assert.That(preprocessors, Does.ContainKey(typeof(string)));
        Assert.AreEqual(1, preprocessors[typeof(string)].Length);
        var target = (Preprocessor)preprocessors[typeof(string)][0].Target!;
        Assert.AreEqual(settings, target.Settings);
    }

    [Test]
    public void GetPreprocessors_Throw_NotFound()
    {
        var extensions = new ExtensionManager.Extensions();

        var ppDeclartaions = new Dictionary<string, JObject?>
        {
            ["pp"] = new JObject(),
        };

        var ex = Assert.Throws<InvalidOperationException>(() => PreprocessorHelper.GetPreprocessors(extensions, ppDeclartaions, null));
        Assert.NotNull(ex);
        Assert.AreEqual("Preprocessor with name 'pp' not registred.", ex!.Message);
    }

    [TestCase(typeof(PreprocessorInvalidCtor))]
    [TestCase(typeof(PreprocessorInvalidCtorManyArgs))]
    public void GetPreprocessors_Throw_CtorInvalid(Type procType)
    {
        var extensions = new ExtensionManager.Extensions(preprocessors: new Dictionary<string, Type>
        {
            ["pp"] = procType,
        });

        var ppDeclartaions = new Dictionary<string, JObject?>
        {
            ["pp"] = new JObject(),
        };

        var ex = Assert.Throws<InvalidOperationException>(() => PreprocessorHelper.GetPreprocessors(extensions, ppDeclartaions, null));
        Assert.NotNull(ex);
        Assert.AreEqual("Constructor with one or zero parameters not found for preprocessor 'pp'", ex!.Message);
    }

    [Test]
    public void GetPreprocessors_LogWarning_SkipProcessMethod()
    {
        var extensions = new ExtensionManager.Extensions(preprocessors: new Dictionary<string, Type>
        {
            ["pp"] = typeof(PreprocessorInvalidProcessorMethodDef),
        });

        var ppDeclartaions = new Dictionary<string, JObject?>
        {
            ["pp"] = new JObject(),
        };

        var loggerMock = new Mock<Abstraction.ILogger>();

        var processors = PreprocessorHelper.GetPreprocessors(extensions, ppDeclartaions, loggerMock.Object);
        Assert.AreEqual(0, processors.Count);
        loggerMock.Verify(l =>
            l.LogWarning(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                "Method '{0}' skiped, because his signature not like Func<T,string,T>.",
                new object[] { "System.String Process()" }));
    }

    private class Preprocessor
    {
        public Preprocessor(string settings)
        {
            Settings = settings;
        }

        public string Settings { get; }

        public string Process(string content, string docPath)
        {
            return content + docPath;
        }
    }

    private class Preprocessor2
    {
        public Preprocessor2()
        {
        }

        public static MethodInfo ProcessStringMethod => typeof(Preprocessor2).GetMethod(nameof(Process), [typeof(string), typeof(string)])!;

        public static MethodInfo ProcessJObjectMethod => typeof(Preprocessor2).GetMethod(nameof(Process), [typeof(JObject), typeof(string)])!;

        public string Process(string content, string docPath)
        {
            return content + docPath;
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Contract")]
        public JObject Process(JObject content, string docPath)
        {
            return content;
        }
    }

    private class PreprocessorInvalidCtor : Preprocessor
    {
        public PreprocessorInvalidCtor(JObject settings)
            : base(string.Empty)
        {
            Settings = settings;
        }

        public new JObject Settings { get; }
    }

    private class PreprocessorInvalidCtorManyArgs : Preprocessor
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "For test")]
        public PreprocessorInvalidCtorManyArgs(string settings, string arg2)
            : base(string.Empty)
        {
        }
    }

    private class PreprocessorInvalidProcessorMethodDef
    {
        public string Process()
            => string.Empty;
    }
}

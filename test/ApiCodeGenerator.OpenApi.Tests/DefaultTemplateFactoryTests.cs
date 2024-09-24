#if ASYNC_API
namespace ApiCodeGenerator.AsyncApi.Tests;
#else
namespace ApiCodeGenerator.OpenApi.Tests;
#endif

public class DefaultTemplateFactoryTests
{
    [Test]
    public void SplitTemplateCacheBySource()
    {
        var settings = new NJsonSchema.CodeGeneration.CSharp.CSharpGeneratorSettings();
        var src1 = new TestTemplateProvider("template1");
        var factory1 = new DefaultTemplateFactory(settings, [src1]);

        var src2 = new TestTemplateProvider("template2");
        var factory2 = new DefaultTemplateFactory(settings, [src2]);

        var actual1 = factory1.CreateTemplate("CSharp", "Class", new object());
        var actual2 = factory2.CreateTemplate("CSharp", "Class", new object());

        Assert.AreEqual("template1", actual1.Render());
        Assert.AreEqual("template2", actual2.Render());
    }

    [Test]
    public void CallBase()
    {
        var settings = new NJsonSchema.CodeGeneration.CSharp.CSharpGeneratorSettings();
        var src1 = new TestTemplateProvider("template1\n{% template Class.base %}");
        var src2 = new TestTemplateProvider("template2\n{% template Class.base %}");
        var src3 = new TestTemplateProvider("template3");

        var factory = new DefaultTemplateFactory(settings, [src1, src2, src3]);
        settings.TemplateFactory = factory;

        var actual = factory.CreateTemplate("CSharp", "Class", new object()).Render();

        Assert.AreEqual("template1\ntemplate2\ntemplate3", actual);
    }

    private class TestTemplateProvider : ITemplateProvider
    {
        private readonly string _providerKey = Guid.NewGuid().ToString();
        private readonly string _templateText;

        public TestTemplateProvider(string templateText)
        {
            _templateText = templateText;
        }

        public string? GetFullName(string name, string language)
        {
            return name == "Class" || name == "Class!"
                ? $"{_providerKey}.{name}"
                : null;
        }

        public string? GetTemplateText(string name, string language)
        {
            if (name.StartsWith(_providerKey))
            {
                return _templateText;
            }

            return null;
        }
    }
}

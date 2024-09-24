using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.Tests;

public class FileTemplateTests
{
    [Test]
    public void NullableEnabled()
    {
        const string NS = nameof(NullableEnabled);
        var settings = new FakeGeneratorSettings
        {
            CSharpGeneratorSettings =
            {
                GenerateNullableReferenceTypes = true,
                Namespace = NS,
            },
        };
        var doc = new AsyncApiDocument();
        var generator = new FakeGenerator(doc, settings, new(settings.CSharpGeneratorSettings));

        var actual = generator.GenerateFileWithoutClasses();

        var expected = TestHelpers.GetExpectedCode(null, null, NS, "#nullable enable\n\n\n");

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void DisabledWarnings()
    {
        const string NS = nameof(DisabledWarnings);
        const string WARN_CODE = "ACG1000";
        const string WARN_DESCR = "ACG1000 Test warning";
        const string DISABLE_LINE = $"#pragma warning disable {WARN_CODE} // {WARN_DESCR}";
        const string RESTORE_LINE = $"#pragma warning restore {WARN_CODE}";
        const string WARN_CODE2 = "ACG1001";
        const string WARN_DESCR2 = "ACG1001 Test warning";
        const string DISABLE_LINE2 = $"#pragma warning disable {WARN_CODE2} // {WARN_DESCR2}";
        const string RESTORE_LINE2 = $"#pragma warning restore {WARN_CODE2}";
        var settings = new FakeGeneratorSettings
        {
            CSharpGeneratorSettings =
            {
                Namespace = NS,
            },
        };
        var doc = new AsyncApiDocument();
        var generator = new FakeGenerator(doc, settings, new(settings.CSharpGeneratorSettings));
        generator.DisabledWarnings.Add(WARN_CODE, WARN_DESCR);
        generator.DisabledWarnings.Add(WARN_CODE2, WARN_DESCR2);

        var actual = generator.GenerateFileWithoutClasses();

        var expected = TestHelpers.GetExpectedCode(null, null, NS, "\n" + DISABLE_LINE + "\n" + DISABLE_LINE2);
        expected += RESTORE_LINE + "\n" + RESTORE_LINE2;

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void AddUsing()
    {
        const string USING1 = "Using1";
        const string USING2 = "Using2";
        const string NS = nameof(AddUsing);
        var settings = new FakeGeneratorSettings
        {
            CSharpGeneratorSettings =
            {
                Namespace = NS,
            },
            AdditionalNamespaceUsages = [
                USING1,
                USING2,
            ],
        };
        var doc = new AsyncApiDocument();
        var generator = new FakeGenerator(doc, settings, new(settings.CSharpGeneratorSettings));

        var actual = generator.GenerateFileWithoutClasses();

        var expected = TestHelpers.GetExpectedCode(null, null, NS, $"using {USING1};\nusing {USING2};\n\n");

        Assert.AreEqual(expected, actual);
    }
}

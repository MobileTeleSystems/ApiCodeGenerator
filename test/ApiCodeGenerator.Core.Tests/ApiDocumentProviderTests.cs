using System.Text;
using Moq.Protected;

namespace ApiCodeGenerator.Core.Tests;

public class ApiDocumentProviderTests
{
    [Test]
    public Task GetDocumentReader_FromText()
    {
        const string expectedText = """
            {"prop":"6218343c-30c3-41eb-85e3-6d726609543e"}
        """;
        var documentGenertator = new DocumentGenerator
        {
            FromDocument = new()
            {
                Json = expectedText,
            },
        };

        return RunAndAssert(documentGenertator, expectedText);
    }

    [Test]
    public Task GetDocumentReader_FromFile()
    {
        const string filePath = "SomeFilePath.json";
        const string expectedText = """
            {"prop":"eac53588-ffda-4679-b734-5df3cac3dbbe"}
        """;
        var documentGenertator = new DocumentGenerator
        {
            FromDocument = new()
            {
                Json = filePath,
            },
        };

        var fileProvider = Mock.Of<IFileProvider>(
            fp => fp.OpenRead(filePath) == new MemoryStream(Encoding.UTF8.GetBytes(expectedText)),
            MockBehavior.Strict);

        return RunAndAssert(documentGenertator, expectedText, new ApiDocumentProvider(fileProvider, new()));
    }

    [Test]
    public async Task GetDocumentReader_FromUrl()
    {
        const string url = "http://tempuri.org/SomeFilePath.json";
        const string expectedText = """
            {"prop":"8887266f-10c3-4114-8efd-6738c61d26f6"}
        """;
        var documentGenertator = new DocumentGenerator
        {
            FromDocument = new()
            {
                Url = url,
            },
        };

        var httpHandler = new Mock<DelegatingHandler>(MockBehavior.Strict);
        httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri != null && m.RequestUri.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(expectedText) })
            .Verifiable();
        var httpClient = new HttpClient(httpHandler.Object);

        await RunAndAssert(documentGenertator, expectedText, new ApiDocumentProvider(new PhysicalFileProvider(), httpClient));
        httpHandler.Verify();
    }

    [Test]
    public Task GetDocumentReader_FromSchemaText()
    {
        const string schemaName = "sName";
        const string schemaText = """{"prop":"88e4111b-c7ac-4629-8d9e-c63b96561079"}""";
        const string expectedText =
            $$$$"""
            {"openapi":"3.0.0","components":{"schemas":{"{{{{schemaName}}}}":{{{{schemaText}}}}}}}
            """;
        var documentGenertator = new DocumentGenerator
        {
            JsonSchemaToOpenApi = new()
            {
                Name = schemaName,
                Schema = schemaText,
            },
        };

        return RunAndAssert(documentGenertator, expectedText);
    }

    [Test]
    public Task GetDocumentReader_FromSchemaFile()
    {
        const string filePath = "SomeFilePath.json";
        const string schemaName = "sName";
        const string schemaText = """{"prop":"eac53588-ffda-4679-b734-5df3cac3dbbe"}""";
        const string expectedText =
            $$$$"""
            {"openapi":"3.0.0","components":{"schemas":{"{{{{schemaName}}}}":{{{{schemaText}}}}}}}
            """;
        var documentGenertator = new DocumentGenerator
        {
            JsonSchemaToOpenApi = new()
            {
                Name = schemaName,
                Schema = filePath,
            },
        };

        var fileProvider = Mock.Of<IFileProvider>(
            fp => fp.OpenRead(filePath) == new MemoryStream(Encoding.UTF8.GetBytes(schemaText)),
            MockBehavior.Strict);

        return RunAndAssert(documentGenertator, expectedText, new ApiDocumentProvider(fileProvider, new()));
    }

    [Test]
    public async Task GetDocumentReader_FromSchemaUrl()
    {
        const string url = "http://tempuri.org/schema.json";
        const string schemaName = "sName";
        const string schemaText = """{"prop":"eac53588-ffda-4679-b734-5df3cac3dbbe"}""";
        const string expectedText =
            $$$$"""
            {"openapi":"3.0.0","components":{"schemas":{"{{{{schemaName}}}}":{{{{schemaText}}}}}}}
            """;
        var documentGenertator = new DocumentGenerator
        {
            JsonSchemaToOpenApi = new()
            {
                Name = schemaName,
                Schema = url,
            },
        };

        var httpHandler = new Mock<DelegatingHandler>(MockBehavior.Strict);
        httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri != null && m.RequestUri.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(schemaText) })
            .Verifiable();
        var httpClient = new HttpClient(httpHandler.Object);

        await RunAndAssert(documentGenertator, expectedText, new ApiDocumentProvider(new PhysicalFileProvider(), httpClient));
        httpHandler.Verify();
    }

    private static async Task RunAndAssert(DocumentGenerator documentGenerator, string expectedText, IApiDocumentProvider? provider = null)
    {
        provider ??= new ApiDocumentProvider();
        var result = await provider.GetDocumentReaderAsync(documentGenerator);

        Assert.NotNull(result?.Reader);
        var actualText = await result!.Reader!.ReadToEndAsync();
        Assert.AreEqual(expectedText, actualText);
    }
}

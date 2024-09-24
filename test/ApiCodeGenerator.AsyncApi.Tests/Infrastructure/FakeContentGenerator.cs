using ApiCodeGenerator.AsyncApi;
using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure;

internal sealed class FakeContentGenerator
    : AsyncApiContentGenerator<FakeContentGenerator, FakeGenerator, FakeGeneratorSettings>
{
    public AsyncApiDocument Document => Generator.Document;

    public FakeGenerator FakeGenerator => Generator;
}

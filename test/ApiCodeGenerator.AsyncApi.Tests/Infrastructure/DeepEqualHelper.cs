using DeepEqual;

namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure;

internal static class DeepEqualHelper
{
    public static IComparison IgnoreUnmatchedProperties { get; } = new ComparisonBuilder().IgnoreUnmatchedProperties().Create();
}

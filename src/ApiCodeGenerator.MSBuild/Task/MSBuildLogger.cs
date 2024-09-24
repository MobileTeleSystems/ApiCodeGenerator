using ApiCodeGenerator.Abstraction;
using Microsoft.Build.Utilities;

namespace ApiCodeGenerator.MSBuild.Task;

internal class MSBuildLogger : ILogger
{
    private readonly TaskLoggingHelper _log;

    public MSBuildLogger(TaskLoggingHelper log)
    {
        _log = log;
    }

    public void LogError(string? sourceFile, string message, params object[] messageArgs)
        => _log.LogError(null, null, null, sourceFile, 0, 0, 0, 0, message, messageArgs);

    public void LogMessage(string message, params object[] messageArgs)
        => _log.LogMessage(message, messageArgs);

    public void LogWarning(string? sourceFile, string message, params object[] messageArgs)
        => _log.LogWarning(null, null, null, sourceFile, 0, 0, 0, 0, message, messageArgs);
}

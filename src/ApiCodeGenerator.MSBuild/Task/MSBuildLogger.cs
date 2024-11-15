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

    public void LogError(string? errorCode, string? sourceFile, string message, params object[] messageArgs)
        => _log.LogError(null, errorCode, null, sourceFile, 0, 0, 0, 0, message, messageArgs);

    public void LogMessage(string? code, string? sourceFile, string message, params object[] messageArgs)
        => _log.LogMessage(null, code, null, sourceFile, 0, 0, 0, 0, Microsoft.Build.Framework.MessageImportance.Normal, message, messageArgs);

    public void LogWarning(string? warningCode, string? sourceFile, string message, params object[] messageArgs)
        => _log.LogWarning(null, warningCode, null, sourceFile, 0, 0, 0, 0, message, messageArgs);
}

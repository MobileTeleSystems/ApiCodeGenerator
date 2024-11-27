using System;
using System.Collections.Generic;
using System.Text;

namespace ApiCodeGenerator.Abstraction
{
    public interface ILogger
    {
        void LogError(string? code, string? sourceFile, string message, params object[] messageArgs);

        void LogMessage(string? code, string? sourceFile, string message, params object[] messageArgs);

        void LogWarning(string? code, string? sourceFile, string message, params object[] messageArgs);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ApiCodeGenerator.Abstraction
{
    internal interface ILogger
    {
        void LogError(string? sourceFile, string message, params object[] messageArgs);

        void LogMessage(string message, params object[] messageArgs);

        void LogWarning(string? sourceFile, string message, params object[] messageArgs);
    }
}

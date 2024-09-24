using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiCodeGenerator.Abstraction;

namespace ApiCodeGenerator.MSBuild
{
    internal class ConsoleLogAdapter : ILogger
    {
        public void LogError(string? sourceFile, string message, params object[] messageArgs)
            => Console.Error.WriteLine($"{sourceFile}: {string.Format(message, messageArgs)}");

        public void LogMessage(string message, params object[] messageArgs)
            => Console.WriteLine($"INFO: {string.Format(message, messageArgs)}");

        public void LogWarning(string? sourceFile, string message, params object[] messageArgs)
            => Console.WriteLine($"WARNING {sourceFile}: {string.Format(message, messageArgs)}");
    }
}

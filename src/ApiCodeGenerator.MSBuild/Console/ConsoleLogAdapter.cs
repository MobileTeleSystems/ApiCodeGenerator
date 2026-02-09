using System;
using System.IO;
using ApiCodeGenerator.Abstraction;

namespace ApiCodeGenerator.MSBuild
{
    internal class ConsoleLogAdapter : ILogger
    {
        public void LogError(string? code, string? sourceFile, string message, params object[] messageArgs)
            => Log(Console.Error, "error", code, sourceFile, string.Format(message, messageArgs));

        public void LogMessage(string? code, string? sourceFile, string message, params object[] messageArgs)
            => Log(Console.Out, code is null ? null : "info", code, sourceFile, string.Format(message, messageArgs));

        public void LogWarning(string? code, string? sourceFile, string message, params object[] messageArgs)
            => Log(Console.Out, "warning", code, sourceFile, string.Format(message, messageArgs));

        private static void Log(TextWriter writer, string? type, string? code, string? file, string message)
        {
            if (file is not null)
            {
                writer.Write(Path.IsPathFullyQualified(file)
                    ? file
                    : Path.GetFullPath(file));
                writer.Write("(1,1): ");
            }

            if (type is not null)
            {
                writer.Write(type);
                if (code is not null)
                {
                    writer.Write(" ");
                    writer.Write(code);
                }

                writer.Write(": ");
            }

            writer.WriteLine(message);
        }
    }
}

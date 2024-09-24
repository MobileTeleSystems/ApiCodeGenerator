using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ApiCodeGenerator.Core
{
    internal class GetDocumentReaderResult
    {
        private GetDocumentReaderResult()
        {
        }

        public TextReader? Reader { get; private set; }

        public string? FilePath { get; private set; }

        public string? Error { get; private set; }

        public static GetDocumentReaderResult Success(TextReader reader, string? filePath)
            => new()
            {
                Reader = reader,
                FilePath = filePath,
            };

        public static GetDocumentReaderResult Failed(string error, string? filePath)
            => new()
            {
                Error = error,
                FilePath = filePath,
            };
    }
}

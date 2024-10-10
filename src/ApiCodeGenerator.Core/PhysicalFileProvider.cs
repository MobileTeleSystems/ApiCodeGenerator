using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiCodeGenerator.Core
{
    /// <summary>
    /// Реализует доступ к физической файловой системе.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class PhysicalFileProvider : IFileProvider
    {
        /// <inheritdoc />
        public bool Exists(string path)
            => File.Exists(path);

        /// <inheritdoc />
        public Stream OpenRead(string filePath)
            => File.OpenRead(filePath);

        /// <inheritdoc />
        public async Task WriteAllTextAsync(string path, string contents)
        {
            var outDir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(contents);
        }
    }
}

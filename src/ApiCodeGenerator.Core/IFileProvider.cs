using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiCodeGenerator.Core
{
    /// <summary>
    /// Абстракция от файловой системы. Для целей тестирования.
    /// </summary>
    public interface IFileProvider
    {
        /// <summary>
        /// Проверяет наличие файла.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns>Возвращает true если файл есть, иначе false.</returns>
        bool Exists(string path);

        /// <summary>
        /// Записывает содержимоей в файл.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="contents">Текст который будет записан в файл.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task WriteAllTextAsync(string path, string contents);

        /// <summary>
        /// Открывает поток для чтения указанного файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Возвращает открытй поток.</returns>
        Stream OpenRead(string filePath);
    }
}

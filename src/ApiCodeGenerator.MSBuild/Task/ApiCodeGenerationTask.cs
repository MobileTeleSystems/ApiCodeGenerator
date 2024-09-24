using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiCodeGenerator.Abstraction;
using Microsoft.Build.Framework;
using Newtonsoft.Json;

namespace ApiCodeGenerator.MSBuild
{
    /// <summary>
    /// Реализует MSBuild-задачу для генерации клиента на основании файла Nswag.
    /// </summary>
    internal class ApiCodeGenerationTask : Microsoft.Build.Utilities.Task
    {
        private readonly IGenerationTask _generationTask;

        /// <summary>
        /// Создает и инциализирует экземпляр объекта <see cref="OpenApiCodeGenerationTask"/>.
        /// </summary>
        /// <param name="generationTaskFactory">Фабрика задачи генерации.</param>
        /// <param name="extensions">Расширения процесса генерации.</param>
        public ApiCodeGenerationTask(IGenerationTaskFactory generationTaskFactory, IExtensions extensions)
        {
            _generationTask = generationTaskFactory.Create(
                extensions,
                new Task.MSBuildLogger(Log));
        }

        /// <summary>
        /// Получает и устанавливает список пар Имя=Значение, разделенных запятой.
        /// </summary>
        public string? Variables { get; set; }

        /// <summary>
        /// Получает и устанавливает путь к целевому файлу Nswag.
        /// </summary>
        [Required]
        public string NswagFile { get; set; } = string.Empty;

        /// <summary>
        /// Получает и устанавливает путь к исходному файлу Json с OpenApi.
        /// </summary>
        [Required]
        public string OpenApiFile { get; set; } = string.Empty;

        /// <summary>
        /// Получает и устанваливает путь к файлу в который будет осуществлен вывод.
        /// </summary>
        [Required]
        public string OutFile { get; set; } = string.Empty;

        /// <inheritdoc />
        public override bool Execute()
            => _generationTask.ExecuteAsync(NswagFile, OpenApiFile, OutFile, Variables).GetAwaiter().GetResult();
    }
}

# Как подключить

1. Установите в проект пакет `ApiCodeGenerator.MSBuild`
1. Добавьте в проект файл с описанием Api в формате: Swagger 2.0, OpenApi 3.0, JsonSchema, AsyncApi 2.0
1. Добавьте в проект одноименный файл с расширением nswag и заполните его
1. Пересоберите проект

# Формат файла nswag
> Для использования подсказок при редактировании файла можно подключить в  ваш редактор файл `nswag-shema.json` из этого репозитория.

Формат nswag - это файлы в которых хранит настройки программа [NSwagStudio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio), ее можно использовать для настройки стандартных генераторов Nswag.
Файл имеет следующий вид:
```json
{
  "documentGenerator": {
    "fromDocument": {
      "json": "$(InputJson)",
      "url": "http://basketService/swagger/v1/swagger.json",
    }
  },
  "codeGenerators": {
    "openApiToCSharpClient": {}
  }
}
```
Т.к. при подключении множества разных API их настройки по большей части совпадают, то для уменьшения дублирования можно вынести настройки препроцессоров и генераторов в файл base.nswag (имя файла можно изменить настройкой). Файл имеет ту же структуру, при этом в блоке `codeGenerators` может быть указано несколько генераторов (к целевому файлу будут применяться настройки выбранного в нем генератора). Сам файл ищется во всех вышестоящих каталогах начиная от файла nswag по которому идет генерация.

## Генерация документа
Блок `documentGenerator` указывает правила получения документа на основании которого будет генерироваться код. Для генерации по документам формата `Swagger 2.0` `OpenApi 3.0` или `AsyncApi 3.0`, в нем нужно заполнить свойство `fromDocument`. При генерации по документу JsonSchema нужно использовать свойство `jsonSchemaToOpenApi`.

В `fromDocument` в свойстве `json` указывается путь к файлу либо строка содержащая сам документ (в примере указана переменная содержащая путь к файлу). В свойстве `url` указывается адрес откуда можно скачать файл, при этом если свойство `json` не заполнено, то документ будет загружаться с этого адреса. В оба свойства можно передавать пути к фалам yaml.

> Если в `fromDocument` пути не указаны или даже отсутствует сам блок то в свойство `json` будет потставлен путь к файлу из `OpenApiReference`

Так же в `fromDocument` может быть задано свойство `preprocessors` через которое подключаются обработчики осуществляющие предобработку документа перед передачей его генератору кода.

## Генерация кода
Блок `codeGenerators` указывает генератор который будет использоваться для формирования кода. В качестве свойства указывается имя генератора, а в качестве значения настройки выбранного генератора.

Есть следующие генераторы:
* `asyncApiToCSharpAmqpService` - клиент на С# для библиотеки [RabbitMQ.Client 5](https://www.nuget.org/packages/RabbitMQ.Client/5.2.0)
* `openApiToCSharpClient` - генерирует клиента С# для доступа к API
<!-- * `openApiToTypeScriptClient` - генерирует клиента TypeScript для доступа к API -->
* `openApiToCSharpController` - генерирует контроллер и интерфейс службы для реализации логики котроллера.
* `openApiToRefitClient` - генерирует интерфейс для доступа к API с использованием библиотеки [Refit](https://github.com/reactiveui/refit). Требуется установка пакета [ApiCodeGenerator.OpenApi.Refit](https://github.com/MobileTeleSystems/ApiCodeGenerator.OpenApi.Refit)

<!-- > ⚠ Если вы используете генератор `openApiToTypeScriptClient`, то в проекте для соответствующих OpenApiReference нужно заменить генератор.
> ```xml
>  <OpenApiReference Update="@(OpenApiReference)">
>	  <CodeGenerator>OacgTypeScript</CodeGenerator>
>	</OpenApiReference>
>  ``` -->

Почитать про опции генераторов:
 - [для всех перечисленных генераторов](https://github.com/RicoSuter/NSwag/wiki/NSwag-Configuration-Document).
 - [для генераторов кода на C#](https://github.com/RicoSuter/NSwag/wiki/CSharpGeneratorBaseSettings)
 - [для openApiToCSharpClient](https://github.com/RicoSuter/NSwag/wiki/CSharpClientGeneratorSettings)
 <!-- - [для openApiToTypescriptClient](https://github.com/RicoSuter/NSwag/wiki/TypeScriptClientGeneratorSettings) -->

Так же для всех генераторов на уровне библиотеки добавлена настройка `replaceNameCollection` для замены в именах свойств символов.
 ```json
 "replaceNameCollection": {
    "@":"_" // Замена в именах @ на _
  }
 ```
### Генерация имен операций
В настройках генератора есть свойство `OperationGenerationMode`, которое переключает режимы генерации как клиентов/контроллеров, так и самих операций.
Доступны следующие варианты:
- `MultipleClientsFromOperationId` - генерирует несколько клиентов/контроллеров разделяя их по первой части идентификатора операции, используя как разделитель подчеркивание, имя операции формируется из второй части
- `MultipleClientsFromPathSegments` - генерирует несколько клиентов/контроллеров используя для имени операции последний сегмент пути, а для имени клиента/контроллера предпоследний сегмент
- `MultipleClientsFromFirstTagAndPathSegments` - генерирует несколько клиентов/контроллеров используя для имени операции последний сегмент пути, а для имени клиента/контроллера первый тэг
- `MultipleClientsFromFirstTagAndOperationId` - генерирует несколько клиентов/контроллеров используя для имени операции ее идентификатор, а для имени клиента/контроллера первый тэг
- `SingleClientFromOperationId` - генерирует клиента/контроллер используя для имени операции ее идентификатор
- `SingleClientFromPathSegments` - генерирует клиента/контроллер используя для имени операции последний сегмент пути
- `SingleClientFromLastSegmentOfOperationId` - генерирует клиента/контроллер используя для имени операции последнюю часть идентификатора отделенную точкой (например: ```"operationId": "some.id"``` -  имя будет `id`).

# Кастомизация сборки
Свойства MSBuild для управления процессом сборки:
- `AcgGenerateOpenapiReference` - включает автоматическую генерацию элементов OpenApiReference по обнаруженным файлам *.nswag. По умолчанию `true`.
- `AcgCleanupEnabled` - удаление сгенерированного кода при выполнении цели Cleanup. По умолчанию `true`.
- `AcgNswagBaseFileName`- имя файла с базовыми настройками. По умолчанию `base.nswag`
- `AcgApiDocumentDir` - каталог в котором будет вестись поиск документов. По умолчанию не задан, а поиск ведется в каталоге файла nswag.
- `AcgApiDocumentLinkNearNswag` - включает генерацию ссылок на документ, для отображения его рядом с файлом nswag (работает при задании `AcgOpenApiDocumentDir`)
- `OpenApiCodeDirectory` - каталог куда попадают сгенерированные файлы. По умолчанию obj
- `AcgNswagBase` - путь к файлу с базовой конфигурацией. При его задании отключается механизм поиска файла и используется указанный.

В процессе работы сценария сборки библиотека генерирует в проекте список OpenApiReference, который в дальнейшем обрабатывается пакетом [Microsoft.Extensions.ApiDescription.Client](https://www.nuget.org/packages/Microsoft.Extensions.ApiDescription.Client/).

В случае если вы отключаете автоматическую генерацию, то вам нужно формировать такие записи самостоятельно. Вот пример записи:

```xml
<ItemGroup>
  <OpenApiReference Include="..\OpenAPI\ExternalService.json">
    <Options>OpenAPI\ExternalService.nswag</Options>
    <CodeGenerator>Acg</CodeGenerator>
  </OpenApiReference>
</ItemGroup>
```
Здесь в `Include` указывается путь к документу OpenApi, `Options` указывает путь к файлу nswag, `CodeGenerator` имя используемого таргета (должен быть всегда Acg<!--, кроме случая генерации для TypeScript-->)

Другие параметры:
 - `OutputPath` - путь к сгенерированному файлу
 - `ClassName` - имя сгенерированного класса. Попадет в nswag как переменная с тем же именем.
 - `Namespace` - пространство имен. Попадет в nswag как переменная с тем же именем.
 - `Variables` - дополнительные переменные передаваемые в nswag, в виде пар Имя=Значение разделенных `;`

Если вам нужно изменить настройки записей OpenApiReference добавленных автоматом, то требуется включить в проект запись вида:
```xml
<OpenApiReference Update="path-to-openapi-doc">
  <OutputPath>%(FileName).g.cs</OutputPath>
</OpenApiReference>
```
Здесь мы поменяли имя генерируемого файла.
> ⚠ Если вы указывали `AcgApiDocumentDir` как полный путь, то в инструкции Update путь также должен быть полным, т.к. проверка осуществляется по совпадению строк, а не того, что это один и тот же файл.

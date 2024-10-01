[Read in Russian](README_RU.MD)

[![NuGet Version](https://img.shields.io/nuget/vpre/ApiCodeGenerator.MSBuild)](https://www.nuget.org/packages/ApiCodeGenerator.MSBuild)
[![GitHub License](https://img.shields.io/github/license/MobileTeleSystems/ApiCodeGenerator)](https://github.com/MobileTeleSystems/ApiCodeGenerator/blob/dev/LICENSE)
![Code Coverage](https://img.shields.io/badge/84.90%25-z?label=coverage) <!-- 
Before setting up the workflow, the coverage percentage is taken from the IDE  -->

# How to install

1. Add the `ApiCodeGenerator.MSBuild` package in your project.
2. Add a file describing the API in formats like: Swagger 2.0, OpenApi 3.0, JsonSchema, AsyncApi 2.0 to your project.
3. Add a file with the same name but with the `.nswag` extension to your project and fill it out.
4. Rebuild the project.

# NSwag File Format
<!-- > To use hints while editing the file, you can connect the `nswag-shema.json` file from this repository to your editor. -->

The NSwag format consists of files that store settings for the [NSwagStudio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio) application, which can be used to configure standard NSwag generators.
The file has the following structure:
```json
{
  "documentGenerator": {
    "fromDocument": {
      "json": "$(InputJson)",
      "url": "http://basketService/swagger/v1/swagger.json"
    }
  },
  "codeGenerators": {
    "openApiToCSharpClient": {}
  }
}
```
Since the settings for various APIs mostly overlap, to reduce duplication, you can move the settings for preprocessors and generators to a `base.nswag` file (the file name can be changed via settings). The file has the same structure, and the `codeGenerators` block can specify several generators (the settings for the selected generator will be applied to the target file). The file is searched in all higher-level directories starting from the NSwag file being generated.

## Document Generation
The `documentGenerator` block specifies the rules for obtaining the document based on which the code will be generated. For generation from documents in `Swagger 2.0`, `OpenApi 3.0`, or `AsyncApi 3.0` formats, you need to fill in the `fromDocument` property. For generation from a JsonSchema document, use the `jsonSchemaToOpenApi` property.

In `fromDocument`, the `json` property specifies the path to the file or a string containing the document itself (the example uses a variable containing the file path). The `url` property specifies the address from which the file can be downloaded. If the `json` property is not filled, the document will be downloaded from this address. Both properties can accept paths to YAML files.

> If paths are not specified in `fromDocument` or the block itself is absent, the `json` property will be set to the path from `OpenApiReference`.

Additionally, `fromDocument` can have a `preprocessors` property to connect handlers that preprocess the document before passing it to the code generator.

## Code Generation
The `codeGenerators` block specifies the generator that will be used to generate the code. The property name is the generator's name, and the value is the settings for the selected generator.

Available generators:
* `asyncApiToCSharpAmqpService` - C# client for the [RabbitMQ.Client 5](https://www.nuget.org/packages/RabbitMQ.Client/5.2.0) library.
* `openApiToCSharpClient` - generates a C# client for accessing the API.
<!-- * `openApiToTypeScriptClient` - generates a TypeScript client for accessing the API. -->
* `openApiToCSharpController` - generates a controller and service interface for implementing the controller's logic.
* `openApiToRefitClient` - generates an interface for accessing the API using the [Refit](https://github.com/reactiveui/refit) library. Requires the `ApiCodeGenerator.OpenApi.Refit` package.

<!-- > ⚠ If you use the `openApiToTypeScriptClient` generator, you need to replace the generator in the project for the corresponding OpenApiReference.
> ```xml
>  <OpenApiReference Update="@(OpenApiReference)">
>         <CodeGenerator>OacgTypeScript</CodeGenerator>
>       </OpenApiReference>
>  ``` -->

Read about generator options:
 - [for all listed generators](https://github.com/RicoSuter/NSwag/wiki/NSwag-Configuration-Document).
 - [for C# code generators](https://github.com/RicoSuter/NSwag/wiki/CSharpGeneratorBaseSettings)
 - [for `openApiToCSharpClient`](https://github.com/RicoSuter/NSwag/wiki/CSharpClientGeneratorSettings)
 - [for `openApiToTypescriptClient`](https://github.com/RicoSuter/NSwag/wiki/TypeScriptClientGeneratorSettings)

Additionally, for all generators, the library adds the `replaceNameCollection` setting for replacing characters in property names.
```json
"replaceNameCollection": {
  "@": "_" // Replace @ with _ in names
}

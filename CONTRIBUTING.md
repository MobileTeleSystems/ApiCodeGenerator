# Contributing guide

Wath you help this project?

You can help by providing feedback: report any bugs in the generator itself or in the generated code. Your suggestions for new features are welcome. If you would like to implement new functionality, please discuss it with the team beforehand.


## Solution and folder structure
|Folder|Description|
|-|-|
|.\src\ApiCodeGenerator.Abstraction|Source Code for the Interface Between the Loader and Generation Modules|
|.\src\ApiCodeGenerator.AsyncApi|Source Code for the Code Generation Module Based on AsyncApi Documents|
|.\src\ApiCodeGenerator.Core|Source Code of core|
|.\src\ApiCodeGenerator.Core|"Source Code for MSBuild Integration"|
|.\src\ApiCodeGenerator.OpenApi|Source Code for the Code Generation Module Based on OpenApi Documents|
|.\test|Unit test projects folder|

## Build
You can use solution to build and run tests.

## Tests
NUnit3 is used as unit-testing framework.

## Debuging
To debug, run the ApiCodeGenerator.MSBuild project. This project initiates the build process for the test project `test/TestPrj.Openapi`, which includes the debug versions of the assemblies from the solution (ensure that all projects are built after any changes before running).

You can also use unit tests for debugging.

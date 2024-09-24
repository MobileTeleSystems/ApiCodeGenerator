using System.CommandLine;
using ApiCodeGenerator.MSBuild;

var cmd = new GenerateCommand();
await cmd.InvokeAsync(args);

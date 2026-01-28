using System.CommandLine;
using ApiCodeGenerator.MSBuild;

var cmd = new GenerateCommand();
var exitCode = await cmd.InvokeAsync(args);

return exitCode;

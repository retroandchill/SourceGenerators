
using System.CommandLine;
using Retro.AutoCommandLine.Sample.Commands;

var rootCommand = ProgramRootCommandFactory.Create();

return await rootCommand.InvokeAsync(args);
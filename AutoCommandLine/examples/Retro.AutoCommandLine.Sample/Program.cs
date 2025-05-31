using System.CommandLine;
using Retro.AutoCommandLine.Sample.Commands;


var factory = new CommandHandlerFactory();
var rootCommand = ProgramRootCommandFactory.Create(factory);

return await rootCommand.InvokeAsync(args);
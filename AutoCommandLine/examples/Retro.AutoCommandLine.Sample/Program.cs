using System;
using System.CommandLine;
using Retro.AutoCommandLine.Sample.Commands;

var context = new CommandContext();
var rootCommand = ProgramRootCommandFactory.Create(context);

return await rootCommand.InvokeAsync(args);
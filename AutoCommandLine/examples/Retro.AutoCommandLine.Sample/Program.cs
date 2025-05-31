using System;
using System.CommandLine;
using Retro.AutoCommandLine.Sample.Commands;


var rootCommand = ProgramRootCommand.Create();

await rootCommand.InvokeAsync(args);
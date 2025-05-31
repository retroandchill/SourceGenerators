using System;
using System.CommandLine;


var rootCommand = new RootCommand("Simple command line app");

rootCommand.SetHandler(() => {
  Console.WriteLine("Hello World!");
});

await rootCommand.InvokeAsync(args);
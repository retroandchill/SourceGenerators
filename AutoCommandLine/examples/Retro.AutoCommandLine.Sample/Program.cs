using System;
using System.CommandLine;
using Retro.AutoCommandLine.Sample.Commands;


var rootCommand = new RootCommand("Sample command line app");

var positionalParameter = new Argument<string>("Positional") {
    Description = "Positional parameter"
};
var requiredOption = new Option<int>("--required") {
    Description = "Required option",
    IsRequired = true
};
var optionalOption = new Option<bool>("--optional") {
    Description = "Optional option",
    IsRequired = false
};

rootCommand.AddArgument(positionalParameter);
rootCommand.AddOption(requiredOption);
rootCommand.AddOption(optionalOption);

rootCommand.SetHandler((boundArgs) => {
  Console.WriteLine("Hello World!");
}, new RootCommandOptionsBinder(positionalParameter, requiredOption, optionalOption));

return await rootCommand.InvokeAsync(args);
using System;
namespace Retro.AutoCommandLine.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CommandLineContextAttribute(Type rootCommand) : Attribute {

  public Type RootCommand { get; } = rootCommand;

}
using System;
#if AUTO_COMMAND_LINE_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.AutoCommandLine.Attributes;

[AttributeUsage(AttributeTargets.Class)]
#if AUTO_COMMAND_LINE_GENERATOR
[IncludeFile]
#endif
internal class CommandLineContextAttribute(Type rootCommand) : Attribute {

  public Type RootCommand { get; } = rootCommand;

}
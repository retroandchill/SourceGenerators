using System;
#if AUTO_COMMAND_LINE_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.AutoCommandLine.Attributes;

[AttributeUsage(AttributeTargets.Property)]
#if AUTO_COMMAND_LINE_GENERATOR
[IncludeFile]
#endif
internal class ArgumentAttribute(string? name = null) : CliParameterAttribute {
  
  public string? Name { get; } = name;
  
}
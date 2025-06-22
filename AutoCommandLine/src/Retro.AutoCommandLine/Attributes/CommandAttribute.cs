using System;
#if AUTO_COMMAND_LINE_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.AutoCommandLine.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
#if AUTO_COMMAND_LINE_GENERATOR
[IncludeFile]
#endif
internal class CommandAttribute(string? name = null) : Attribute {
  public string? Name { get; } = name;
  
  public string? Description { get; init; }

  public bool IsRootCommand { get; init; } = false;
}
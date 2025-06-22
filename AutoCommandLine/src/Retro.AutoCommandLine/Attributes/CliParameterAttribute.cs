using System;
using System.Collections.Immutable;
#if AUTO_COMMAND_LINE_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.AutoCommandLine.Attributes;

#if AUTO_COMMAND_LINE_GENERATOR
[IncludeFile]
#endif
internal abstract class CliParameterAttribute : Attribute {
  
  public string? Description { get; init; }
  
}
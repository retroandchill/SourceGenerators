using System;
using System.Collections.Immutable;
#if AUTO_COMMAND_LINE_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.AutoCommandLine.Attributes;

[AttributeUsage(AttributeTargets.Property)]
#if AUTO_COMMAND_LINE_GENERATOR
[IncludeFile]
#endif
internal class OptionAttribute(params string[] aliases) : CliParameterAttribute {
  
  public ImmutableArray<string> Aliases { get; } = [..aliases];
  
}
using System;
using System.Collections.Immutable;
namespace Retro.AutoCommandLine.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class OptionAttribute(params string[] aliases) : Attribute {
  
  public ImmutableArray<string> Aliases { get; } = aliases.ToImmutableArray();
  
  public string? Description { get; init; }
  
}
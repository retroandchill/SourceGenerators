using System;
using System.Collections.Immutable;
namespace Retro.AutoCommandLine.Core.Attributes;

public abstract class CliParameterAttribute : Attribute {
  
  public string? Description { get; init; }
  
}
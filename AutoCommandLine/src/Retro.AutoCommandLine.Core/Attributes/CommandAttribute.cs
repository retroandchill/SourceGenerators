using System;
namespace Retro.AutoCommandLine.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class CommandAttribute(string? name = null) : Attribute {
  public string? Name { get; } = name;
  
  public string? Description { get; init; }
  
  public bool HasHandler { get; init; } = false;
  
  public bool IsRootCommand { get; init; } = false;
}
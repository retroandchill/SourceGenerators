using System;
namespace Retro.AutoCommandLine.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ArgumentAttribute(string? name = null) : Attribute {
  
  public string? Name { get; } = name;
  
  public string? Description { get; init; }
  
}
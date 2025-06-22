using System;
namespace Retro.AutoCommandLine.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ArgumentAttribute(string? name = null) : CliParameterAttribute {
  
  public string? Name { get; } = name;
  
}
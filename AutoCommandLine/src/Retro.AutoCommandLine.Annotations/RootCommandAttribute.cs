using System;
namespace Retro.AutoCommandLine.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public class RootCommandAttribute(string? description = null) : Attribute {

  public string? Description { get; } = description;

}
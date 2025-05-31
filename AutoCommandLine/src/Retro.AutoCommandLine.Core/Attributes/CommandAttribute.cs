using System;
namespace Retro.AutoCommandLine.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class CommandAttribute : Attribute {
  public bool HasHandler { get; init; } = false;
}
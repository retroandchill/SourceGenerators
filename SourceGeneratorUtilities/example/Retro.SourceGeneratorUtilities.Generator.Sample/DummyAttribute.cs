using System;

namespace Retro.SourceGeneratorUtilities.Generator.Sample;

[AttributeUsage(AttributeTargets.Class)]
public class DummyAttribute(int value1, string? value2 = null) : Attribute {

  public DummyAttribute() : this(1, null) {
  }
  
  public DummyAttribute(string value) : this(1, value) {
    
  }
  
  public int Value1 { get; } = value1;
  public string? Value2 { get; } = value2;
}
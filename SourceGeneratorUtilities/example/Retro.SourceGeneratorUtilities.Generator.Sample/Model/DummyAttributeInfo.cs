using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Attributes;

namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType<DummyAttribute>]
public record DummyAttributeInfo(int Value1, string? Value2 = null) {
  
  public DummyAttributeInfo() : this(1) {
  }
  
  public DummyAttributeInfo(string value) : this(1, value) {
  }


  public double Value3 { get; init; } = 1.0;
  public ITypeSymbol? Value4 { get; init; }
}
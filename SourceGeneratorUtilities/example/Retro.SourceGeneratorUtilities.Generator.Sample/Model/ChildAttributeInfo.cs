using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Attributes;

namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType<ChildAttribute>]
public record ChildAttributeInfo() : DummyAttributeInfo(1) {
  
  public ITypeSymbol? GenericTypeValue { get; init; }
  
}
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Attributes;
using Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;
namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType<DependencyAttribute>]
public record DependencyOverview(ITypeSymbol Type, ServiceScope Scope) {
  public string? Key { get; init; }
}
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Attributes;
using Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;
namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType<SingletonAttribute>]
public record SingletonOverview(ITypeSymbol Type) : DependencyOverview(Type, ServiceScope.Singleton);

[AttributeInfoType(typeof(SingletonAttribute<>))]
public record SingletonOneParamOverview(ITypeSymbol Type) : SingletonOverview(Type) {
  
}
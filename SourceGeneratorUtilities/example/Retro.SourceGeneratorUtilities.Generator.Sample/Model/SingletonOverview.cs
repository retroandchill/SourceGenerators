using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType<SingletonAttribute>]
public record SingletonOverview(ITypeSymbol Type) : DependencyOverview(Type, ServiceScope.Singleton);

[AttributeInfoType(typeof(SingletonAttribute<>))]
public record SingletonOneParamOverview(ITypeSymbol Type) : SingletonOverview(Type);
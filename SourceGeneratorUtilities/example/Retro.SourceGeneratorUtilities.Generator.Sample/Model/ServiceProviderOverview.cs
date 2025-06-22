using Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType<ServiceProviderAttribute>]
public record struct ServiceProviderOverview {
  
  public bool AllowDynamicRegistrations { get; init; }
  
}
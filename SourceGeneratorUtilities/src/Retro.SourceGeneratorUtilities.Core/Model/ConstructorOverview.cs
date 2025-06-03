using System.Collections.Immutable;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record struct ConstructorOverview(ImmutableArray<ParameterOverview> Parameters) {

  public bool HasInitializer => Initializer.HasValue;
  
  public ConstructorInitializerOverview? Initializer { get; init; }
  
}
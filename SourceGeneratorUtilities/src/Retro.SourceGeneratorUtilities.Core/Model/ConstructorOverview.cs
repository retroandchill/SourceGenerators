using System.Collections.Immutable;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record ConstructorOverview(ImmutableArray<ParameterOverview> Parameters) {
  
  public bool IsPrimaryConstructor { get; init; }

  public bool HasInitializer => Initializer is not null;
  
  public ConstructorInitializerOverview? Initializer { get; init; }

  public ImmutableArray<AssignmentOverview> Assignments { get; init; } = [];

}
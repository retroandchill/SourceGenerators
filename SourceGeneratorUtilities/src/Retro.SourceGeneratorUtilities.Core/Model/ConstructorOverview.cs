using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record ConstructorOverview(IMethodSymbol Symbol, IReadOnlyList<ParameterOverview> Parameters) {
  
  public bool IsPrimaryConstructor { get; init; }

  public bool HasInitializer => Initializer is not null;
  
  public ConstructorInitializerOverview? Initializer { get; init; }

  public IReadOnlyList<AssignmentOverview> Assignments { get; init; } = [];

}
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record DataTypeOverview {
  
  public required INamedTypeSymbol Symbol { get; init; }
  
  public INamespaceSymbol Namespace => Symbol.ContainingNamespace;
  
  public string Name => Symbol.Name;
  
  public required DataTypeOverview? Base { get; init; }
  
  public string? BaseName => Base?.Symbol.ToDisplayString();
  
  public required IReadOnlyList<ConstructorOverview> Constructors { get; init; }
  
  public required IReadOnlyList<PropertyOverview> Properties { get; init; }
  
}
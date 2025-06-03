using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Types;

public class NamedTypeSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol> {
  private IEqualityComparer<ISymbol> _equalityComparerImplementation;

  public static NamedTypeSymbolEqualityComparer Default { get; } = new(SymbolEqualityComparer.Default);

  public static NamedTypeSymbolEqualityComparer IncludeNullability { get; } =
    new(SymbolEqualityComparer.IncludeNullability);

  private NamedTypeSymbolEqualityComparer(IEqualityComparer<ISymbol> equalityComparerImplementation) {
    _equalityComparerImplementation = equalityComparerImplementation;
  }

  public bool Equals(INamedTypeSymbol x, INamedTypeSymbol y) {
    return _equalityComparerImplementation.Equals(x, y);
  }

  public int GetHashCode(INamedTypeSymbol obj) {
    return _equalityComparerImplementation.GetHashCode(obj);
  }
}
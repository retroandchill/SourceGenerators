using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Types;
namespace Retro.SourceGeneratorUtilities.Core.Members;

public class MethodSymbolEqualityComparer : IEqualityComparer<IMethodSymbol> {
  private readonly IEqualityComparer<ISymbol> _equalityComparerImplementation;

  public static MethodSymbolEqualityComparer Default { get; } = new(SymbolEqualityComparer.Default);

  public static MethodSymbolEqualityComparer IncludeNullability { get; } =
    new(SymbolEqualityComparer.IncludeNullability);

  private MethodSymbolEqualityComparer(IEqualityComparer<ISymbol> equalityComparerImplementation) {
    _equalityComparerImplementation = equalityComparerImplementation;
  }

  public bool Equals(IMethodSymbol x, IMethodSymbol y) {
    return _equalityComparerImplementation.Equals(x, y);
  }

  public int GetHashCode(IMethodSymbol obj) {
    return _equalityComparerImplementation.GetHashCode(obj);
  }
  
}
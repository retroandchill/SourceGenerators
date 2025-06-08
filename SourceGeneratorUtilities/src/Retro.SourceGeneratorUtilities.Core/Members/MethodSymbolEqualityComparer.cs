using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Types;
namespace Retro.SourceGeneratorUtilities.Core.Members;

/// <summary>
/// A custom equality comparer for comparing instances of <see cref="IMethodSymbol"/>.
/// This comparer leverages an instance of <see cref="ISymbol"/> equality comparer
/// to determine equality and hash codes for method symbols.
/// </summary>
public class MethodSymbolEqualityComparer : IEqualityComparer<IMethodSymbol> {
  private readonly IEqualityComparer<ISymbol> _equalityComparerImplementation;

  /// <summary>
  /// Gets the default instance of <see cref="MethodSymbolEqualityComparer"/>.
  /// This instance uses the default <see cref="ISymbol"/> equality comparer
  /// to determine equality and calculate hash codes for <see cref="IMethodSymbol"/> objects.
  /// </summary>
  public static MethodSymbolEqualityComparer Default { get; } = new(SymbolEqualityComparer.Default);

  /// <summary>
  /// Gets an instance of <see cref="MethodSymbolEqualityComparer"/> that includes nullability
  /// information when determining equality and calculating hash codes for <see cref="IMethodSymbol"/> objects.
  /// This comparer uses <see cref="SymbolEqualityComparer.IncludeNullability"/> for comparison.
  /// </summary>
  public static MethodSymbolEqualityComparer IncludeNullability { get; } =
    new(SymbolEqualityComparer.IncludeNullability);

  private MethodSymbolEqualityComparer(IEqualityComparer<ISymbol> equalityComparerImplementation) {
    _equalityComparerImplementation = equalityComparerImplementation;
  }

  /// <inheritdoc />
  public bool Equals(IMethodSymbol x, IMethodSymbol y) {
    return _equalityComparerImplementation.Equals(x, y);
  }

  /// <inheritdoc />
  public int GetHashCode(IMethodSymbol obj) {
    return _equalityComparerImplementation.GetHashCode(obj);
  }
  
}
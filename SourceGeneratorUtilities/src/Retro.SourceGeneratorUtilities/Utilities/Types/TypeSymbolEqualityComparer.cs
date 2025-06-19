using Microsoft.CodeAnalysis;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Types;

/// <summary>
/// Provides a mechanism to compare instances of <see cref="ITypeSymbol"/> for equality.
/// </summary>
/// <remarks>
/// This comparer allows for the comparison of <see cref="ITypeSymbol"/> objects based on their
/// underlying metadata representation, optionally including nullability considerations.
/// </remarks>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal class TypeSymbolEqualityComparer : IEqualityComparer<ITypeSymbol> {
  private readonly IEqualityComparer<ISymbol> _equalityComparerImplementation;

  /// <summary>
  /// Gets the default instance of <see cref="TypeSymbolEqualityComparer"/>.
  /// </summary>
  /// <remarks>
  /// The default comparer is based on <see cref="SymbolEqualityComparer.Default"/>,
  /// which performs equality checks for <see cref="ITypeSymbol"/> instances without considering nullability.
  /// </remarks>
  public static TypeSymbolEqualityComparer Default { get; } = new(SymbolEqualityComparer.Default);

  /// <summary>
  /// Gets an instance of <see cref="TypeSymbolEqualityComparer"/> that performs equality checks
  /// for <see cref="ITypeSymbol"/> instances while considering nullability.
  /// </summary>
  /// <remarks>
  /// This comparer is based on <see cref="SymbolEqualityComparer.IncludeNullability"/>,
  /// which accounts for both the type's structure and its nullability annotations when comparing symbols.
  /// </remarks>
  public static TypeSymbolEqualityComparer IncludeNullability { get; } = new(SymbolEqualityComparer.IncludeNullability);
  
  private TypeSymbolEqualityComparer(IEqualityComparer<ISymbol> equalityComparerImplementation) {
    _equalityComparerImplementation = equalityComparerImplementation;
  }

  /// <inheritdoc />
  public bool Equals(ITypeSymbol x, ITypeSymbol y) {
    return _equalityComparerImplementation.Equals(x, y);
  }

  /// <inheritdoc />
  public int GetHashCode(ITypeSymbol obj) {
    return _equalityComparerImplementation.GetHashCode(obj);
  }
}
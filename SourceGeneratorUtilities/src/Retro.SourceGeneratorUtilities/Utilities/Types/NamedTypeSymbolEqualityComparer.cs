using Microsoft.CodeAnalysis;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Types;

/// <summary>
/// Provides an implementation of <see cref="IEqualityComparer{T}"/> for comparing <see cref="INamedTypeSymbol"/> instances.
/// </summary>
/// <remarks>
/// This class is specifically designed to compare named type symbols with optional support for nullability considerations.
/// It uses a configurable internal <see cref="ISymbol"/> equality implementation to perform the comparisons.
/// </remarks>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal class NamedTypeSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol> {
  private readonly IEqualityComparer<ISymbol> _equalityComparerImplementation;

  /// <summary>
  /// Gets the default instance of <see cref="NamedTypeSymbolEqualityComparer"/>.
  /// </summary>
  /// <remarks>
  /// The default instance uses <see cref="SymbolEqualityComparer.Default"/> for equality comparison,
  /// providing standard symbol comparison without considering nullability differences.
  /// This default instance can be used where nullability considerations are not required or relevant.
  /// </remarks>
  public static NamedTypeSymbolEqualityComparer Default { get; } = new(SymbolEqualityComparer.Default);

  /// <summary>
  /// Gets an instance of <see cref="NamedTypeSymbolEqualityComparer"/> that considers nullability when comparing symbols.
  /// </summary>
  /// <remarks>
  /// This instance uses <see cref="SymbolEqualityComparer.IncludeNullability"/> for equality comparison,
  /// ensuring that differences in nullability annotations are accounted for during symbol comparisons.
  /// It is ideal for scenarios where nullability is a critical aspect of the comparison logic.
  /// </remarks>
  public static NamedTypeSymbolEqualityComparer IncludeNullability { get; } = new(SymbolEqualityComparer.IncludeNullability);

  private NamedTypeSymbolEqualityComparer(IEqualityComparer<ISymbol> equalityComparerImplementation) {
    _equalityComparerImplementation = equalityComparerImplementation;
  }

  /// <inheritdoc />
  public bool Equals(INamedTypeSymbol x, INamedTypeSymbol y) {
    return _equalityComparerImplementation.Equals(x, y);
  }

  /// <inheritdoc />
  public int GetHashCode(INamedTypeSymbol obj) {
    return _equalityComparerImplementation.GetHashCode(obj);
  }
}
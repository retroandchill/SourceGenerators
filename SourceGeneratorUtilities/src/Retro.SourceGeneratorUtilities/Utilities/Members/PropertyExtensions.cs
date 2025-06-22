using Microsoft.CodeAnalysis;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Members;

/// <summary>
/// Provides extension methods for retrieving and analyzing the properties of a given type symbol within Roslyn's analysis framework.
/// </summary>
/// <remarks>
/// This class is designed to simplify the process of obtaining metadata about properties declared in a type,
/// including their name, type, accessibility, setter presence, and initializer expression, if any.
/// </remarks>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal static class PropertyExtensions {
  /// <summary>
  /// Retrieves all public properties of the specified type symbol.
  /// </summary>
  /// <param name="typeSymbol">The type symbol from which public properties are to be retrieved.</param>
  /// <returns>An IEnumerable containing public properties of the provided type symbol.</returns>
  public static IEnumerable<IPropertySymbol> GetPublicProperties(this ITypeSymbol typeSymbol) {
    return typeSymbol.GetMembers()
        .OfType<IPropertySymbol>()
        .Where(x => x.DeclaredAccessibility == Accessibility.Public);
  }
}
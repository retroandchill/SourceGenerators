using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

/// <summary>
/// Provides extension methods for retrieving and analyzing the properties of a given type symbol within Roslyn's analysis framework.
/// </summary>
/// <remarks>
/// This class is designed to simplify the process of obtaining metadata about properties declared in a type,
/// including their name, type, accessibility, setter presence, and initializer expression, if any.
/// </remarks>
public static class PropertyExtensions {
  /// <summary>
  /// Retrieves an enumerable collection of <see cref="PropertyOverview"/> objects representing the properties
  /// of the specified type symbol.
  /// </summary>
  /// <param name="typeSymbol">The type symbol whose properties should be retrieved.</param>
  /// <returns>
  /// A collection of <see cref="PropertyOverview"/> instances, where each instance provides metadata about a
  /// property of the specified type symbol.
  /// </returns>
  public static IEnumerable<PropertyOverview> GetProperties(this ITypeSymbol typeSymbol) {
    return typeSymbol.GetMembers()
        .OfType<IPropertySymbol>()
        .Select(GetPropertyOverview);
  }

  /// <summary>
  /// Creates a <see cref="PropertyOverview"/> object containing metadata about the specified property symbol.
  /// </summary>
  /// <param name="propertySymbol">The property symbol for which the metadata will be retrieved.</param>
  /// <returns>
  /// A <see cref="PropertyOverview"/> instance that provides information such as accessibility, setter presence, type, initializer expression, and name of the specified property.
  /// </returns>
  public static PropertyOverview GetPropertyOverview(this IPropertySymbol propertySymbol) {
    return new PropertyOverview(propertySymbol) {
        Accessibility = propertySymbol.DeclaredAccessibility.ToAccessibilityLevel(),
        HasSetter = propertySymbol.SetMethod is not null,
        Initializer = propertySymbol.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<PropertyDeclarationSyntax>()
            .Where(x => x.Initializer is not null)
            .Select(x => x.Initializer!.Value)
            .FirstOrDefault()
    };
  }
}
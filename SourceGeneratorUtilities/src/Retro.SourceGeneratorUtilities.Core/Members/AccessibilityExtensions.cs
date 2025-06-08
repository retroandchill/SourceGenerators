using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

/// <summary>
/// Provides extension methods for converting Microsoft.CodeAnalysis.Accessibility
/// values to AccessibilityLevel enumeration.
/// </summary>
public static class AccessibilityExtensions {

  /// <summary>
  /// Converts a <see cref="Microsoft.CodeAnalysis.Accessibility"/> value to an <see cref="AccessibilityLevel"/> enumeration value.
  /// </summary>
  /// <param name="accessibility">The <see cref="Microsoft.CodeAnalysis.Accessibility"/> value to convert.</param>
  /// <returns>An <see cref="AccessibilityLevel"/> value that represents the equivalent accessibility level.</returns>
  public static AccessibilityLevel ToAccessibilityLevel(this Accessibility accessibility) {
    return accessibility switch {
        Accessibility.Public => AccessibilityLevel.Public,
        Accessibility.Internal => AccessibilityLevel.Internal,
        Accessibility.Protected => AccessibilityLevel.Protected,
        Accessibility.Private => AccessibilityLevel.Private,
        Accessibility.ProtectedAndInternal => AccessibilityLevel.ProtectedInternal,
        _ => AccessibilityLevel.Private
    };
  }
}
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Members;

public static class AccessibilityExtensions {
  public static string ToDisplayString(this Accessibility accessibility) {
    return accessibility switch {
        Accessibility.Public => "public",
        Accessibility.Internal => "internal",
        Accessibility.Protected => "protected",
        Accessibility.Private => "private",
        _ => ""
    };
  }
}
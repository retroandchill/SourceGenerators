using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

public static class AccessibilityExtensions {

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
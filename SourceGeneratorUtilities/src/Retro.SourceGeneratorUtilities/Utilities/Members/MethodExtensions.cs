using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Types;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Members;

/// <summary>
/// Provides extension methods for working with methods represented as <see cref="IMethodSymbol"/>.
/// </summary>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal static class MethodExtensions {

  /// <summary>
  /// Determines whether two methods have a compatible or common return type.
  /// </summary>
  /// <param name="method">The current method symbol to compare.</param>
  /// <param name="otherMethod">The other method symbol to compare against.</param>
  /// <returns>
  /// True if the methods have a common or compatible return type; otherwise, false.
  /// </returns>
  public static bool HasCommonReturnType(this IMethodSymbol method, IMethodSymbol otherMethod) {
    if (method.ReturnsVoid) {
      return true;
    }

    return !otherMethod.ReturnsVoid && otherMethod.ReturnType.IsAssignableTo(method.ReturnType);
  }

  /// <summary>
  /// Determines whether a method can be called from another method based on their static nature.
  /// </summary>
  /// <param name="method">The method symbol that needs to be callable.</param>
  /// <param name="otherMethod">The method symbol from which the first method is being checked for callability.</param>
  /// <returns>
  /// True if the method can be called from the other method; otherwise, false.
  /// </returns>
  public static bool IsCallableFrom(this IMethodSymbol method, IMethodSymbol otherMethod) {
    return otherMethod.IsStatic || method.IsStatic == otherMethod.IsStatic;
  }
  
}
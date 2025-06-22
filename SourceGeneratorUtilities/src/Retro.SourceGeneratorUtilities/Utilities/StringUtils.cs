#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities;

/// <summary>
/// Provides utility methods for string manipulation and operations.
/// </summary>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal static class StringUtils {
  /// <summary>
  /// Concatenates the elements of a sequence of strings, using the specified separator between each element.
  /// </summary>
  /// <param name="strings">The sequence of strings to join.</param>
  /// <param name="separator">The string to use as a separator.</param>
  /// <returns>A single concatenated string containing the elements of the sequence, separated by the specified separator.</returns>
  public static string Joining(this IEnumerable<string> strings, string separator) {
    return string.Join(separator, strings);
  }
}
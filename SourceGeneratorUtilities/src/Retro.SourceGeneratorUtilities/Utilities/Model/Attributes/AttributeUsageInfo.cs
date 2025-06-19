#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;

/// <summary>
/// Represents metadata about the usage of an attribute including its valid targets,
/// whether multiple instances can be applied, and whether it is inherited.
/// </summary>
/// <param name="ValidOn">The valid targets for the attribute.</param>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
public record struct AttributeUsageInfo(AttributeTargets ValidOn) {
  /// <summary>
  /// Gets or sets a value indicating whether multiple instances of the attribute can be applied
  /// to a single program element.
  /// </summary>
  public bool AllowMultiple { get; init; }

  /// <summary>
  /// Gets or sets a value indicating whether the attribute is inherited by derived classes
  /// and overridden members of a base class.
  /// </summary>
  public bool Inherited { get; init; }
}
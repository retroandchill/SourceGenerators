using System.Collections.Immutable;
namespace Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;

/// <summary>
/// Represents an overview of a constructor within an attribute information model.
/// </summary>
public record struct AttributeInfoConstructorOverview {
  /// <summary>
  /// Gets or sets the list of parameters associated with the attribute constructor overview.
  /// </summary>
  /// <remarks>
  /// Each parameter is represented as an <see cref="AttributeInfoConstructorParamOverview"/> record,
  /// which provides detailed information about the associated parameter symbol.
  /// </remarks>
  public required ImmutableList<AttributeInfoConstructorParamOverview> Parameters { get; init; }

  /// <summary>
  /// Gets a value indicating whether the constructor has parameters defined.
  /// </summary>
  /// <remarks>
  /// This property evaluates the count of the parameter list in the constructor overview.
  /// If the count is greater than zero, it indicates that the constructor contains parameters.
  /// </remarks>
  public bool HasParameters => Parameters.Count > 0;

  /// <summary>
  /// Indicates whether the current item is the last within its containing collection or sequence.
  /// </summary>
  /// <remarks>
  /// This property is primarily used to determine position-related logic when iterating over or processing elements.
  /// </remarks>
  public bool IsLast { get; init; }
}
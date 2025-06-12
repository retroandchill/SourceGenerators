using System.Collections.Generic;
using System.Collections.Immutable;

namespace Retro.SourceGeneratorUtilities.Core.Model.Attributes;

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
  /// Indicates whether the current item is the last within its containing collection or sequence.
  /// </summary>
  /// <remarks>
  /// This property is primarily used to determine position-related logic when iterating over or processing elements.
  /// </remarks>
  public bool IsLast { get; init; }
}
namespace Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;

/// <summary>
/// Represents an overview of a type parameter associated with an attribute.
/// Provides information about the position of the parameter and whether it is the last parameter.
/// </summary>
/// <param name="Index">The zero-based index of the type parameter.</param>
public record struct AttributeTypeParameterOverview(int Index) {
  /// <summary>
  /// Gets a value indicating whether this is the last parameter in the sequence of attribute type parameters.
  /// </summary>
  public bool IsLast { get; init; }
}
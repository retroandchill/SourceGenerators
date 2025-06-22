using Microsoft.CodeAnalysis;
namespace Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;

/// <summary>
/// Represents a type validation structure that encapsulates the kind of a type
/// and whether it is the last element in the collection of valid types.
/// </summary>
/// <param name="Kind">The string representation of the kind of type</param>
public record struct ValidOnType(string Kind) {
  /// <summary>
  /// Specifies whether the current instance is the last element in a collection of valid types.
  /// </summary>
  /// <remarks>
  /// This property is typically used to identify the position of the current type in a sequence
  /// of valid types, especially within type validation logic.
  /// </remarks>
  public bool IsLast { get; init; }

  /// <summary>
  /// Defines an implicit operator that converts a <see cref="TypeKind"/> instance
  /// to a <see cref="ValidOnType"/> instance by using the string representation
  /// of the specified <see cref="TypeKind"/>.
  /// </summary>
  /// <param name="kind">The TypeKind to be converted into a ValidOnType instance.</param>
  /// <returns>A ValidOnType instance with its Kind property set to the string representation of the input TypeKind.</returns>
  public static implicit operator ValidOnType(TypeKind kind) {
    return new ValidOnType(kind.ToString());
  }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;

/// <summary>
/// Represents an overview of a property within an attribute information model.
/// </summary>
/// <param name="Symbol">The <see cref="IPropertySymbol"/> representing the property.</param>
public readonly record struct AttributeInfoPropertyOverview(IPropertySymbol Symbol) {
  /// <summary>
  /// Gets the <see cref="ITypeSymbol"/> that represents the type of the property.
  /// </summary>
  public ITypeSymbol Type => Symbol.Type;

  /// <summary>
  /// Gets the name of the property represented by the <see cref="IPropertySymbol"/>.
  /// </summary>
  public string Name => Symbol.Name;

  /// <summary>
  /// Gets a value indicating whether the property has a default value specified.
  /// </summary>
  public bool HasDefaultValue => DefaultValue is not null;

  /// <summary>
  /// Gets the syntax node representing the default value of the property, if one is specified.
  /// </summary>
  public required ExpressionSyntax? DefaultValue { get; init; }

  /// <summary>
  /// Indicates whether the current property is the last one in a sequence or collection.
  /// </summary>
  public bool IsLast { get; init; }
}
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Types;
namespace Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;

/// <summary>
/// Represents an overview of a constructor parameter associated with an attribute.
/// </summary>
/// <remarks>
/// This record struct encapsulates details of a constructor parameter, including its symbol,
/// type, name, index, and its position as the last parameter (if applicable).
/// </remarks>
/// <param name="Symbol">
/// The <see cref="IParameterSymbol"/> representing the constructor parameter.
/// </param>
public record struct AttributeInfoConstructorParamOverview(IParameterSymbol Symbol) {
  /// <summary>
  /// Gets the type of the constructor parameter.
  /// </summary>
  /// <remarks>
  /// This property returns the <see cref="ITypeSymbol"/> representing the type of the associated constructor parameter.
  /// It reflects the type information as defined in the symbol metadata.
  /// </remarks>
  public ITypeSymbol Type => Symbol.Type;

  /// <summary>
  /// Gets the non-nullable type of the constructor parameter.
  /// </summary>
  /// <remarks>
  /// This property retrieves the <see cref="ITypeSymbol"/> of the constructor parameter with its nullable annotation
  /// set to <see cref="NullableAnnotation.NotAnnotated"/>. It provides a type representation that is explicitly
  /// non-nullable, regardless of the original nullable annotation of the parameter type.
  /// </remarks>
  public string NonNullableType => Type.IsSameType<ITypeSymbol>() ? typeof(Type).FullName! : 
      Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();

  /// <summary>
  /// Gets the name of the constructor parameter.
  /// </summary>
  /// <remarks>
  /// This property provides the identifier name of the constructor parameter as defined in the associated <see cref="IParameterSymbol"/>.
  /// It directly reflects the parameter's declared name within the symbol metadata.
  /// </remarks>
  public string Name => Symbol.Name;

  /// <summary>
  /// Gets or sets the index of the constructor parameter.
  /// </summary>
  /// <remarks>
  /// This property represents the position of the parameter within the constructor's parameter list.
  /// It is a required value and is initialized during the construction of the <see cref="AttributeInfoConstructorParamOverview"/> instance.
  /// </remarks>
  public required int Index { get; init; }

  /// <summary>
  /// Indicates whether this parameter is the last in the collection of parameters.
  /// </summary>
  /// <remarks>
  /// This property returns a boolean value that is <c>true</c> if the parameter is the last one
  /// in the constructor parameter list; otherwise, it is <c>false</c>.
  /// </remarks>
  public bool IsLast { get; init; }
}
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Types;
namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents an overview of a property, including its metadata, such as type, name,
/// accessibility level, and other characteristics.
/// </summary>
/// <remarks>
/// This class is designed to provide essential metadata about properties to facilitate
/// code generation and analysis. It includes information such as the property type,
/// name, whether the property has a setter, and whether it has an initializer expression.
/// </remarks>
/// <param name="Symbol">
/// The symbol representing the property in the code. This symbol is used to extract
/// the property's type, name, and other characteristics during initialization.
/// </param>
public record PropertyOverview(IPropertySymbol Symbol) {

  /// <summary>
  /// Gets the type of the property.
  /// </summary>
  /// <remarks>
  /// This property represents the type of the associated property as defined in the source code.
  /// It is extracted from the property's symbol, which provides rich metadata about the code element.
  /// </remarks>
  public ITypeSymbol Type { get; init; } = Symbol.Type;

  /// <summary>
  /// Gets the name of the property.
  /// </summary>
  /// <remarks>
  /// This property represents the identifier name of the property as defined in the source code.
  /// It is extracted from the symbol representing the property, which provides metadata such as
  /// the property's declared name within its source context.
  /// </remarks>
  public string Name { get; init; } = Symbol.Name;

  /// <summary>
  /// Gets the accessibility level of the property.
  /// </summary>
  /// <remarks>
  /// This property indicates the declared accessibility of the property, such as public, private, internal, protected, or protected internal.
  /// It is determined based on the property's symbol metadata and provides information about the visibility scope of the property within the code.
  /// </remarks>
  public AccessibilityLevel Accessibility { get; init; } = AccessibilityLevel.Private;

  /// <summary>
  /// Indicates whether the property has a setter method.
  /// </summary>
  /// <remarks>
  /// This property evaluates to true if the associated property includes a setter,
  /// allowing its value to be updated. If the property is read-only, this will return false.
  /// </remarks>
  public bool HasSetter { get; init; }

  /// <summary>
  /// Determines whether the property has an initializer expression.
  /// </summary>
  /// <remarks>
  /// This property indicates whether the associated property in the source code
  /// is initialized with a value when declared. It checks if the property includes
  /// an initializer expression, which is represented by the <see cref="Initializer"/> property.
  /// </remarks>
  public bool HasInitializer => Initializer is not null;

  /// <summary>
  /// Gets or sets the initializer expression of the property, if any.
  /// </summary>
  /// <remarks>
  /// This property represents the initializer expression assigned to the property at the point of its declaration in the source code.
  /// If no initializer is present, this property will return null. This is used to capture the explicit value initialization of the property.
  /// </remarks>
  public ExpressionSyntax? Initializer { get; init; }
  
}
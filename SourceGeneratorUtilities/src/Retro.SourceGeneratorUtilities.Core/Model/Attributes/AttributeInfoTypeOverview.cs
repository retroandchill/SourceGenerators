using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Core.Model.Attributes;

/// <summary>
/// Represents an overview of the types related to a specific attribute and the model it applies to.
/// This class provides metadata including the namespace, the name of the model,
/// constructors, properties, and any related child classes.
/// </summary>
/// <remarks>
/// This record aggregates information about a model type and its associated attribute type.
/// It is designed to facilitate analysis and generation of source code or metadata
/// by providing a structured representation of the relationship between a model
/// and the attribute applied to it.
/// </remarks>
/// <param name="ModelSymbol">The <see cref="INamedTypeSymbol"/> representing the model type.</param>
/// <param name="AttributeSymbol">The <see cref="INamedTypeSymbol"/> representing the attribute type.</param>
public record AttributeInfoTypeOverview(INamedTypeSymbol ModelSymbol, INamedTypeSymbol AttributeSymbol) {
  /// <summary>
  /// Gets the namespace containing the model type.
  /// </summary>
  /// <remarks>
  /// This property provides access to the <see cref="INamespaceSymbol"/> that represents
  /// the namespace in which the model type is defined.
  /// </remarks>
  public INamespaceSymbol Namespace => ModelSymbol.ContainingNamespace;

  /// <summary>
  /// Gets the name of the model type associated with the attribute.
  /// </summary>
  /// <remarks>
  /// This property retrieves the name of the type represented by <see cref="ModelSymbol"/>,
  /// which is the model type to which the attribute is applied. It serves as a
  /// shorthand for accessing the name of the associated model.
  /// </remarks>
  public string Name => ModelSymbol.Name;

  /// <summary>
  /// Gets the type name of the attribute as a string representation.
  /// </summary>
  /// <remarks>
  /// This property utilizes the extension method <see cref="TypeExtensions.GetTypeofName"/>
  /// to extract the type name of the attribute associated with the model.
  /// It provides a formatted string suitable for source generation or analysis purposes.
  /// </remarks>
  public string AttributeTypeofName => AttributeSymbol.GetTypeofName();


  /// <summary>
  /// Gets the constructors available for the attribute type overview.
  /// </summary>
  /// <remarks>
  /// This property provides access to an immutable array of <see cref="AttributeInfoConstructorOverview"/> objects
  /// representing the constructors defined for the attribute associated with the model type.
  /// </remarks>
  public required ImmutableArray<AttributeInfoConstructorOverview> Constructors { get; init; }

  /// <summary>
  /// Gets the collection of properties associated with the type overview.
  /// </summary>
  /// <remarks>
  /// This property provides an immutable array of <see cref="AttributeInfoPropertyOverview"/> instances,
  /// representing the properties related to the attribute type overview.
  /// </remarks>
  public required ImmutableArray<AttributeInfoPropertyOverview> Properties { get; init; }

  /// <summary>
  /// Gets the collection of child classes associated with the current attribute type overview.
  /// </summary>
  /// <remarks>
  /// This property provides access to an immutable array of <see cref="ChildAttributeTypeInfoOverview"/>,
  /// representing the child classes that are related to the attribute type in this context.
  /// </remarks>
  public required ImmutableArray<ChildAttributeTypeInfoOverview> ChildClasses { get; init; }
}
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents an overview of a data type, providing metadata such as its symbol, namespace,
/// base type, constructors, and properties.
/// </summary>
public record DataTypeOverview {

  /// <summary>
  /// Gets the <see cref="INamedTypeSymbol"/> representing the metadata of the current data type.
  /// </summary>
  /// <remarks>
  /// This property stores the symbol that provides detailed information about the type,
  /// including its name, namespace, base type, and members. It serves as the foundation
  /// for fetching metadata and performing type analysis.
  /// </remarks>
  public required INamedTypeSymbol Symbol { get; init; }

  /// <summary>
  /// Gets the <see cref="INamespaceSymbol"/> representing the namespace associated with the current type.
  /// </summary>
  /// <remarks>
  /// This property provides access to the namespace in which the current type is defined. It can be utilized
  /// to retrieve or analyze the hierarchical structure of namespaces in the type's metadata.
  /// </remarks>
  public INamespaceSymbol Namespace => Symbol.ContainingNamespace;

  /// <summary>
  /// Gets the name of the current data type represented by the <see cref="Symbol"/> property.
  /// </summary>
  /// <remarks>
  /// This property retrieves the simple name of the type as defined in its metadata (e.g., "MyClass" or "MyStruct").
  /// It does not include namespace or other qualifiers.
  /// </remarks>
  public string Name => Symbol.Name;

  /// <summary>
  /// Gets the <see cref="DataTypeOverview"/> representing the base type of the current data type, if one exists.
  /// </summary>
  /// <remarks>
  /// This property provides metadata about the base type of the current type, which may include information such as
  /// its symbol, constructors, and properties. If the current type does not have a base type, the value will be null.
  /// </remarks>
  public required DataTypeOverview? Base { get; init; }

  /// <summary>
  /// Gets the fully qualified name of the base type as a string representation, if one exists.
  /// </summary>
  /// <remarks>
  /// This property retrieves the name of the base type associated with the current data type.
  /// If the data type does not inherit from another type, this property returns null. The
  /// base type's name is derived from the symbol of the base type and includes namespace
  /// information for clarity.
  /// </remarks>
  public string? BaseName => Base?.Symbol.ToDisplayString();

  /// <summary>
  /// Gets the collection of <see cref="ConstructorOverview"/> objects representing the constructors of the current data type.
  /// </summary>
  /// <remarks>
  /// This property provides metadata about the constructors defined within the data type, detailing their parameters, accessibility,
  /// and whether they are primary constructors. It is particularly useful for analyzing and generating code based on the available
  /// constructors in a data type.
  /// </remarks>
  public required IReadOnlyList<ConstructorOverview> Constructors { get; init; }

  /// <summary>
  /// Gets the collection of <see cref="PropertyOverview"/> instances representing the properties
  /// defined in the current data type, including relevant metadata and characteristics for each property.
  /// </summary>
  /// <remarks>
  /// This property provides detailed information about each property in the type, such as its type,
  /// name, accessibility level, and whether it has a setter or an initializer. It is essential for
  /// analyzing property-specific metadata and generating code dependent on property data.
  /// </remarks>
  public required IReadOnlyList<PropertyOverview> Properties { get; init; }
  
}
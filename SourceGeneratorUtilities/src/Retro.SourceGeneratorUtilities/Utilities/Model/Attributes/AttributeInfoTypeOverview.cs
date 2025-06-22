using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Types;
namespace Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;

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
  /// Indicates whether the model type represented by this instance is a value type.
  /// </summary>
  /// <remarks>
  /// This property reflects the <see cref="INamedTypeSymbol.IsValueType"/> property of the model symbol.
  /// It allows determination of whether the model type is a value type, such as a struct,
  /// as opposed to a reference type like a class or an interface.
  /// </remarks>
  public bool IsValueType => ModelSymbol.IsValueType;

  /// <summary>
  /// Gets a value indicating whether the model type is an unbound generic type.
  /// </summary>
  /// <remarks>
  /// An unbound generic type is a type that defines generic parameters but does not specify
  /// any specific type arguments. This property checks if the model type represented by
  /// <see cref="INamedTypeSymbol"/> is such a type.
  /// </remarks>
  public bool IsUnboundGeneric => AttributeSymbol.IsGenericType && AttributeSymbol.TypeArguments.All(a => a is ITypeParameterSymbol);
  
  public required bool AllowMultiple { get; init; }
  
  public required AttributeTargets ValidOn { get; init; }
  
  public bool ValidOnAssembly => ValidOn.HasFlag(AttributeTargets.Assembly);
  
  public bool ValidOnModule => ValidOn.HasFlag(AttributeTargets.Module);
  
  public bool ValidOnClass => ValidOn.HasFlag(AttributeTargets.Class);
  
  public bool ValidOnStruct => ValidOn.HasFlag(AttributeTargets.Struct);
  
  public bool ValidOnInterface => ValidOn.HasFlag(AttributeTargets.Interface);
  
  public bool ValidOnEnum => ValidOn.HasFlag(AttributeTargets.Enum);
  
  public bool ValidOnDelegate => ValidOn.HasFlag(AttributeTargets.Delegate);
  
  public bool ValidOnNamedType => ValidOnClass || ValidOnStruct || ValidOnInterface || ValidOnEnum || ValidOnDelegate;
  
  public bool ValidOnMethod => ValidOn.HasFlag(AttributeTargets.Method);
  
  public bool ValidOnConstructor => ValidOn.HasFlag(AttributeTargets.Constructor);
  
  public bool ValidOnReturnValue => ValidOn.HasFlag(AttributeTargets.ReturnValue);
  
  public bool ValidOnParameter => ValidOn.HasFlag(AttributeTargets.Parameter);
  
  public bool ValidOnAnyMethod => ValidOnMethod || ValidOnConstructor;
  
  public bool ValidOnProperty => ValidOn.HasFlag(AttributeTargets.Property);
  
  public bool ValidOnField => ValidOn.HasFlag(AttributeTargets.Field);
  
  public bool ValidOnEvent => ValidOn.HasFlag(AttributeTargets.Event);
  
  public bool ValidOnGenericParameter => ValidOn.HasFlag(AttributeTargets.GenericParameter);
  

  /// <summary>
  /// Gets the constructors available for the attribute type overview.
  /// </summary>
  /// <remarks>
  /// This property provides access to an immutable array of <see cref="AttributeInfoConstructorOverview"/> objects
  /// representing the constructors defined for the attribute associated with the model type.
  /// </remarks>
  public required ImmutableArray<AttributeInfoConstructorOverview> Constructors { get; init; }

  /// <summary>
  /// Gets the collection of type parameters for the attribute type.
  /// </summary>
  /// <remarks>
  /// This property provides an immutable array of <see cref="AttributeTypeParameterOverview"/>
  /// instances that describe the type parameters defined for the attribute type.
  /// </remarks>
  public required ImmutableArray<AttributeTypeParameterOverview> TypeParameters { get; init; }

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
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

  /// <summary>
  /// Indicates whether multiple instances of the attribute can be applied to a target.
  /// </summary>
  /// <remarks>
  /// This property specifies if the attribute is allowable to be applied more than once
  /// to the same program element, offering greater flexibility in its usage when set to true.
  /// </remarks>
  public required bool AllowMultiple { get; init; }

  /// <summary>
  /// Gets the attribute targets that specify the program elements
  /// on which the attribute is valid.
  /// </summary>
  /// <remarks>
  /// This property provides access to a combination of <see cref="AttributeTargets"/> flags,
  /// indicating where the attribute can be applied, such as classes, methods, parameters, etc.
  /// </remarks>
  public required AttributeTargets ValidOn { get; init; }

  /// <summary>
  /// Gets a value indicating whether the attribute can be applied to an assembly.
  /// </summary>
  /// <remarks>
  /// This property evaluates the defined attribute targets and checks if the
  /// <see cref="AttributeTargets.Assembly"/> flag is set, indicating the attribute's applicability.
  /// </remarks>
  public bool ValidOnAssembly => ValidOn.HasFlag(AttributeTargets.Assembly);

  /// <summary>
  /// Indicates whether the attribute is valid on a module.
  /// </summary>
  /// <remarks>
  /// This property evaluates whether the <see cref="AttributeTargets.Module"/> flag
  /// is set in the <c>ValidOn</c> property, which determines the applicability of the attribute to a module.
  /// </remarks>
  public bool ValidOnModule => ValidOn.HasFlag(AttributeTargets.Module);

  /// <summary>
  /// Determines whether the attribute can be applied to a class.
  /// </summary>
  /// <remarks>
  /// This property checks if the <see cref="AttributeTargets.Class"/> flag is set in the
  /// <see cref="ValidOn"/> property, indicating that the attribute is valid for use on class types.
  /// </remarks>
  public bool ValidOnClass => ValidOn.HasFlag(AttributeTargets.Class);

  /// <summary>
  /// Indicates whether the attribute can be applied to struct types.
  /// </summary>
  /// <remarks>
  /// This property evaluates whether the <see cref="AttributeTargets.Struct"/> flag
  /// is included in the defined <see cref="ValidOn"/> targets of the attribute.
  /// </remarks>
  public bool ValidOnStruct => ValidOn.HasFlag(AttributeTargets.Struct);

  /// <summary>
  /// Indicates whether the attribute can be applied to an interface.
  /// </summary>
  /// <remarks>
  /// This property determines if the attribute is valid on interfaces based on the <see cref="AttributeTargets"/> flags.
  /// The result evaluates whether the <see cref="AttributeTargets.Interface"/> flag is set for the attribute.
  /// </remarks>
  public bool ValidOnInterface => ValidOn.HasFlag(AttributeTargets.Interface);

  /// <summary>
  /// Indicates whether the attribute is valid on enumeration types.
  /// </summary>
  /// <remarks>
  /// This property checks if the attribute's targets include <see cref="AttributeTargets.Enum"/>.
  /// It is useful for determining if the attribute can be applied to enumeration definitions.
  /// </remarks>
  public bool ValidOnEnum => ValidOn.HasFlag(AttributeTargets.Enum);

  /// <summary>
  /// Gets a value indicating whether the attribute is valid when applied to a delegate.
  /// </summary>
  /// <remarks>
  /// This property checks if the <see cref="AttributeTargets.Delegate"/> flag is set
  /// in the <see cref="ValidOn"/> property, determining if the attribute can target delegate types.
  /// </remarks>
  public bool ValidOnDelegate => ValidOn.HasFlag(AttributeTargets.Delegate);

  /// <summary>
  /// Determines whether the attribute can be applied to named type declarations.
  /// </summary>
  /// <remarks>
  /// This property evaluates to <c>true</c> when the attribute is valid on
  /// classes, structs, interfaces, enums, or delegates, as represented by the
  /// respective flags in the <see cref="AttributeTargets"/> enumeration.
  /// </remarks>
  public bool ValidOnNamedType => ValidOnClass || ValidOnStruct || ValidOnInterface || ValidOnEnum || ValidOnDelegate;

  /// <summary>
  /// Gets the collection of types on which the attribute can be applied.
  /// </summary>
  /// <remarks>
  /// This property aggregates the possible <see cref="ValidOnType"/> instances based on
  /// the <see cref="AttributeTargets"/> flags specified in the <c>ValidOn</c> property. It includes
  /// types such as classes, structs, interfaces, enums, and delegates, depending on their validity.
  /// </remarks>
  public ImmutableArray<ValidOnType> ValidOnTypes {
    get {
      var validOnTypes = new List<ValidOnType>();
      if (ValidOnClass) validOnTypes.Add(TypeKind.Class);
      if (ValidOnStruct) validOnTypes.Add(TypeKind.Struct);
      if (ValidOnInterface) validOnTypes.Add(TypeKind.Interface);
      if (ValidOnEnum) validOnTypes.Add(TypeKind.Enum);
      if (ValidOnDelegate) validOnTypes.Add(TypeKind.Delegate);
      
      return [..validOnTypes.Select((x, i) => x with { IsLast = i == validOnTypes.Count - 1})];
    }
  }

  /// <summary>
  /// Gets a value indicating whether the attribute is valid on all supported type-level targets,
  /// including class, interface, enum, and delegate types.
  /// </summary>
  /// <remarks>
  /// This property evaluates to <see langword="true"/> if the attribute is applicable to
  /// <see cref="AttributeTargets.Class"/>, <see cref="AttributeTargets.Interface"/>,
  /// <see cref="AttributeTargets.Enum"/>, and <see cref="AttributeTargets.Delegate"/> collectively.
  /// Otherwise, it returns <see langword="false"/>.
  /// </remarks>
  public bool ValidOnAnyType => ValidOnClass && ValidOnInterface && ValidOnEnum && ValidOnDelegate;

  /// <summary>
  /// Indicates whether the attribute can be applied to methods.
  /// </summary>
  /// <remarks>
  /// This property determines if the <see cref="AttributeTargets.Method"/> flag
  /// is set in the <c>ValidOn</c> property, which specifies the allowed application
  /// contexts for the attribute.
  /// </remarks>
  public bool ValidOnMethod => ValidOn.HasFlag(AttributeTargets.Method);

  /// <summary>
  /// Indicates whether the attribute can be applied to constructors.
  /// </summary>
  /// <remarks>
  /// This property evaluates if the <see cref="AttributeTargets.Constructor"/> flag is set
  /// in the <c>ValidOn</c> attribute targets, denoting its applicability to constructors.
  /// </remarks>
  public bool ValidOnConstructor => ValidOn.HasFlag(AttributeTargets.Constructor);

  /// <summary>
  /// Gets a value indicating whether the attribute is valid on both methods and constructors.
  /// </summary>
  /// <remarks>
  /// This property evaluates to <c>true</c> if the attribute can be applied to methods
  /// or constructors, and <c>false</c> otherwise. It combines the validation checks for
  /// both <see cref="AttributeTargets.Method"/> and <see cref="AttributeTargets.Constructor"/>.
  /// </remarks>
  public bool ValidOnAnyMethod => ValidOnMethod || ValidOnConstructor;

  /// <summary>
  /// Gets a value indicating whether the attribute is valid exclusively on constructors.
  /// </summary>
  /// <remarks>
  /// This property evaluates if the attribute can be applied only to constructors and not to methods.
  /// It returns true if the attribute targets constructors but not methods; otherwise, false.
  /// </remarks>
  public bool OnlyValidOnConstructor => ValidOnConstructor && !ValidOnMethod;

  /// <summary>
  /// Gets a value indicating whether the attribute is valid when applied
  /// to the return value of a method.
  /// </summary>
  /// <remarks>
  /// This property evaluates whether the <see cref="AttributeTargets.ReturnValue"/>
  /// flag is present in the <see cref="AttributeTargets"/> associated with the attribute.
  /// </remarks>
  public bool ValidOnReturnValue => ValidOn.HasFlag(AttributeTargets.ReturnValue);

  /// <summary>
  /// Indicates whether the attribute can be applied to parameters.
  /// </summary>
  /// <remarks>
  /// This property returns a boolean value that determines if the <see cref="AttributeTargets.Parameter"/> flag
  /// is included in the <see cref="ValidOn"/> property. Use this to check if the attribute is valid for parameters.
  /// </remarks>
  public bool ValidOnParameter => ValidOn.HasFlag(AttributeTargets.Parameter);

  /// <summary>
  /// Determines whether the attribute is applicable to properties.
  /// </summary>
  /// <remarks>
  /// This property evaluates whether the attribute's target includes <see cref="AttributeTargets.Property"/>.
  /// It uses the <see cref="AttributeTargets"/> enumeration to identify if properties are a valid declaration context
  /// for the associated attribute.
  /// </remarks>
  public bool ValidOnProperty => ValidOn.HasFlag(AttributeTargets.Property);

  /// <summary>
  /// Indicates whether the attribute is valid on a field.
  /// </summary>
  /// <remarks>
  /// This property evaluates whether the attribute target includes <see cref="AttributeTargets.Field"/>.
  /// Returns <c>true</c> if the attribute can be applied to fields; otherwise, returns <c>false</c>.
  /// </remarks>
  public bool ValidOnField => ValidOn.HasFlag(AttributeTargets.Field);

  /// <summary>
  /// Indicates whether the attribute is applicable to events.
  /// </summary>
  /// <remarks>
  /// This property evaluates whether the <see cref="AttributeTargets.Event"/> flag
  /// is set on the <see cref="ValidOn"/> property, determining if the attribute can
  /// be applied to event declarations.
  /// </remarks>
  public bool ValidOnEvent => ValidOn.HasFlag(AttributeTargets.Event);

  /// <summary>
  /// Determines whether the attribute can be applied to a generic parameter.
  /// </summary>
  /// <remarks>
  /// This property evaluates if the <see cref="AttributeTargets.GenericParameter"/> flag
  /// is set within the <see cref="ValidOn"/> property, indicating the attribute's applicability
  /// to generic parameters.
  /// </remarks>
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
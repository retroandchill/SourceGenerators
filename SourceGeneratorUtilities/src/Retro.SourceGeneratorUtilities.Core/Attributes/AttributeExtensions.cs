using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Model;
using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Core.Attributes;

/// <summary>
/// Provides extension methods for working with <see cref="AttributeData"/> instances
/// to facilitate processing and extracting attribute-related information in the context
/// of source generation.
/// </summary>
public static class AttributeExtensions {
  /// <summary>
  /// Converts an <see cref="AttributeData"/> instance into an <see cref="AttributeInfo{T}"/> object,
  /// allowing easy access to attribute-related information in a strongly-typed manner.
  /// </summary>
  /// <typeparam name="T">The type of the attribute to be processed. Must be a subclass of <see cref="Attribute"/>.</typeparam>
  /// <param name="attributeData">The <see cref="AttributeData"/> instance to be converted.</param>
  /// <returns>An <see cref="AttributeInfo{T}"/> object representing the attribute information.</returns>
  public static AttributeInfo<T> GetInfo<T>(this AttributeData attributeData) where T : Attribute {
    return new AttributeInfo<T>(attributeData);
  }

  /// <summary>
  /// Converts a collection of <see cref="AttributeData"/> instances into a collection of <see cref="AttributeInfo{T}"/> objects,
  /// filtering and mapping attributes of the specified type for easy access to attribute-related information.
  /// </summary>
  /// <typeparam name="T">The type of the attribute to be processed. Must be a subclass of <see cref="Attribute"/>.</typeparam>
  /// <param name="attributeData">The collection of <see cref="AttributeData"/> instances to be processed.</param>
  /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="AttributeInfo{T}"/> objects representing the attribute information
  /// for attributes of type <typeparamref name="T"/>.</returns>
  public static IEnumerable<AttributeInfo<T>> GetInfo<T>(this IEnumerable<AttributeData> attributeData) where T : Attribute {
    return attributeData
        .Where(x => x.AttributeClass?.IsSameType<T>() ?? false)
        .Select(x => x.GetInfo<T>());
  }

  /// <summary>
  /// Determines whether the given <see cref="AttributeData"/> has a constructor
  /// that matches the specified parameter types.
  /// </summary>
  /// <param name="attributeData">
  /// The <see cref="AttributeData"/> instance to be analyzed.
  /// </param>
  /// <param name="constructorTypes">
  /// An array of <see cref="Type"/> objects representing the parameter types
  /// of the constructor to match.
  /// </param>
  /// <returns>
  /// <c>true</c> if the <see cref="AttributeData"/> instance has a constructor
  /// with parameters matching the specified types; otherwise, <c>false</c>.
  /// </returns>
  public static bool HasMatchingConstructor(this AttributeData attributeData, params Type[] constructorTypes) {
    if (attributeData.ConstructorArguments.Length != constructorTypes.Length) {
      return false;
    }

    for (var i = 0; i < attributeData.ConstructorArguments.Length; i++) {
      var constructorType = constructorTypes[i];
      var constructorArgument = attributeData.ConstructorArguments[i];

      if (!constructorArgument.Type?.IsSameType(constructorType) ?? false) {
        return false;
      }
    }

    return true;
  }

  public static IEnumerable<INamedTypeSymbol> GetAttributeInfoTypes(this IAssemblySymbol assembly) {
    return assembly.GetAttributes()
        .Where(a => a.AttributeClass?.IsOfType<AttributeInfoTypeAttribute>() ?? false)
        .Select(GetAttributeInfoType)
        .Where(x => x is not null)
        .SelectMany(ExtractAttributeTypes)
        .Distinct(NamedTypeSymbolEqualityComparer.Default);
  }

  private static INamedTypeSymbol? GetAttributeInfoType(this AttributeData extractedAttribute) {
    if (extractedAttribute?.AttributeClass is null) {
      return null;
    }

    var attributeClass = extractedAttribute.AttributeClass;
    if (attributeClass.IsGenericType && attributeClass.ConstructedFrom.IsSameType(typeof(AttributeInfoTypeAttribute<>))) {
      return attributeClass.TypeArguments[0] as INamedTypeSymbol;
    }
    
    if (attributeClass.IsSameType<AttributeInfoTypeAttribute>()) {
      return extractedAttribute.ConstructorArguments[0].Value as INamedTypeSymbol;
    }

    throw new InvalidOperationException("Invalid attribute type");
  }
  
  private static IEnumerable<INamedTypeSymbol> ExtractAttributeTypes(INamedTypeSymbol? namedType) {
    if (namedType is null) {
      yield break;
    }

    while (!namedType.IsSameType<Attribute>()) {
      yield return namedType;
      if (namedType.BaseType is null) {
        break;
      }

      namedType = namedType.BaseType;
    }
  }
}
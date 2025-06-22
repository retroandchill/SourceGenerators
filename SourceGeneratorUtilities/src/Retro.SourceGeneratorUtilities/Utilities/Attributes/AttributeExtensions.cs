using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Types;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Attributes;

/// <summary>
/// Provides extension methods for working with <see cref="AttributeData"/> instances
/// to facilitate processing and extracting attribute-related information in the context
/// of source generation.
/// </summary>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal static class AttributeExtensions {
  /// <summary>
  /// Determines whether the specified <see cref="ISymbol"/> has an attribute
  /// that matches the given <see cref="Type"/>.
  /// </summary>
  /// <param name="symbol">
  /// The <see cref="ISymbol"/> to check for the presence of the attribute.
  /// </param>
  /// <param name="attributeType">
  /// The <see cref="Type"/> of the attribute to check for.
  /// </param>
  /// <returns>
  /// <c>true</c> if the <see cref="ISymbol"/> has an attribute of the specified type; otherwise, <c>false</c>.
  /// </returns>
  public static bool HasAttribute(this ISymbol symbol, Type attributeType) {
    return symbol.GetAttributes()
        .Any(a => a.AttributeClass?.IsAssignableTo(attributeType) ?? false);
  }

  /// <summary>
  /// Determines whether the specified <see cref="ISymbol"/> has an attribute
  /// of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the attribute to check for.
  /// </typeparam>
  /// <param name="symbol">
  /// The <see cref="ISymbol"/> to check for the presence of the attribute.
  /// </param>
  /// <returns>
  /// <c>true</c> if the <see cref="ISymbol"/> has an attribute of type <typeparamref name="T"/>; otherwise, <c>false</c>.
  /// </returns>
  public static bool HasAttribute<T>(this ISymbol symbol) where T : Attribute {
    return symbol.HasAttribute(typeof(T));
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
      if (constructorType == typeof(object)) {
        continue;
      }
      
      var constructorArgument = attributeData.ConstructorArguments[i];

      if (!constructorArgument.Type?.IsSameType(constructorType) ?? false) {
        return false;
      }
    }

    return true;
  }

  /// <summary>
  /// Extracts information from the specified <see cref="AttributeData"/> to create an <see cref="AttributeUsageInfo"/> instance.
  /// </summary>
  /// <param name="attributeData">
  /// The <see cref="AttributeData"/> representing an attribute to analyze and extract usage information from.
  /// </param>
  /// <returns>
  /// An <see cref="AttributeUsageInfo"/> instance containing the parsed attribute usage details,
  /// including valid targets, whether the attribute can be applied multiple times, and whether
  /// it is inheritable.
  /// </returns>
  public static AttributeUsageInfo GetUsageInfo(this AttributeData attributeData) {
    return attributeData.AttributeClass?.GetUsageInfo() ?? throw new InvalidOperationException("Symbol is not an attribute");
  }

  /// <summary>
  /// Retrieves the <see cref="AttributeUsageInfo"/> of the specified <see cref="INamedTypeSymbol"/>
  /// if it represents an attribute type.
  /// </summary>
  /// <param name="symbol">
  /// The <see cref="INamedTypeSymbol"/> for which to retrieve the attribute usage information.
  /// </param>
  /// <returns>
  /// The <see cref="AttributeUsageInfo"/> for the specified <see cref="INamedTypeSymbol"/> representing an attribute.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the provided symbol does not represent an attribute or has an invalid attribute usage configuration.
  /// </exception>
  public static AttributeUsageInfo GetUsageInfo(this INamedTypeSymbol symbol) {
    if (!symbol.IsAssignableTo<Attribute>()) {
      throw new InvalidOperationException("Symbol is not an attribute");
    }
    
    var attributeUsage = symbol.GetAttributes()
        .SingleOrDefault(a => a.AttributeClass?.IsAssignableTo<AttributeUsageAttribute>() ?? false);

    if (attributeUsage is null) {
      return new AttributeUsageInfo(AttributeTargets.All);
    }

    if (!attributeUsage.HasMatchingConstructor(typeof(AttributeTargets))) {
      throw new InvalidOperationException("Invalid attribute usage");
    }

    var target = attributeUsage.ConstructorArguments[0].GetTypedValue<AttributeTargets>();

    var namedArguments = attributeUsage.NamedArguments.ToDictionary(x => x.Key, x => x.Value);

    return new AttributeUsageInfo(target) {
        AllowMultiple =
            namedArguments.TryGetValue(nameof(AttributeUsageAttribute.AllowMultiple), out var allowMultiple) &&
            allowMultiple.GetTypedValue<bool>(),
        Inherited = namedArguments.TryGetValue(nameof(AttributeUsageAttribute.Inherited), out var inherited) &&
                    inherited.GetTypedValue<bool>()
    };
  }
}
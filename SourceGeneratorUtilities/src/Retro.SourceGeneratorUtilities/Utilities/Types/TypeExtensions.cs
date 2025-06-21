using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Types;

/// <summary>
/// Provides extension methods for operations related to Roslyn's <see cref="ITypeSymbol"/> and .NET <see cref="Type"/>.
/// </summary>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal static class TypeExtensions {
  private const string TypeIsNull = "Type is null";
  
  private static readonly MethodInfo EnumerableCast = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!;
  private static readonly MethodInfo GenericToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!;
  
  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="targetType">The target .NET type to compare against.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified target type; otherwise, false.
  /// </returns>
  public static bool IsSameType(this ITypeSymbol type, Type targetType) {
    return type switch {
        INamedTypeSymbol namedTypeSymbol => namedTypeSymbol.IsSameType(targetType),
        IArrayTypeSymbol arrayTypeSymbol => arrayTypeSymbol.IsSameType(targetType),
        IPointerTypeSymbol pointerTypeSymbol => pointerTypeSymbol.IsSameType(targetType),
        ITypeParameterSymbol typeParameterSymbol => typeParameterSymbol.IsSameType(targetType),
        _ => throw new NotSupportedException($"Type {type.GetType()} is not supported")
    };
  }

  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to compare.</param>
  /// <param name="targetType">The target .NET type to compare against.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified target type; otherwise, false.
  /// </returns>
  public static bool IsSameType(this INamedTypeSymbol type, Type targetType) {
    if (type.MetadataName != targetType.Name) {
      return false;
    }

    if (type.IsGenericType) {
      if (!targetType.IsGenericType) {
        return false;
      }

      var genericArguments = targetType.GetGenericArguments();
      if (type.TypeArguments.Length != genericArguments.Length) {
        return false;
      }

      for (var i = 0; i < genericArguments.Length; i++) {
        var parameterSymbol = type.TypeArguments[i];
        var targetTypeParameter = genericArguments[i];

        if (!parameterSymbol.IsSameType(targetTypeParameter)) {
          return false;
        }
      }
    }

    if (type.ContainingType is not null) {
      return targetType.IsNested && type.ContainingType.IsSameType(targetType.DeclaringType!);
    }
    
    return targetType.Namespace == type.ContainingNamespace.ToDisplayString();
  }

  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="targetType">The target .NET type to compare against.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified target type; otherwise, false.
  /// </returns>
  public static bool IsSameType(this IArrayTypeSymbol type, Type targetType) {
    return targetType.IsArray && targetType.GetArrayRank() == type.Rank && type.ElementType.IsSameType(targetType.GetElementType()!);
  }

  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="targetType">The target .NET type to compare against.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified target type; otherwise, false.
  /// </returns>
  public static bool IsSameType(this IPointerTypeSymbol type, Type targetType) {
    return targetType.IsPointer && type.PointedAtType.IsSameType(targetType.GetElementType()!);
  }

  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="targetType">The target .NET type to compare against.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified target type; otherwise, false.
  /// </returns>
  public static bool IsSameType(this ITypeParameterSymbol type, Type targetType) {
    return targetType.IsGenericParameter && type.MetadataName == targetType.Name && type.DeclaringType!.IsSameTypeNoGenericCheck(targetType.DeclaringType!);
  }

  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified generic type parameter <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The target .NET type to compare against.</typeparam>
  /// <param name="type">The current type symbol to check.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified generic type parameter <typeparamref name="T"/>; otherwise, false.
  /// </returns>
  public static bool IsSameType<T>(this ITypeSymbol type) {
    return type.IsSameType(typeof(T));
  }
  
  private static bool IsSameTypeNoGenericCheck(this INamedTypeSymbol type, Type targetType) {
    return type.MetadataName == targetType.Name && targetType.Namespace == type.ContainingNamespace.ToDisplayString();
  }


  /// <summary>
  /// Determines whether the specified type is assignable from the current <see cref="ITypeSymbol"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <typeparam name="T">The .NET type to compare against as the target type.</typeparam>
  /// <returns>
  /// True if the specified type is assignable from the current type symbol; otherwise, false.
  /// </returns>
  public static bool IsAssignableTo<T>(this ITypeSymbol type) {
    return type.IsAssignableTo(typeof(T));
  }


  /// <summary>
  /// Determines whether the current <see cref="ITypeSymbol"/> can be assigned from the specified <see cref="Type"/>.
  /// </summary>
  /// <remarks>
  /// In the case of open generics, if the other type is open then it will match on then it will match on the generic
  /// definition regardless of type parameters.
  /// </remarks>
  /// <param name="type">The current type symbol representing the type to check.</param>
  /// <param name="otherType">The target .NET type to determine assignability from.</param>
  /// <returns>
  /// True if the current type symbol can be assigned from the specified target type; otherwise, false.
  /// </returns>
  public static bool IsAssignableTo(this ITypeSymbol type, Type otherType) {
    if (type.IsSameType(otherType)) {
      return true;
    }

    if (type is INamedTypeSymbol { IsGenericType: true } namedType && otherType is { IsGenericType: true} && namedType.ConstructedFrom.IsSameType(otherType.GetGenericTypeDefinition())) {
      return ValidateGenericTypeArguments(otherType, namedType);
    }

    if (otherType.IsClass && type is { TypeKind: TypeKind.Class, BaseType: not null }) {
      return type.BaseType.IsAssignableTo(otherType);
    }

    if (otherType.IsInterface && type.TypeKind is TypeKind.Interface or TypeKind.Class or TypeKind.Struct) {
      return type.AllInterfaces
          .Any(i => i.IsAssignableTo(otherType));
    }

    return false;
  }

  /// <summary>
  /// Checks if the type represented by the current <see cref="ITypeSymbol"/> is of the specified type represented by another <see cref="ITypeSymbol"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="other">The type symbol to compare against.</param>
  /// <returns>
  /// True if the current type symbol is of the specified type; otherwise, false.
  /// </returns>
  public static bool IsAssignableTo(this ITypeSymbol type, ITypeSymbol other) {
    if (SymbolEqualityComparer.Default.Equals(type, other)) {
      return true;
    }

    return other.TypeKind switch {
        TypeKind.Class when type is { TypeKind: TypeKind.Class, BaseType: not null } => type.BaseType.IsAssignableTo(other),
        TypeKind.Interface when type.TypeKind is TypeKind.Interface or TypeKind.Class => type.Interfaces
            .Any(i => i.IsAssignableTo(other)),
        _ => false
    };
  }

  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> can be assigned to the specified generic type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The target type to evaluate for assignment compatibility.</typeparam>
  /// <param name="type">The current type symbol to check.</param>
  /// <returns>
  /// True if the current type symbol can be assigned to the specified target type; otherwise, false.
  /// </returns>
  public static bool IsAssignableFrom<T>(this ITypeSymbol type) {
    return type.IsAssignableFrom(typeof(T));
  }


  /// <summary>
  /// Determines whether the current <see cref="ITypeSymbol"/> can be assigned to the specified .NET <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="otherType">The target .NET type to which assignment is verified.</param>
  /// <returns>
  /// True if the current type symbol can be assigned to the specified target type; otherwise, false.
  /// </returns> 
  /// <remarks>
  /// In the case of open generics, if the other type is open then it will match on then it will match on the generic
  /// definition regardless of type parameters.
  /// </remarks>
  public static bool IsAssignableFrom(this ITypeSymbol type, Type otherType) {
    if (type.IsSameType(otherType)) {
      return true;
    }

    if (type is not INamedTypeSymbol { IsGenericType: true } namedType ||
        otherType is not { IsGenericType: true } ||
        !namedType.ConstructedFrom.IsSameType(otherType.GetGenericTypeDefinition())) {
      return type switch {
          { TypeKind: TypeKind.Class } when otherType is { IsClass: true, BaseType: not null } =>
              type.IsAssignableFrom(otherType.BaseType),
          { TypeKind: TypeKind.Interface } => otherType.GetInterfaces().Any(type.IsAssignableFrom),
          _ => false
      };
    }

    return ValidateGenericTypeArguments(type, otherType, namedType);
  }


  private static bool ValidateGenericTypeArguments(Type otherType, INamedTypeSymbol namedType) {
    var typeArguments = namedType.TypeArguments;
    var otherArguments = otherType.GetGenericArguments();
    var declaringArguments = otherType.GetGenericTypeDefinition().GetGenericArguments();
        
    if (typeArguments.Length != otherArguments.Length || typeArguments.Length != declaringArguments.Length) {
      return false;
    }

    for (var i = 0; i < typeArguments.Length; i++) {
      var typeArgument = typeArguments[i];
      var otherArgument = otherArguments[i];
      var declaringArgument = declaringArguments[i];

      if (otherArgument.IsGenericParameter) {
        continue;
      }
          
      var genericParameterAttributes = declaringArgument.GenericParameterAttributes;
      if ((genericParameterAttributes & GenericParameterAttributes.Covariant) != 0) {
        if (!typeArgument.IsAssignableTo(otherArgument)) {
          return false;
        }
      }
      else if ((genericParameterAttributes & GenericParameterAttributes.Contravariant) != 0) {
        if (!typeArgument.IsAssignableFrom(otherArgument)) {
          return false;
        }
      }
      else if (!typeArgument.IsSameType(otherArgument)) {
        return false;
      }

    }

    return true;
  }

  private static bool ValidateGenericTypeArguments(ITypeSymbol type, Type otherType, INamedTypeSymbol namedType) {
    var typeArguments = otherType.GetGenericArguments();
    var otherArguments = namedType.TypeArguments;
    var declaringArguments = namedType.TypeParameters;
        
    if (typeArguments.Length != otherArguments.Length || typeArguments.Length != declaringArguments.Length) {
      return false;
    }

    for (var i = 0; i < typeArguments.Length; i++) {
      var typeArgument = typeArguments[i];
      var otherArgument = otherArguments[i];
      var declaringArgument = declaringArguments[i];

      if (otherArgument is ITypeParameterSymbol) {
        continue;
      }
      
      return declaringArgument.Variance switch {
          VarianceKind.Out => otherArgument.IsAssignableFrom(typeArgument),
          VarianceKind.In => otherArgument.IsAssignableTo(typeArgument),
          _ => otherArgument.IsSameType(typeArgument)
      };
    }

    return type switch {
        { TypeKind: TypeKind.Class } when otherType is { IsClass: true, BaseType: not null } =>
            type.IsAssignableFrom(otherType.BaseType),
        { TypeKind: TypeKind.Interface } => otherType.GetInterfaces().Any(type.IsAssignableFrom),
        _ => false
    };
  }

  /// <summary>
  /// Retrieves the value of the specified <see cref="TypedConstant"/> and casts it to the specified type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The target type to cast the value to.</typeparam>
  /// <param name="attributeValue">The <see cref="TypedConstant"/> containing the value to retrieve.</param>
  /// <returns>
  /// The value of the <see cref="TypedConstant"/> cast to the specified type <typeparamref name="T"/>.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the value of the <see cref="TypedConstant"/> is null and the specified type <typeparamref name="T"/> is a value type.
  /// </exception>
  public static T GetTypedValue<T>(this TypedConstant attributeValue) {
    return (T) attributeValue.GetTypedValue(typeof(T))!;
  }

  private static object? GetTypedValue(this TypedConstant attributeValue, Type type) {
    if (attributeValue.Kind != TypedConstantKind.Array && attributeValue.Value is null && type.IsValueType) {
      throw new InvalidOperationException(TypeIsNull);
    }

    if (!type.IsArray) return attributeValue.Value;

    var elementType = type.GetElementType()!;
    var instantiatedCastMethod = EnumerableCast.MakeGenericMethod(elementType);
    var instantiatedToArrayMethod = GenericToArray.MakeGenericMethod(elementType);


    var valueLiterals = attributeValue.Values
        .Select(x => x.Value);
    
    var castedValues = instantiatedCastMethod.Invoke(null, [valueLiterals]);
    return instantiatedToArrayMethod.Invoke(null, [castedValues]);

  }

  /// <summary>
  /// Retrieves the <see cref="INamedTypeSymbol"/> associated with the specified .NET <see cref="Type"/> from the provided <see cref="Compilation"/>.
  /// </summary>
  /// <param name="compilation">The Roslyn <see cref="Compilation"/> instance used to search for the type.</param>
  /// <param name="type">The .NET <see cref="Type"/> to find within the compilation.</param>
  /// <returns>
  /// The <see cref="INamedTypeSymbol"/> representing the specified <see cref="Type"/> if found; otherwise, an exception is thrown.
  /// </returns>
  /// <exception cref="InvalidOperationException">Thrown if the provided type or its metadata name is null, or if the type cannot be found in the compilation.</exception>
  public static ITypeSymbol GetType(this Compilation compilation, Type type) {
    if (type.IsArray) {
      return compilation.CreateArrayTypeSymbol(compilation.GetType(type.GetElementType()!), type.GetArrayRank());
    }

    if (type.IsPointer) {
      return compilation.CreatePointerTypeSymbol(compilation.GetType(type.GetElementType()!));
    }

    if (type.IsGenericParameter) {
      var owningType = compilation.GetTypeByMetadataName(type.DeclaringType!.FullName!);
      if (owningType is null) {
        throw new InvalidOperationException(TypeIsNull);
      }
      
      return owningType.TypeParameters[type.GenericParameterPosition];
    }

    if (type.IsConstructedGenericType) {
      var genericDefinition = type.GetGenericTypeDefinition()!;
      var unboundType = (INamedTypeSymbol) compilation.GetType(genericDefinition);
      var typeArgs = type.GetGenericArguments()
          .Select((a, i) => a.IsGenericParameter ? unboundType.TypeArguments[i] : compilation.GetType(a))
          .ToArray();
      return unboundType.Construct(typeArgs);
    }
    
    var metadataName = type.FullName;
    if (metadataName is null) {
      throw new InvalidOperationException(TypeIsNull);
    }

    var symbol = compilation.GetTypeByMetadataName(metadataName);
    if (symbol is null) {
      throw new InvalidOperationException(TypeIsNull);
    }

    return symbol;
  }

  /// <summary>
  /// Retrieves the <see cref="INamedTypeSymbol"/> that corresponds to the specified .NET <see cref="Type"/> within the provided <see cref="Compilation"/>.
  /// </summary>
  /// <param name="compilation">The compilation context to search for the specified type.</param>
  /// <typeparam name="T">The .NET type for which to retrieve the corresponding <see cref="INamedTypeSymbol"/>.</typeparam>
  /// <returns>
  /// The <see cref="INamedTypeSymbol"/> that corresponds to the specified .NET type, or null if no matching symbol is found.
  /// </returns>
  public static ITypeSymbol GetType<T>(this Compilation compilation) {
    return compilation.GetType(typeof(T));
  }

  /// <summary>
  /// Retrieves the specified type and its base types in a sequence, starting from the type itself and traversing up the inheritance hierarchy.
  /// </summary>
  /// <param name="type">The type symbol for which to retrieve the type and its base types.</param>
  /// <returns>
  /// A sequence of <see cref="ITypeSymbol"/> objects representing the specified type and its base types in order of inheritance.
  /// </returns>
  public static IEnumerable<ITypeSymbol> GetBaseTypeAndThis(this ITypeSymbol type) {
    var current = type;
    while (current != null) {
      yield return current;
      current = current.BaseType;
    }
  }

  /// <summary>
  /// Retrieves the name of the specified <see cref="INamedTypeSymbol"/> in a format compatible with C# typeof syntax.
  /// </summary>
  /// <param name="type">
  /// The <see cref="INamedTypeSymbol"/> instance representing the type whose name is to be retrieved.
  /// </param>
  /// <returns>
  /// A string representing the name of the type, formatted to include the correct generic argument placeholders if applicable.
  /// </returns>
  public static string GetTypeofName(this INamedTypeSymbol type) {
    var baseName = type.ToDisplayString(new SymbolDisplayFormat(
                                            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                                            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                                            genericsOptions: SymbolDisplayGenericsOptions.None
                                        ));

    if (!type.IsGenericType) {
      return baseName;
    }

    var genericArguments = string.Concat(Enumerable.Repeat(",", type.TypeArguments.Length - 1));
    return $"{baseName}<{genericArguments}>";
  }
}
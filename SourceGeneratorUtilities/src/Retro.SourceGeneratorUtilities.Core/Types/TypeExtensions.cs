using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Types;

/// <summary>
/// Provides extension methods for operations related to Roslyn's <see cref="ITypeSymbol"/> and .NET <see cref="Type"/>.
/// </summary>
public static class TypeExtensions {
  /// <summary>
  /// Generates a string representation of the specified <see cref="Type"/> that accurately reflects its structure,
  /// including its namespace, generic arguments, or array type if applicable.
  /// </summary>
  /// <param name="type">The type whose display string will be generated.</param>
  /// <returns>
  /// A string representation of the type, including appropriate formatting for generic and array types.
  /// For primitive types, the C# type name is returned (e.g., "int" for <see cref="Int32"/>).
  /// </returns>
  public static string ToDisplayString(this Type type) {
    if (type.TryGetSpecialTypeName(out var specialTypeName)) {
      return specialTypeName;
    }

    if (type.IsArray) {
      return $"{type.GetElementType()!.ToDisplayString()}[]";
    }

    if (!type.IsGenericType) {
      return type.FullName ?? type.Name;
    }

    if (type is not { IsNested: true, DeclaringType: not null }) {
      return $"{type.Namespace}.{type.GetSimpleGenericTypeName()}";
    }

    var declaringType = type.DeclaringType.ToDisplayString();
    var nestedTypeName = type.GetSimpleGenericTypeName();
    return $"{declaringType}.{nestedTypeName}";
  }

  private static string GetSimpleGenericTypeName(this Type type) {
    if (!type.IsGenericType) {
      return type.Name;
    }

    var baseName = type.Name;
    var backtickIndex = baseName.IndexOf('`');
    if (backtickIndex > 0) {
      baseName = baseName.Substring(0, backtickIndex);
    }

    // Build the generic parameters part
    var genericArgs = type.GetGenericArguments();

    // For nested types in generic types, we need to skip the parent's type parameters
    if (type.IsNested && (type.DeclaringType?.IsGenericType ?? false)) {
      var parentTypeParamCount = type.DeclaringType.GetGenericArguments().Length;
      genericArgs = genericArgs.Skip(parentTypeParamCount).ToArray();
    }

    var genericParams = string.Join(",", genericArgs.Select(ToDisplayString));
    return $"{baseName}<{genericParams}>";
  }

  private static bool TryGetSpecialTypeName(this Type type, [NotNullWhen(true)] out string? specialTypeName) {
    specialTypeName = type switch {
        not null when type == typeof(void) => "void",
        not null when type == typeof(bool) => "bool",
        not null when type == typeof(byte) => "byte",
        not null when type == typeof(sbyte) => "sbyte",
        not null when type == typeof(char) => "char",
        not null when type == typeof(decimal) => "decimal",
        not null when type == typeof(double) => "double",
        not null when type == typeof(float) => "float",
        not null when type == typeof(int) => "int",
        not null when type == typeof(uint) => "uint",
        not null when type == typeof(long) => "long",
        not null when type == typeof(ulong) => "ulong",
        not null when type == typeof(short) => "short",
        not null when type == typeof(ushort) => "ushort",
        not null when type == typeof(string) => "string",
        not null when type == typeof(object) => "object",
        _ => null
    };
    return specialTypeName is not null;
  }

  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="targetType">The target .NET type to compare against.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified target type; otherwise, false.
  /// </returns>
  public static bool IsSameType(this ITypeSymbol type, Type targetType) {
    return type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString() == targetType.ToDisplayString();
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

  /// <summary>
  /// Checks if the type represented by the current <see cref="ITypeSymbol"/> is of the specified type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The type to compare against.</typeparam>
  /// <param name="type">The current type symbol to check.</param>
  /// <returns>
  /// True if the current type symbol is of the specified type; otherwise, false.
  /// </returns>
  public static bool IsOfType<T>(this ITypeSymbol type) {
    return type.IsOfType(typeof(T));
  }

  /// <summary>
  /// Determines whether the specified <see cref="ITypeSymbol"/> instance represents the same type as,
  /// inherits from, or implements the specified .NET <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The <see cref="ITypeSymbol"/> to be checked.</param>
  /// <param name="otherType">The target <see cref="Type"/> to compare against.</param>
  /// <returns>
  /// True if the <see cref="ITypeSymbol"/> is of the same type, inherits from, or implements the target <see cref="Type"/>.
  /// Otherwise, false.
  /// </returns>
  public static bool IsOfType(this ITypeSymbol type, Type otherType) {
    if (type.IsSameType(otherType)) {
      return true;
    }

    if (type is INamedTypeSymbol { IsGenericType: true } namedType && otherType is { IsGenericType: true, DeclaringType: not null } && namedType.ConstructedFrom.IsSameType(otherType.DeclaringType)) {
      var typeArguments = namedType.TypeArguments;
      var otherArguments = otherType.GetGenericArguments();
      var declaringArguments = otherType.DeclaringType.GetGenericArguments();
        
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
        var isCovariant = (genericParameterAttributes & GenericParameterAttributes.Covariant) != 0;
        var isContravariant = (genericParameterAttributes & GenericParameterAttributes.Contravariant) != 0;

        if (isCovariant) {
          if (!typeArgument.IsOfType(otherArgument)) {
            return false;
          }
        }
        else if (isContravariant) {
          if (!otherArgument.IsOfType(typeArgument)) {
            return false;
          }
        }
        else if (!typeArgument.IsSameType(otherArgument)) {
          return false;
        }

      }
    }


    if (otherType.IsClass && type is { TypeKind: TypeKind.Class, BaseType: not null }) {
      return type.BaseType.IsOfType(otherType);
    }

    if (otherType.IsInterface && type.TypeKind is TypeKind.Interface or TypeKind.Class or TypeKind.Struct) {
      return type.AllInterfaces
          .Any(i => i.IsOfType(otherType));
    }

    return false;
  }

  /// <summary>
  /// Determines if the specified <see cref="Type"/> instance is of the same type as or assignable to the provided <see cref="ITypeSymbol"/>.
  /// </summary>
  /// <param name="type">The <see cref="Type"/> instance to check.</param>
  /// <param name="otherType">The <see cref="ITypeSymbol"/> to compare against.</param>
  /// <returns>
  /// A boolean value indicating whether the <see cref="Type"/> is of the same type as the <see cref="ITypeSymbol"/>
  /// or assignable to it, considering generic type compatibility and inheritance hierarchy.
  /// </returns>
  public static bool IsOfType(this Type type, ITypeSymbol otherType) {
    if (otherType.IsSameType(type)) {
      return true;
    }

    if (otherType is not INamedTypeSymbol { IsGenericType: true } namedType ||
        type is not { IsGenericType: true, DeclaringType: not null } ||
        !namedType.ConstructedFrom.IsSameType(type.DeclaringType)) {
      return otherType switch {
          { TypeKind: TypeKind.Class } when type is { IsClass: true, BaseType: not null } =>
              type.BaseType.IsOfType(otherType),
          { TypeKind: TypeKind.Interface } => type.GetInterfaces().Any(i => i.IsOfType(otherType)),
          _ => false
      };
    }

    var typeArguments = type.GetGenericArguments();
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
        
      var isCovariant = declaringArgument.Variance == VarianceKind.Out;
      var isContravariant = declaringArgument.Variance == VarianceKind.In;

      if (isCovariant) {
        if (!typeArgument.IsOfType(otherArgument)) {
          return false;
        }
      }
      else if (isContravariant) {
        if (!otherArgument.IsOfType(typeArgument)) {
          return false;
        }
      }
      else if (!otherArgument.IsSameType(typeArgument)) {
        return false;
      }

    }

    return otherType switch {
        { TypeKind: TypeKind.Class } when type is { IsClass: true, BaseType: not null } =>
            type.BaseType.IsOfType(otherType),
        { TypeKind: TypeKind.Interface } => type.GetInterfaces().Any(i => i.IsOfType(otherType)),
        _ => false
    };
  }

  /// <summary>
  /// Checks if the type represented by the current <see cref="ITypeSymbol"/> is of the specified type represented by another <see cref="ITypeSymbol"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="other">The type symbol to compare against.</param>
  /// <returns>
  /// True if the current type symbol is of the specified type; otherwise, false.
  /// </returns>
  public static bool IsOfType(this ITypeSymbol type, ITypeSymbol other) {
    if (SymbolEqualityComparer.Default.Equals(type, other)) {
      return true;
    }

    return other.TypeKind switch {
        TypeKind.Class when type is { TypeKind: TypeKind.Class, BaseType: not null } => type.BaseType.IsOfType(other),
        TypeKind.Interface when type.TypeKind is TypeKind.Interface or TypeKind.Class => type.Interfaces
            .Any(i => i.IsOfType(other)),
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
    if (attributeValue.Value is null && typeof(T).IsValueType) {
      throw new InvalidOperationException("Type is null");
    }

    return (T)attributeValue.Value!;
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
  public static INamedTypeSymbol GetNamedType(this Compilation compilation, Type type) {
    var metadataName = type.FullName;
    if (metadataName is null) {
      throw new InvalidOperationException("Type is null");
    }

    var symbol = compilation.GetTypeByMetadataName(metadataName);
    if (symbol is null) {
      throw new InvalidOperationException("Type is null");
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
  public static INamedTypeSymbol GetNamedType<T>(this Compilation compilation) {
    return compilation.GetNamedType(typeof(T));
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
}
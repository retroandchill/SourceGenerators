using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Retro.SourceGeneratorUtilities.Core.Types;

/// <summary>
/// Provides extension methods for operations related to Roslyn's <see cref="ITypeSymbol"/> and .NET <see cref="Type"/>.
/// </summary>
public static class TypeExtensions {
  /// <summary>
  /// Determines if the current <see cref="ITypeSymbol"/> represents the same type as the specified <see cref="Type"/>.
  /// </summary>
  /// <param name="type">The current type symbol to check.</param>
  /// <param name="targetType">The target .NET type to compare against.</param>
  /// <returns>
  /// True if the current type symbol represents the same type as the specified target type; otherwise, false.
  /// </returns>
  public static bool IsSameType(this ITypeSymbol type, Type targetType) {
    if (type is ITypeParameterSymbol && targetType.IsGenericParameter) {
      return true;
    }
    
    if (targetType == typeof(void)) return type.SpecialType == SpecialType.System_Void;
    if (targetType == typeof(bool)) return type.SpecialType == SpecialType.System_Boolean;
    if (targetType == typeof(char)) return type.SpecialType == SpecialType.System_Char;
    if (targetType == typeof(sbyte)) return type.SpecialType == SpecialType.System_SByte;
    if (targetType == typeof(byte)) return type.SpecialType == SpecialType.System_Byte;
    if (targetType == typeof(short)) return type.SpecialType == SpecialType.System_Int16;
    if (targetType == typeof(ushort)) return type.SpecialType == SpecialType.System_UInt16;
    if (targetType == typeof(int)) return type.SpecialType == SpecialType.System_Int32;
    if (targetType == typeof(uint)) return type.SpecialType == SpecialType.System_UInt32;
    if (targetType == typeof(long)) return type.SpecialType == SpecialType.System_Int64;
    if (targetType == typeof(ulong)) return type.SpecialType == SpecialType.System_UInt64;
    if (targetType == typeof(float)) return type.SpecialType == SpecialType.System_Single;
    if (targetType == typeof(double)) return type.SpecialType == SpecialType.System_Double;
    if (targetType == typeof(decimal)) return type.SpecialType == SpecialType.System_Decimal;
    if (targetType == typeof(string)) return type.SpecialType == SpecialType.System_String;
    if (targetType == typeof(object)) return type.SpecialType == SpecialType.System_Object;

    // Handle generic types
    if (type is not INamedTypeSymbol namedType || !targetType.IsGenericType) return type.ToString() == targetType.FullName;
    
    if (!namedType.IsGenericType)
      return false;

    // Check if the generic type definitions match
    if ($"{namedType.ContainingSymbol}.{namedType.ConstructedFrom.MetadataName}" != targetType.GetGenericTypeDefinition().FullName) {
      return false;
    }

    // Check if the number of type arguments matches
    var genericArguments = targetType.GetGenericArguments();
    if (namedType.TypeArguments.Length != genericArguments.Length) {
      return false;
    }

    // Compare each type argument
    for (var i = 0; i < namedType.TypeArguments.Length; i++) {
      var typeArg = namedType.TypeArguments[i];
      var targetTypeArg = genericArguments[i];
      if (!typeArg.IsSameType(targetTypeArg)) {
        return false;
      }
    }

    return true;


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
    if (type.IsSameType<T>()) {
      return true;
    }

    if (typeof(T).IsClass && type is { TypeKind: TypeKind.Class, BaseType: not null }) {
      return type.BaseType.IsOfType<T>();
    }

    if (typeof(T).IsInterface && type.TypeKind is TypeKind.Interface or TypeKind.Class) {
      return type.Interfaces
          .Any(i => i.IsOfType<T>());
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

}
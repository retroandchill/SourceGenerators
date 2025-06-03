using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Types;

public static class TypeExtensions {
  public static bool IsSameType(this ITypeSymbol type, Type targetType) {
    return type.ToString() == targetType.FullName;
  }

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
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Model;

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

  public static TypedConstantKind GetTypedConstantKind(this ITypeSymbol type) {
    if (type.IsSameType<Type>()) {
      return TypedConstantKind.Type;
    }

    return type.TypeKind switch {
        TypeKind.Enum => TypedConstantKind.Enum,
        TypeKind.Array => TypedConstantKind.Array,
        _ => type.SpecialType switch {
            SpecialType.System_Boolean or SpecialType.System_Char or SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
                or SpecialType.System_Single or SpecialType.System_Double => TypedConstantKind.Primitive,
            _ => TypedConstantKind.Error
        }
    };
  }

  public static T GetTypedValue<T>(this TypedConstant attributeValue) {
    if (attributeValue.Value is null) {
      throw new InvalidOperationException("Type is null");
    }
    
    return (T) attributeValue.Value;
  }

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

  public static INamedTypeSymbol GetNamedType<T>(this Compilation compilation) {
    return compilation.GetNamedType(typeof(T));
  }

  public static ImmutableDictionary<INamedTypeSymbol, TypePropertyInitializationOverview> GetPropertyInitializations(this IEnumerable<INamedTypeSymbol> types) {
    var exploreSet = new Dictionary<INamedTypeSymbol, TypePropertyInitializationOverview>(NamedTypeSymbolEqualityComparer.Default);
    foreach (var type in types) {
      GetPropertyInitialization(type, exploreSet);
    }

    return exploreSet.ToImmutableDictionary(NamedTypeSymbolEqualityComparer.Default);
  }

  private static TypePropertyInitializationOverview GetPropertyInitialization(this INamedTypeSymbol type, Dictionary<INamedTypeSymbol, TypePropertyInitializationOverview> exploreSet) {
    if (exploreSet.TryGetValue(type, out var overview)) {
      return overview;
    }
    
    var baseType = type.BaseType?.GetPropertyInitialization(exploreSet);
    var newOverview = new TypePropertyInitializationOverview {
        Base = baseType
    };
    exploreSet.Add(type, newOverview);
    return newOverview;
  }
}
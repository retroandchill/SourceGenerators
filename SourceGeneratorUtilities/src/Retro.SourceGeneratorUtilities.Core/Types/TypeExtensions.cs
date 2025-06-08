using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Members;
using Retro.SourceGeneratorUtilities.Core.Model;

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
    
    return type.ToString() == targetType.FullName;
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
    
    return (T) attributeValue.Value!;
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
  /// Generates an immutable dictionary of data class overviews for the specified collection of <see cref="INamedTypeSymbol"/> instances.
  /// </summary>
  /// <param name="types">The collection of <see cref="INamedTypeSymbol"/> instances to generate data class overviews for.</param>
  /// <returns>
  /// An immutable dictionary mapping each <see cref="INamedTypeSymbol"/> to its corresponding <see cref="DataTypeOverview"/>.
  /// </returns>
  public static ImmutableDictionary<INamedTypeSymbol, DataTypeOverview> GetDataClassOverviews(this IEnumerable<INamedTypeSymbol> types) {
    var exploreSet = new Dictionary<INamedTypeSymbol, DataTypeOverview>(NamedTypeSymbolEqualityComparer.Default);
    foreach (var type in types) {
      GetDataClassOverview(type, exploreSet);
    }

    return exploreSet.ToImmutableDictionary(NamedTypeSymbolEqualityComparer.Default);
  }

  private static DataTypeOverview? GetDataClassOverview(this INamedTypeSymbol type, Dictionary<INamedTypeSymbol, DataTypeOverview> exploreSet) {
    if (type.SpecialType != SpecialType.None || type.IsSameType<Attribute>()) {
      return null;
    }
    
    if (exploreSet.TryGetValue(type, out var overview)) {
      return overview;
    }
    
    var baseType = type.BaseType?.GetDataClassOverview(exploreSet);

    var publicProperties = type.GetProperties()
        .Where(x => x.Accessibility == AccessibilityLevel.Public)
        .ToImmutableList();

    var constructors = type.GetAllConstructors(baseType, publicProperties)
        .ToImmutableList();
    
    var newOverview = new DataTypeOverview {
        Symbol = type,
        Base = baseType,
        Constructors = constructors,
        Properties = publicProperties
    };
    exploreSet.Add(type, newOverview);
    return newOverview;
  }

  private static IEnumerable<ConstructorOverview> GetAllConstructors(this INamedTypeSymbol type, DataTypeOverview? baseType, ImmutableList<PropertyOverview> properties) {
    var constructorsList = type.GetAllConstructors().ToImmutableList();

    var allConstructors = constructorsList.ToList();
    var exploreSet = new Dictionary<IMethodSymbol, ConstructorOverview>(MethodSymbolEqualityComparer.Default);
    if (baseType is not null) {
      foreach (var constructor in baseType.Constructors) {
        exploreSet.Add(constructor.Symbol, constructor);
        allConstructors.Add(constructor);
      }
    }
    
    var propertiesList = properties.ToList();
    var currentBase = baseType;
    while (currentBase is not null) {
      propertiesList.AddRange(currentBase.Properties);
      currentBase = currentBase.Base;
    }

    return constructorsList.Select(constructor => constructor.GetConstructorOverview(allConstructors, exploreSet, propertiesList));
  }

  private static ConstructorOverview GetConstructorOverview(this ConstructorOverview symbol, IReadOnlyList<ConstructorOverview> allConstructors,
                                                            Dictionary<IMethodSymbol, ConstructorOverview> exploreSet, 
                                                            IReadOnlyList<PropertyOverview> properties) {
    if (exploreSet.TryGetValue(symbol.Symbol, out var thisConstructor)) {
      return thisConstructor;
    }
    
    var calledConstructor = symbol.Initializer is not null ? allConstructors.FirstOrDefault(x => x.Symbol.Equals(symbol.Initializer.Symbol, SymbolEqualityComparer.Default)) : null;
    
    var properConstructor = calledConstructor?.GetConstructorOverview(allConstructors, exploreSet, properties);

    var parameterAssignmentOverrides = properConstructor?.Parameters
        .Select(x => {
          if (!properConstructor.IsPrimaryConstructor)
            return new {
                Parameter = x,
                Assignment = properConstructor.Assignments
                    .FirstOrDefault(y => y.Right is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == x.Name)
            };

          var assignment = properties
              .FirstOrDefault(y => y.Initializer is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == x.Name);
            
          return new {
              Parameter = x,
              Assignment = assignment is not null ? new AssignmentOverview(assignment.Symbol, assignment.Initializer!) : null
          }!;

        })
        .ToImmutableList();

    var constructorAssignments = properConstructor?.Assignments
        .Select(x => {
          var parameterOverride = parameterAssignmentOverrides!
              .Select((y, i) => {
                if (y.Assignment is null || !y.Assignment.Left.Equals(x.Left, SymbolEqualityComparer.Default)) {
                  return null;
                }

                var arguments = symbol.Initializer!.Arguments;
                return i >= arguments.Count ? y.Parameter.DefaultValue : arguments[i].Expression.Expression;

              })
              .FirstOrDefault(y => y is not null);

          if (parameterOverride is null) {
            return x;
          }

          return x with { Right = parameterOverride };
        })
        .ToImmutableList() ?? ImmutableList<AssignmentOverview>.Empty;
    
    var assignmentList = new List<AssignmentOverview>();
    assignmentList.AddRange(constructorAssignments);
    foreach (var assignment in symbol.Assignments) {
      var existingAssignment = assignmentList.FindIndex(x => x.Left.Equals(assignment.Left, SymbolEqualityComparer.Default));
      if (existingAssignment >= 0) {
        assignmentList[existingAssignment] = assignment;
      }
      else {
        assignmentList.Add(assignment);
      }
    }
    
    var newSymbol = symbol with {
        Assignments = assignmentList
            .ConcatPropertyAssignments(properties)
            .ToImmutableList()
    };
    exploreSet.Add(symbol.Symbol, newSymbol);
    return newSymbol;
  }
  
  private static IEnumerable<AssignmentOverview> ConcatPropertyAssignments(this IReadOnlyList<AssignmentOverview> assignments, IEnumerable<PropertyOverview> properties) {
    return assignments.Concat(properties
                                  .Where(y => !assignments
                                             .Any(z => z.Left.Equals(y.Symbol, SymbolEqualityComparer.Default)))
                                  .Select(y => new AssignmentOverview(y.Symbol, y.Initializer ?? SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))));
  }
}
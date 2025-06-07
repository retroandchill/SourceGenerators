using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Members;
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

  public static ImmutableDictionary<INamedTypeSymbol, DataTypeOverview> GetPropertyInitializations(this IEnumerable<INamedTypeSymbol> types) {
    var exploreSet = new Dictionary<INamedTypeSymbol, DataTypeOverview>(NamedTypeSymbolEqualityComparer.Default);
    foreach (var type in types) {
      GetPropertyInitialization(type, exploreSet);
    }

    return exploreSet.ToImmutableDictionary(NamedTypeSymbolEqualityComparer.Default);
  }

  private static DataTypeOverview? GetPropertyInitialization(this INamedTypeSymbol type, Dictionary<INamedTypeSymbol, DataTypeOverview> exploreSet) {
    if (type.SpecialType != SpecialType.None || type.IsSameType<Attribute>()) {
      return null;
    }
    
    if (exploreSet.TryGetValue(type, out var overview)) {
      return overview;
    }
    
    var baseType = type.BaseType?.GetPropertyInitialization(exploreSet);

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
          };

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
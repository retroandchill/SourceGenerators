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
/// Provides extension methods for working with data types and generating overviews of their structure and metadata.
/// </summary>
public static class DataTypeExtensions {
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

    return constructorsList.Select<ConstructorOverview, ConstructorOverview>(constructor => constructor.GetConstructorOverview(allConstructors, exploreSet, propertiesList));
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
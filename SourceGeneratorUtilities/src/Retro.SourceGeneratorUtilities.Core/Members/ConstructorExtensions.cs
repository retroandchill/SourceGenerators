using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

/// <summary>
/// Provides extension methods for retrieving and processing constructor information.
/// </summary>
public static class ConstructorExtensions {

  /// <summary>
  /// Retrieves all constructors of the specified named type symbol as a collection of <see cref="ConstructorOverview"/> objects.
  /// </summary>
  /// <param name="type">The <see cref="INamedTypeSymbol"/> representing the type whose constructors are to be retrieved.</param>
  /// <returns>A collection of <see cref="ConstructorOverview"/> representing all constructors of the specified type.</returns>
  public static IEnumerable<ConstructorOverview> GetAllConstructors(this INamedTypeSymbol type) {
    return type.Constructors
        .Select(x => x.GetConstructorOverview())
        .Where(x => x is not null)!;
  }

  /// <summary>
  /// Retrieves an overview of the specified method symbol if it is a constructor.
  /// </summary>
  /// <param name="symbol">The <see cref="IMethodSymbol"/> representing the method to evaluate and retrieve the overview for.</param>
  /// <returns>A <see cref="ConstructorOverview"/> representing the constructor's metadata, or null if the symbol is not a constructor.</returns>
  public static ConstructorOverview? GetConstructorOverview(this IMethodSymbol symbol) {
    if (symbol.MethodKind != MethodKind.Constructor) {
      return null;
    }

    if (symbol.IsImplicitlyDeclared) {
      return new ConstructorOverview(symbol, []);
    }
    
    var syntaxType = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
    return syntaxType switch {
        TypeDeclarationSyntax { ParameterList: not null } typeDeclaration => symbol.GetConstructorOverview(typeDeclaration),
        ConstructorDeclarationSyntax methodDeclaration => symbol.GetConstructorOverview(methodDeclaration),
        _ => null
    };
  }

  /// <summary>
  /// Retrieves the parameters of the specified method symbol as an immutable array of <see cref="ParameterOverview"/> objects.
  /// </summary>
  /// <param name="symbol">The <see cref="IMethodSymbol"/> representing the method whose parameters are to be retrieved.</param>
  /// <returns>An immutable array of <see cref="ParameterOverview"/> representing the parameters of the specified method.</returns>
  public static ImmutableArray<ParameterOverview> GetParameters(this IMethodSymbol symbol) {
    return [
        ..symbol.Parameters
            .Select((x, i) => x.GetParameterOverview(i, i == symbol.Parameters.Length - 1))
    ];
  }

  /// <summary>
  /// Creates a <see cref="ParameterOverview"/> object for the specified parameter symbol,
  /// capturing details such as the parameter's type, name, default value, position, and whether it is the last parameter.
  /// </summary>
  /// <param name="symbol">The <see cref="IParameterSymbol"/> representing the parameter to process.</param>
  /// <param name="index">The zero-based index of the parameter within the method or constructor signature.</param>
  /// <param name="isLast">A boolean indicating whether the parameter is the last one in the signature.</param>
  /// <returns>A <see cref="ParameterOverview"/> object containing metadata about the specified parameter.</returns>
  public static ParameterOverview GetParameterOverview(this IParameterSymbol symbol, int index = 0, bool isLast = false) {
    return new ParameterOverview(symbol.Type, symbol.Name) {
        DefaultValue = symbol.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<ParameterSyntax>()
            .Select(x => x.Default)
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .FirstOrDefault(),
        Index = index,
        IsLast = isLast
    };
  }

  private static ConstructorOverview GetConstructorOverview(this IMethodSymbol symbol, TypeDeclarationSyntax typeDeclaration) {
    var syntaxTree = typeDeclaration.SyntaxTree;
    var compilation = ((ISourceAssemblySymbol)symbol.ContainingAssembly).Compilation;
    var semanticModel = compilation.GetSemanticModel(syntaxTree);

    var baseType = typeDeclaration.BaseList?.Types
        .OfType<PrimaryConstructorBaseTypeSyntax>()
        .FirstOrDefault();

    // Get the IMethodSymbol for the base constructor being called
    var constructorSymbol = baseType != null 
        ? semanticModel.GetSymbolInfo(baseType).Symbol as IMethodSymbol 
        : null;

    return new ConstructorOverview(symbol, symbol.GetParameters()) {
        IsPrimaryConstructor = true,
        Initializer = baseType != null && constructorSymbol is not null
            ? new ConstructorInitializerOverview(constructorSymbol, InitializerType.Base, baseType.ArgumentList.Arguments
                    .Select((y, i) => new ArgumentOverview(y) {
                        IsLast = i == baseType.ArgumentList.Arguments.Count - 1
                    })
                                                     .ToImmutableList())
            : null
    };
  }

  private static ConstructorOverview GetConstructorOverview(this IMethodSymbol symbol, ConstructorDeclarationSyntax methodDeclaration) {
    var initializer = methodDeclaration.Initializer;
    var initializerType = initializer?.ThisOrBaseKeyword.ToString() == "base"
        ? InitializerType.Base
        : InitializerType.This;
    
    var syntaxTree = methodDeclaration.SyntaxTree;
    var compilation = ((ISourceAssemblySymbol)symbol.ContainingAssembly).Compilation;
    var semanticModel = compilation.GetSemanticModel(syntaxTree);

    var constructorSymbol = initializer != null 
        ? semanticModel.GetSymbolInfo(initializer).Symbol as IMethodSymbol 
        : null;

    
    
    return new ConstructorOverview(symbol, symbol.GetParameters()) {
        Initializer = initializer is not null && constructorSymbol is not null ? new ConstructorInitializerOverview(constructorSymbol, initializerType, initializer.ArgumentList.Arguments
                                                                                                                        .Select((x, i) => new ArgumentOverview(x) {
                                                                                                                            IsLast = i == initializer.ArgumentList.Arguments.Count - 1
                                                                                                                        })
                                                                                                                        .ToImmutableList()) : null,
        Assignments = methodDeclaration.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(assignment => assignment.Left is MemberAccessExpressionSyntax)
            .Select(x => {
                  var memberAccess = (MemberAccessExpressionSyntax)x.Left;
                  var propertySymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol as IPropertySymbol;
                  return new {Left = propertySymbol, x.Right};
                }
            )
            .Where(x => x.Left is not null)
            .Select(x => new AssignmentOverview(x.Left!, x.Right))
            .ToImmutableList()
    };
  }

}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

public static class MethodExtensions {

  public static IEnumerable<ConstructorOverview> GetAllConstructors(this INamedTypeSymbol type) {
    return type.Constructors
        .Select(x => x.GetConstructorOverview())
        .Where(x => x is not null)!;
  }

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

  public static ImmutableArray<ParameterOverview> GetParameters(this IMethodSymbol symbol) {
    return [
        ..symbol.Parameters
            .Select((x, i) => x.GetParameterOverview(i, i == symbol.Parameters.Length - 1))
    ];
  }

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
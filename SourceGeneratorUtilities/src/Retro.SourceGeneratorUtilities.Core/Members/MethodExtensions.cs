using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

public static class MethodExtensions {

  public static ConstructorsOverview GetAllConstructors(this INamedTypeSymbol type) {
    var arrayBuilder = ImmutableArray.CreateBuilder<ConstructorOverview>();
    ConstructorOverview? primaryConstructor = null;
    foreach (var method in type.Constructors
                 .Select(x => x.GetConstructorOverview())) {
      if (method is null) continue;

      if (method.IsPrimaryConstructor) {
        primaryConstructor = method;
        continue;
      }
      
      arrayBuilder.Add(method);
    }
    
    return new ConstructorsOverview(primaryConstructor, arrayBuilder.ToImmutable());
  }

  public static ConstructorOverview? GetConstructorOverview(this IMethodSymbol symbol) {
    if (symbol.MethodKind != MethodKind.Constructor) {
      return null;
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
    return new ConstructorOverview(symbol.GetParameters()) {
        IsPrimaryConstructor = true,
        Initializer = typeDeclaration.BaseList?.Types
            .OfType<PrimaryConstructorBaseTypeSyntax>()
            .Select(x => new ConstructorInitializerOverview(InitializerType.Base, [
                ..x.ArgumentList.Arguments
                    .Select((y, i) => new ArgumentOverview(y) {
                        IsLast = i == x.ArgumentList.Arguments.Count - 1
                    })
            ]))
            .FirstOrDefault()
    };
  }

  private static ConstructorOverview? GetConstructorOverview(this IMethodSymbol symbol, ConstructorDeclarationSyntax methodDeclaration) {
    var initializer = methodDeclaration.Initializer;
    var initializerType = initializer?.ThisOrBaseKeyword.ToString() == "base"
        ? InitializerType.Base
        : InitializerType.This;
    
    return new ConstructorOverview(symbol.GetParameters()) {
        Initializer = initializer is not null ? new ConstructorInitializerOverview(initializerType, [
            ..initializer.ArgumentList.Arguments
                .Select((x, i) => new ArgumentOverview(x) {
                    IsLast = i == initializer.ArgumentList.Arguments.Count - 1
                })
        ]) : null,
        Assignments = [
            ..methodDeclaration.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignment => assignment.Left is MemberAccessExpressionSyntax)
                .Select(x => new AssignmentOverview(x.Left, x.Right))
        ]
    };
  }
  
  public static IEnumerable<AssignmentExpressionSyntax> GetPropertyAssignments(this IMethodSymbol method) {
    var syntaxType = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
    if (syntaxType is ClassDeclarationSyntax { ParameterList: not null } classDeclaration) {
      // Handle primary constructor assignments
      var propertyAssignments = classDeclaration.Members
          .OfType<PropertyDeclarationSyntax>()
          .Where(p => p.Initializer != null)
          .Select(p => SyntaxFactory.AssignmentExpression(
                      SyntaxKind.SimpleAssignmentExpression,
                      SyntaxFactory.MemberAccessExpression(
                          SyntaxKind.SimpleMemberAccessExpression,
                          SyntaxFactory.ThisExpression(),
                          SyntaxFactory.IdentifierName(p.Identifier.Text)),
                      p.Initializer.Value));

      foreach (var assignment in propertyAssignments) {
        yield return assignment;
      }
    }

    // Handle regular constructor or method body assignments
    if (syntaxType is not MethodDeclarationSyntax methodSyntax) yield break;
    var bodyAssignments = methodSyntax.DescendantNodes()
        .OfType<AssignmentExpressionSyntax>()
        .Where(assignment => assignment.Left is MemberAccessExpressionSyntax);

    foreach (var assignment in bodyAssignments) {
      yield return assignment;
    }
  }

  public static  ConstructorInitializerOverview? GetConstructorInitializer(this IMethodSymbol constructor) {
    var syntax = constructor.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

    if (syntax is ClassDeclarationSyntax classDeclarationSyntax) {
      var baseInvocation = classDeclarationSyntax.BaseList?.Types
          .OfType<PrimaryConstructorBaseTypeSyntax>()
          .FirstOrDefault();
      
      return new ConstructorInitializerOverview(InitializerType.Base, baseInvocation?.ArgumentList.Arguments
                                                    .Select(x => new ArgumentOverview(x))
                                                    .ToImmutableArray() ?? []);
    }
    
    if (syntax is not ConstructorDeclarationSyntax constructorSyntax) {
      return null;
    }

    var initializer = constructorSyntax.Initializer;
    var initializerType = initializer?.ThisOrBaseKeyword.ToString() == "base"
        ? InitializerType.Base
        : InitializerType.This;
    return new ConstructorInitializerOverview(initializerType, initializer?.ArgumentList.Arguments
                                                  .Select((x, i) => new ArgumentOverview(x) {
                                                      IsLast = i == initializer.ArgumentList.Arguments.Count - 1
                                                  })
                                                  .ToImmutableArray() ?? []);
  }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

public static class MethodExtensions {
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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Attributes;
using Retro.SourceGeneratorUtilities.Core.Members;
using Retro.SourceGeneratorUtilities.Core.Model;
using Retro.SourceGeneratorUtilities.Core.Types;
using Retro.SourceGeneratorUtilities.Formatters;
using Retro.SourceGeneratorUtilties.Generator.Properties;

namespace Retro.SourceGeneratorUtilities.Generators;

[Generator]
public class AttributeInfoGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var attributeTypes = context.SyntaxProvider
        .CreateSyntaxProvider((s, _) => s is GenericNameSyntax { Identifier.ValueText: "GetInfo" },
                              GetMethodInvocations)
        .SelectMany((m, _) => ExtractAttributeTypes(m))
        .Collect();

    var attributesWithCompilation = context.CompilationProvider
        .Combine(attributeTypes);

    context.RegisterSourceOutput(attributesWithCompilation, (spc, source) => {
      Execute(source.Left, source.Right.Distinct(NamedTypeSymbolEqualityComparer.Default), spc);
    });
  }

  private static IMethodSymbol? GetMethodInvocations(GeneratorSyntaxContext context, CancellationToken token) {
    var genericName = (GenericNameSyntax)context.Node;
    var semanticModel = context.SemanticModel;

    // Get the symbol for the generic name
    var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, genericName);
    if (symbolInfo.Symbol is not IMethodSymbol { IsGenericMethod: true, TypeParameters.Length: 1 } method) {
      return null;
    }


    if (!method.ContainingType.IsSameType(typeof(AttributeExtensions)) ||
        method.Name != nameof(AttributeExtensions.GetInfo)) {
      return null;
    }

    return method;
  }

  private static IEnumerable<INamedTypeSymbol> ExtractAttributeTypes(IMethodSymbol? method) {
    var typeArgument = method?.TypeArguments[0];
    if (typeArgument is not INamedTypeSymbol namedType) {
      yield break;
    }

    while (!namedType.IsSameType<Attribute>()) {
      yield return namedType;
      if (namedType.BaseType is null) {
        break;
      }

      namedType = namedType.BaseType;
    }
  }

  private static void Execute(Compilation compilation, IEnumerable<INamedTypeSymbol> classSymbols, SourceProductionContext context) {
    var allClassSymbols = classSymbols.GetDataClassOverviews();
    foreach (var initializer in allClassSymbols) {
      var classSymbol = initializer.Value;
      var templateParams = new {
          Namespace = classSymbol.Namespace.ToDisplayString(),
          AttributeName = classSymbol.Name,
          HasParentClass = classSymbol.Base is not null && !classSymbol.Base.Symbol.IsSameType<Attribute>(),
          ParentAttribute = classSymbol.Base?.Symbol.ToDisplayString(),
          Constructors = classSymbol.Constructors
              .Select(x => ConvertToTypeMetadata(x, compilation))
              .ToImmutableList(),
          Properties = classSymbol.Properties
              .Select(x => ConvertToTypeMetadata(x, compilation))
              .ToImmutableList()
      };

      var handlebars = Handlebars.Create();
      handlebars.Configuration.TextEncoder = null;
      handlebars.Configuration.FormatterProviders.Add(new EnumStringValueFormatter());

      var template = handlebars.Compile(SourceTemplates.AttributeInfoTemplate);

      var templateResult = template(templateParams);
      context.AddSource($"{classSymbol.Name}Info.g.cs", templateResult);
    }
  }

  private static ConstructorOverview ConvertToTypeMetadata(ConstructorOverview constructorOverview, Compilation compilation) {
    return constructorOverview with {
        Assignments = constructorOverview.Assignments
            .Select(x => x.PropertyType.IsSameType<Type>() ? x with {
                PropertyType = compilation.GetNamedType<ITypeSymbol>(),
                Right = x.Right is TypeOfExpressionSyntax typeOfExpression ? ConvertToCompilationFetchExpression(typeOfExpression, compilation) : x.Right
            } : x)
            .ToImmutableList()
    };
  }
  
  private static PropertyOverview ConvertToTypeMetadata(PropertyOverview propertyOverview, Compilation compilation) {
    return propertyOverview with {
        Type = propertyOverview.Type.IsSameType<Type>() ? compilation.GetNamedType<ITypeSymbol>() : propertyOverview.Type,
    };
  }

  private static ExpressionSyntax ConvertToCompilationFetchExpression(TypeOfExpressionSyntax expression, Compilation compilation) {
    var typeArg = expression.Type;
    

    var typeSymbol = ModelExtensions.GetTypeInfo(compilation.GetSemanticModel(typeArg.SyntaxTree), typeArg).Type;
    if (typeSymbol is null) {
      throw new InvalidOperationException("Type is null");
    }

    return SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("compilation"),
            SyntaxFactory.GenericName(
                SyntaxFactory.IdentifierName(nameof(TypeExtensions.GetNamedType)).Identifier,
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                   SyntaxFactory.IdentifierName(typeSymbol.ToDisplayString()))))));

  }
}
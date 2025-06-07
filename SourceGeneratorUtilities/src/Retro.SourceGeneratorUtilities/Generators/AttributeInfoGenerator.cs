using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
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

    context.RegisterSourceOutput(attributeTypes, (spc, source) => {
      Execute(source.Distinct(NamedTypeSymbolEqualityComparer.Default), spc);
    });
  }

  private static IMethodSymbol? GetMethodInvocations(GeneratorSyntaxContext context, CancellationToken token) {
    var genericName = (GenericNameSyntax)context.Node;
    var semanticModel = context.SemanticModel;

    // Get the symbol for the generic name
    var symbolInfo = semanticModel.GetSymbolInfo(genericName);
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

  private static void Execute(IEnumerable<INamedTypeSymbol> classSymbols, SourceProductionContext context) {
    var allClassSymbols = classSymbols.GetPropertyInitializations();
    foreach (var initializer in allClassSymbols) {
      var classSymbol = initializer.Value;
      var templateParams = new {
          Namespace = classSymbol.Namespace.ToDisplayString(),
          AttributeName = classSymbol.Name,
          HasParentClass = classSymbol.Base is not null && !classSymbol.Base.Symbol.IsSameType<Attribute>(),
          ParentAttribute = classSymbol.Base?.Symbol.ToDisplayString(),
          classSymbol.Constructors,
          classSymbol.Properties
      };

      var handlebars = Handlebars.Create();
      handlebars.Configuration.TextEncoder = null;
      handlebars.Configuration.FormatterProviders.Add(new EnumStringValueFormatter());

      var template = handlebars.Compile(SourceTemplates.AttributeInfoTemplate);

      var templateResult = template(templateParams);
      context.AddSource($"{classSymbol.Name}Info.g.cs", templateResult);
    }
  }
}
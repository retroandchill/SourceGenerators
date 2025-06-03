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
using Retro.SourceGeneratorUtilities.Core.Types;
using Retro.SourceGeneratorUtilities.Model;
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
    
    var attributesAndCompilation = context.CompilationProvider.Combine(attributeTypes);

    context.RegisterSourceOutput(attributesAndCompilation, (spc, source) => {
      foreach (var attributeType in source.Right.Distinct(NamedTypeSymbolEqualityComparer.Default)) {
        Execute(source.Left, attributeType, spc);
      }
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

  private static void Execute(Compilation compilation, INamedTypeSymbol classSymbol, SourceProductionContext context) {
    var primaryConstructor = classSymbol.DeclaringSyntaxReferences
        .Select(x => x.GetSyntax())
        .OfType<ClassDeclarationSyntax>()
        .Where(x => x.ParameterList is not null)
        .Select(x => {
          var semanticModel = compilation.GetSemanticModel(x.SyntaxTree);
          var primaryConstructorBaseTypeSyntax = x.BaseList?.Types
              .OfType<PrimaryConstructorBaseTypeSyntax>()
              .FirstOrDefault();
          return new {
              Parameters = x.ParameterList!.Parameters.Select((y, i) => new {
                      Type = semanticModel.GetTypeInfo(y.Type!).Type?.ToDisplayString() ?? string.Empty,
                      Name = y.Identifier.ValueText,
                      IsLast = i == x.ParameterList.Parameters.Count - 1

                  })
                  .ToList(),
              HasBaseCall = true,
          };
        })
        .FirstOrDefault();
    
    var templateParams = new {
        Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
        AttributeName = classSymbol.Name,
        HasParentClass = classSymbol.BaseType is not null && !classSymbol.BaseType.IsSameType<Attribute>(),
        ParentAttribute = classSymbol.BaseType?.ToDisplayString(),
        Constructors = classSymbol.Constructors
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Select(ConstructorDeclarationInfo.FromConstructor)
            .ToList(),
        Properties = classSymbol.GetProperties()
            .Select(x => new {
                Accessibility = x.DeclaredAccessibility.ToDisplayString(),
                Type = x.Type.ToDisplayString(),
                Name = x.Name
            })
            .ToList()
    };

    var handlebars = Handlebars.Create();
    handlebars.Configuration.TextEncoder = null;

    var template = handlebars.Compile(SourceTemplates.AttributeInfoTemplate);

    var templateResult = template(templateParams);
    context.AddSource($"{classSymbol.Name}Info.g.cs", templateResult);
  }
}
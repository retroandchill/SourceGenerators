using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Errors;
using Retro.SourceGeneratorUtilities.Core.Members;
using Retro.SourceGeneratorUtilities.Core.Model.Attributes;
using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Core.Attributes;

public static class AttributeInfoTypeExtensions {
  public static AttributeInfoTypeInfo GetAttributeInfoTypeInfo(this AttributeData attributeData) {
    return attributeData.TryGetAttributeInfoTypeInfo(out var info) ? info : throw new InvalidOperationException("Invalid attribute type");
  }
  
  public static bool TryGetAttributeInfoTypeInfo(this AttributeData attributeData, out AttributeInfoTypeInfo info) {
    if (attributeData.AttributeClass is null) {
      info = default;
      return false;
    }

    if (attributeData.AttributeClass.IsGenericType && 
        attributeData.AttributeClass.ConstructedFrom.IsSameType(typeof(AttributeInfoTypeAttribute<>))) {
      info = new AttributeInfoTypeInfo(attributeData.AttributeClass.TypeArguments[0]);
      return true;
    }
    
    if (!attributeData.AttributeClass.IsSameType<AttributeInfoTypeAttribute>() || !attributeData.HasMatchingConstructor(typeof(Type))) {
      info = default;
      return false;
    }

    info = new AttributeInfoTypeInfo(attributeData.ConstructorArguments[0].GetTypedValue<ITypeSymbol>());
    return true;
  }

  public static IEnumerable<AttributeInfoTypeInfo> GetAttributeInfoTypeInfos(this IEnumerable<AttributeData> attributes) {
    return attributes
        .Select(a => a.TryGetAttributeInfoTypeInfo(out var info)
                    ? (Found: true, Info: info)
                    : (Found: false, Info: default))
        .Where(t => t.Found)
        .Select(t => t.Info);
  }

  public static DiagnosticResult<AttributeInfoTypeOverview> ExtractAttributeInfoTypeOverview(this INamedTypeSymbol typeSymbol, 
                                                                           ImmutableArray<INamedTypeSymbol> possibleTypes) {
    var attributeType = typeSymbol.GetAttributes()
        .GetAttributeInfoTypeInfos()
        .Select(x => x.Type)
        .OfType<INamedTypeSymbol>()
        .Single();
    
    var attributeConstructors = attributeType.Constructors;
    var modelConstructors = typeSymbol.Constructors;

    var validatedConstructors = attributeConstructors
        .Select(attributeConstructor => attributeConstructor.FindMatchingConstructor(typeSymbol, modelConstructors))
        .Collect()
        .SelectNonNull((m, i) => new AttributeInfoConstructorOverview {
            Parameters = m.Parameters
                .Select((p, j) => new AttributeInfoConstructorParamOverview(p) {
                    Index = i,
                    IsLast = j == m.Parameters.Length - 1
                })
                .ToImmutableList(),
            IsLast = i == attributeConstructors.Length - 1
        })
        .ToImmutableArray();

    var attributeProperties = attributeType.GetBaseTypeAndThis()
        .SelectMany(t => t.GetPublicProperties())
        .Where(t => t.SetMethod?.DeclaredAccessibility == Accessibility.Public)
        .ToImmutableArray();
    var modelProperties = typeSymbol.GetBaseTypeAndThis()
        .SelectMany(t => t.GetPublicProperties())
        .Where(t => t.SetMethod?.DeclaredAccessibility == Accessibility.Public)
        .ToImmutableArray();

    var validatedProperties = attributeProperties
        .Select(property => property.FindMatchingProperty(typeSymbol, modelProperties))
        .Collect()
        .SelectNonNull((p, i) => new AttributeInfoPropertyOverview(p) {
            DefaultValue = p.DeclaringSyntaxReferences
                .Select(s => s.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .Select(s => s.Initializer?.Value)
                .FirstOrDefault(),
            IsLast = i == attributeProperties.Length - 1
        })
        .ToImmutableArray();

    return validatedConstructors
        .Combine(validatedProperties,
                 (c, p) => new AttributeInfoTypeOverview(typeSymbol, attributeType) {
                     Constructors = c,
                     Properties = p,
                     ChildClasses = [
                         ..possibleTypes
                             .Where(t => t.BaseType?.Equals(typeSymbol, SymbolEqualityComparer.Default) ?? false)
                             .Select(t => new ChildAttributeTypeInfoOverview(t, t.GetAttributes()
                                                                                 .GetAttributeInfoTypeInfos()
                                                                                 .Select(x => x.Type)
                                                                                 .OfType<INamedTypeSymbol>()
                                                                                 .Single()))
                             .Where(d =>
                                        d.AttributeType.BaseType?.Equals(
                                            attributeType, SymbolEqualityComparer.Default) ?? false)
                             .GroupBy(d => d.AttributeType, NamedTypeSymbolEqualityComparer.Default)
                             .Select(d => d.First())
                     ]
                 });
  }

  private static DiagnosticResult<IMethodSymbol?> FindMatchingConstructor(this IMethodSymbol targetConstructor,
                                                                          INamedTypeSymbol source, 
                                                                          ImmutableArray<IMethodSymbol> modelConstructors) {

    foreach (var modelConstructor in modelConstructors) {
      if (modelConstructor.Parameters.Length != targetConstructor.Parameters.Length) {
        continue;
      }
      
      var isMatch = true;
      for (var i = 0; i < modelConstructor.Parameters.Length; i++) {
        var modelParameter = modelConstructor.Parameters[i];
        var targetParameter = targetConstructor.Parameters[i];

        if (targetParameter.Type.IsSameType<Type>()) {
          if (modelParameter.Type.IsSameType<ITypeSymbol>()) continue;
          isMatch = false;
          break;
        } else if (!targetParameter.Type.Equals(modelParameter.Type, SymbolEqualityComparer.Default)) {
          isMatch = false;
          break;
        }
      }

      if (isMatch) {
        return new DiagnosticResult<IMethodSymbol?>(modelConstructor);
      }
    }

    return new DiagnosticResult<IMethodSymbol?>(null,
                                                  Diagnostic.Create(new DiagnosticDescriptor("SGU001",
                                                                      "Missing model constructor",
                                                                      $"Missing attribute constructor for: {targetConstructor}",
                                                                      "Model Generator",
                                                                      DiagnosticSeverity.Error,
                                                                      true),
                                                                    source.Locations.FirstOrDefault(),
                                                                    source.Locations.Skip(1)));
  }

  private static DiagnosticResult<IPropertySymbol?> FindMatchingProperty(
      this IPropertySymbol targetProperty, INamedTypeSymbol source, ImmutableArray<IPropertySymbol> modelProperties) {
    foreach (var modelProperty in modelProperties.Where(modelProperty => modelProperty.Name == targetProperty.Name)) {
      if (targetProperty.Type.IsSameType<Type>()) {
        if (!modelProperty.Type.IsSameType<ITypeSymbol>()) continue;
        return new DiagnosticResult<IPropertySymbol?>(modelProperty);
      }

      if (targetProperty.Type.Equals(modelProperty.Type, SymbolEqualityComparer.Default)) {
        return new DiagnosticResult<IPropertySymbol?>(modelProperty);
      }
    }

    return new DiagnosticResult<IPropertySymbol?>(null,
        Diagnostic.Create(new DiagnosticDescriptor("SGU002",
                                                   "Missing model property",
                                                   $"Missing property: {targetProperty}",
                                                   "Model Generator",
                                                   DiagnosticSeverity.Error,
                                                   true),
                          source.Locations.FirstOrDefault(),
                          source.Locations.Skip(1)));
  }
}
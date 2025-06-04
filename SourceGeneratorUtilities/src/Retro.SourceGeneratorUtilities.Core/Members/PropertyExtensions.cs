using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Model;

namespace Retro.SourceGeneratorUtilities.Core.Members;

public static class PropertyExtensions {
  public static IEnumerable<PropertyOverview> GetProperties(this ITypeSymbol typeSymbol) {
    return typeSymbol.GetMembers()
        .OfType<IPropertySymbol>()
        .Select(GetPropertyOverview);
  }
  
  public static PropertyOverview GetPropertyOverview(this IPropertySymbol propertySymbol) {
    return new PropertyOverview(propertySymbol.Type, propertySymbol.Name) {
        Accessibility = propertySymbol.DeclaredAccessibility.ToAccessibilityLevel(),
        HasSetter = propertySymbol.SetMethod is not null,
        Initializer = propertySymbol.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<PropertyDeclarationSyntax>()
            .Where(x => x.Initializer is not null)
            .Select(x => x.Initializer!.Value)
            .FirstOrDefault()
    };
  }
}
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Types;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record PropertyOverview(ITypeSymbol Type, string Name) {

  public string AttributeType => Type.IsSameType<Type>() ? nameof(ITypeSymbol) : Type.ToDisplayString();

  public AccessibilityLevel Accessibility { get; init; } = AccessibilityLevel.Private;
  
  public bool HasSetter { get; init; }
  
  public bool HasInitializer => Initializer is not null;
  
  public ExpressionSyntax? Initializer { get; init; }

  public string? AttributeInitializer {
    get {
      if (Initializer is null) {
        return null;
      }

      if (Type.IsSameType<Type>() && Initializer is TypeOfExpressionSyntax typeofSyntax) {
        return $"compilation.GetNamedType<{typeofSyntax.Type}>()";
      }
      
      return Initializer.ToString();
    }
  }
  
}
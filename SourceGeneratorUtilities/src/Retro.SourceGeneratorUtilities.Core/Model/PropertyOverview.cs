using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Types;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record PropertyOverview(IPropertySymbol Symbol) {
  
  public ITypeSymbol Type { get; init; } = Symbol.Type;
  
  public string Name { get; init; } = Symbol.Name;

  public AccessibilityLevel Accessibility { get; init; } = AccessibilityLevel.Private;
  
  public bool HasSetter { get; init; }
  
  public bool HasInitializer => Initializer is not null;
  
  public ExpressionSyntax? Initializer { get; init; }
  
}
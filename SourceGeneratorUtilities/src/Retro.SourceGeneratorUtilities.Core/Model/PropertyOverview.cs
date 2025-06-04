using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record PropertyOverview(ITypeSymbol Type, string Name) {

  public AccessibilityLevel Accessibility { get; init; } = AccessibilityLevel.Private;
  
  public bool HasSetter { get; init; }
  
  public bool HasInitializer => Initializer is not null;
  
  public ExpressionSyntax? Initializer { get; init; }
  
}
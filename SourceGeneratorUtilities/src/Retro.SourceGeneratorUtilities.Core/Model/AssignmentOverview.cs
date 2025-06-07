using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record struct AssignmentOverview(IPropertySymbol Left, ExpressionSyntax Right) {
  
  public ITypeSymbol PropertyType => Left.Type;
  public string PropertyName => Left.Name;
  
  public bool HasSetter => Left.SetMethod is not null;
}
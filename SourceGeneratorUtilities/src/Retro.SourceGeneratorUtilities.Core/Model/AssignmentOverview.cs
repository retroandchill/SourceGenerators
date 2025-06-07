using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record AssignmentOverview(IPropertySymbol Left, ExpressionSyntax Right) {
  
  public ITypeSymbol PropertyType { get; init; } = Left.Type;
  public string PropertyName { get; init; } = Left.Name;
  
  public bool HasSetter { get; init; } = Left.SetMethod is not null;
}
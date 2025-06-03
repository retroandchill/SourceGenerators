using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record ParameterOverview(ITypeSymbol Type, string Name) {

  public bool HasDefaultValue => DefaultValue is not null;
  
  public ExpressionSyntax? DefaultValue { get; init; }
  
  public bool IsLast { get; init; }
  
}
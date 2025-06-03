using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record struct ArgumentOverview(ArgumentSyntax Expression) {
  
  public bool IsLast { get; init; }
  
}
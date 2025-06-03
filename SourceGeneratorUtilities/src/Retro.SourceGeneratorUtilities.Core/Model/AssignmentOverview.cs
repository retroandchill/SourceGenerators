using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record struct AssignmentOverview(ExpressionSyntax Left, ExpressionSyntax Right);
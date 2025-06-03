using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Members;

namespace Retro.SourceGeneratorUtilities.Model;

public record ConstructorDeclarationInfo {
  public required List<ParameterInfo> Parameters { get; init; }
  
  public required bool HasExternalCall { get; init; }
  
  public required string? ConstructorInitializer { get; init; }
  
  public required List<InitializerArgumentInfo> InitializerArguments { get; init; }
  
  public required List<AssignmentInfo> Assignments { get; init; }

  public static ConstructorDeclarationInfo FromConstructor(IMethodSymbol method) {
    var constructorInitializer = method.GetConstructorInitializer();
    
    return new ConstructorDeclarationInfo {
        Parameters = method.Parameters.Select((p, i) => new ParameterInfo {
            Type = p.Type.ToDisplayString(),
            Name = p.Name,
            HasDefaultValue = p.HasExplicitDefaultValue,
            IsLast = i == method.Parameters.Length - 1
        })
        .ToList(),
        HasExternalCall = constructorInitializer is not null,
        ConstructorInitializer = constructorInitializer?.Type.ToString(),
        InitializerArguments = constructorInitializer?.Arguments
            .Select((y, i) => new InitializerArgumentInfo {
                Expression = y.Expression.ToString(),
                IsLast = y.IsLast
            })
            .ToList() ?? [],
        Assignments = method.GetPropertyAssignments()
            .Select(y => new AssignmentInfo {
                Left = y.Left.ToString(),
                Right = y.Right.ToString()
            })
            .ToList()
    };
  }
}
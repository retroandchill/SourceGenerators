using System;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record ParameterOverview(ITypeSymbol Type, string Name) {

  public ITypeSymbol NonNullableType => Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
  
  public string AttributeType => Type.IsSameType<Type>() ? typeof(ITypeSymbol).FullName! : Type.ToDisplayString();
  
  public bool IsEnum => Type.TypeKind == TypeKind.Enum;

  public bool IsReflectionType => Type.IsSameType<Type>();
  
  public bool IsRegularType => !IsEnum && !IsReflectionType;

  public bool HasDefaultValue => DefaultValue is not null;
  
  public ExpressionSyntax? DefaultValue { get; init; }
  
  public int Index { get; init; }
  
  public bool IsLast { get; init; }
  
}
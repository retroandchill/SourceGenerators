using System;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents an overview of a parameter in a method or constructor, including its type, name,
/// and additional metadata.
/// </summary>
/// <param name="Type">
/// The <see cref="ITypeSymbol"/> representing the parameter's type.
/// </param>
/// <param name="Name">
/// The name of the parameter as a string.
/// </param>
public record ParameterOverview(ITypeSymbol Type, string Name) {

  /// <summary>
  /// Gets the non-nullable version of the parameter's type. This property ensures the type
  /// is annotated as non-nullable, regardless of its original nullability annotation.
  /// </summary>
  /// <remarks>
  /// This property returns a new instance of the type with the nullability annotation explicitly set
  /// to indicate that the type is not nullable. It utilizes the Roslyn API to modify the nullable
  /// annotation of the parameter type.
  /// </remarks>
  public ITypeSymbol NonNullableType => Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

  /// <summary>
  /// Indicates whether the parameter has a default value assigned.
  /// </summary>
  /// <remarks>
  /// This property returns <c>true</c> if the parameter has a non-null default value
  /// expression assigned to it. Otherwise, it returns <c>false</c>.
  /// </remarks>
  public bool HasDefaultValue => DefaultValue is not null;

  /// <summary>
  /// Gets or sets the default value assigned to the parameter, if any. This property represents
  /// the default value syntax specified in the source code for the parameter declaration.
  /// </summary>
  /// <remarks>
  /// If the parameter has an explicitly defined default value in its declaration, this property
  /// will hold the corresponding expression syntax for that value. Otherwise, it will be null.
  /// </remarks>
  public ExpressionSyntax? DefaultValue { get; init; }

  /// <summary>
  /// Gets the zero-based index of the parameter in the parameter list of its containing method or constructor.
  /// </summary>
  /// <remarks>
  /// This property represents the order in which the parameter appears in the declaration of its containing
  /// method or constructor. It is automatically assigned when the parameter metadata is processed.
  /// </remarks>
  public int Index { get; init; }

  /// <summary>
  /// Determines if the parameter is the last in the collection of parameters within its method or constructor.
  /// </summary>
  /// <remarks>
  /// This property is typically set based on the parameter's position in the collection.
  /// It is useful for scenarios where processing or validation logic might differ for the final parameter,
  /// such as constructing strings, formatting, or applying specific rules related to sequence.
  /// </remarks>
  public bool IsLast { get; init; }
  
}
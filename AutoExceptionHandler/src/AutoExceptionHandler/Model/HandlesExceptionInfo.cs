using System.Collections.Immutable;
using AutoExceptionHandler.Annotations;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
namespace AutoExceptionHandler.Model;

/// <summary>
/// Represents information about exceptions that are handled by methods
/// annotated with the <see cref="HandlesExceptionAttribute"/>.
/// </summary>
[AttributeInfoType<HandlesExceptionAttribute>]
public record struct HandlesExceptionInfo {

  /// <summary>
  /// Gets the collection of exception types associated with the
  /// <see cref="HandlesExceptionInfo"/> structure. These exception types
  /// represent the exceptions handled by methods annotated with the
  /// <see cref="HandlesExceptionAttribute"/>.
  /// </summary>
  public ImmutableArray<ITypeSymbol> ExceptionTypes { get; }

  /// <summary>
  /// Represents the information required for identifying exceptions
  /// that are handled by methods annotated with the <see cref="HandlesExceptionAttribute"/>.
  /// </summary>
  /// <remarks>
  /// This struct provides the ability to store and manage the types of exceptions
  /// that are explicitly handled within a given context.
  /// The <see cref="HandlesExceptionAttribute"/> annotation is used to mark
  /// methods handling specific exceptions.
  /// </remarks>
  /// <param name="exceptionTypes">The types of exceptions that are take in.</param>
  public HandlesExceptionInfo(params ITypeSymbol[] exceptionTypes) {
    ExceptionTypes = [..exceptionTypes];
  }
  
}
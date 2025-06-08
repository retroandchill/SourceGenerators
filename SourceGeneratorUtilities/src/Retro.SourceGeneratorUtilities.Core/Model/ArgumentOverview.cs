using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents an overview of an argument in the syntax tree.
/// </summary>
/// <param name="Expression">
/// The syntax representation of the argument within the syntax tree.
/// </param>
public record struct ArgumentOverview(ArgumentSyntax Expression) {

  /// <summary>
  /// Gets a value indicating whether the current argument is the last one in a sequence.
  /// </summary>
  /// <remarks>
  /// This property is typically used to determine if an argument is the final element
  /// in a collection of arguments, such as those passed to a method or constructor.
  /// </remarks>
  public bool IsLast { get; init; }
  
}
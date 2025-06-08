using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents an overview of a property assignment, including the left-hand (property) and right-hand (value) sides of the assignment.
/// </summary>
/// <remarks>
/// This model is useful for source generation utilities to track assignments in constructors or other contexts where properties are being initialized.
/// </remarks>
/// <param name="Left">The property symbol on the left-hand side of the assignment.</param>
/// <param name="Right">The expression syntax on the right-hand side of the assignment.</param>
public record AssignmentOverview(IPropertySymbol Left, ExpressionSyntax Right) {

  /// <summary>
  /// Gets the type symbol of the property being assigned.
  /// </summary>
  /// <remarks>
  /// This property represents the type of the property on the left-hand side of the assignment.
  /// It is derived from the <see cref="Microsoft.CodeAnalysis.IPropertySymbol"/> provided in the constructor.
  /// </remarks>
  public ITypeSymbol PropertyType { get; init; } = Left.Type;

  /// <summary>
  /// Gets the name of the property being assigned.
  /// </summary>
  /// <remarks>
  /// This property represents the identifier of the property on the left-hand side of the assignment expression.
  /// It is derived from the <see cref="Microsoft.CodeAnalysis.IPropertySymbol.Name"/> of the provided property symbol.
  /// </remarks>
  public string PropertyName { get; init; } = Left.Name;

  /// <summary>
  /// Gets a value indicating whether the property being assigned has a setter.
  /// </summary>
  /// <remarks>
  /// This property determines if the left-hand property in the assignment includes a setter method.
  /// It is derived from the <see cref="Microsoft.CodeAnalysis.IPropertySymbol.SetMethod"/> provided for the property.
  /// </remarks>
  public bool HasSetter { get; init; } = Left.SetMethod is not null;
}
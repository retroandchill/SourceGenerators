using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents an overview of a constructor, including its symbol, parameters, and other related metadata.
/// </summary>
/// <param name="Symbol">
/// The <see cref="IMethodSymbol"/> representation of the constructor.
/// </param>
/// <param name="Parameters">
/// The list of parameters for the constructor, represented as a collection of <see cref="ParameterOverview"/>.
/// </param>
public record ConstructorOverview(IMethodSymbol Symbol, IReadOnlyList<ParameterOverview> Parameters) {

  /// <summary>
  /// Indicates whether the constructor represented by this instance is a
  /// primary constructor in the source code.
  /// </summary>
  /// <remarks>
  /// A primary constructor is defined directly within the declaration of a class,
  /// record, or struct, typically as part of the type's syntax declaration.
  /// It combines the declaration of instance fields and a constructor into a single cohesive mechanism.
  /// </remarks>
  public bool IsPrimaryConstructor { get; init; }

  /// <summary>
  /// Indicates whether the constructor represented by this instance has an
  /// initializer explicitly defined in the source code.
  /// </summary>
  /// <remarks>
  /// A constructor initializer provides the ability to invoke a different constructor
  /// within the same type or within the base type. It is typically expressed as
  /// `this(...)` or `base(...)` in the constructor's definition.
  /// </remarks>
  public bool HasInitializer => Initializer is not null;

  /// <summary>
  /// Represents the initializer for the constructor associated with this instance.
  /// </summary>
  /// <remarks>
  /// An initializer typically refers to an explicit invocation of another constructor
  /// in the same type (using `this`) or a constructor in its base type (using `base`),
  /// allowing shared initialization logic between constructors.
  /// </remarks>
  public ConstructorInitializerOverview? Initializer { get; init; }

  /// <summary>
  /// Represents the collection of property assignments occurring within the constructor's body.
  /// </summary>
  /// <remarks>
  /// Assignments provide a mapping of properties being initialized with corresponding values in the constructor.
  /// Each <see cref="AssignmentOverview"/> instance represents a single assignment, where a property on the left-hand
  /// side is assigned a value represented by an expression on the right-hand side.
  /// </remarks>
  public IReadOnlyList<AssignmentOverview> Assignments { get; init; } = [];

}
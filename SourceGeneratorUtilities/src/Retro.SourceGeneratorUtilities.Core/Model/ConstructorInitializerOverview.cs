using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents an overview of a constructor initializer, providing details about its type and arguments.
/// </summary>
/// <param name="Symbol">The symbol representing the constructor initializer.</param>
/// <param name="Type">The type of the constructor initializer.</param>
/// <param name="Arguments">The arguments provided to the constructor initializer.</param>
public record ConstructorInitializerOverview(IMethodSymbol Symbol, InitializerType Type, IReadOnlyList<ArgumentOverview> Arguments);
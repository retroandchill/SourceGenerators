using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record ConstructorInitializerOverview(IMethodSymbol Symbol, InitializerType Type, IReadOnlyList<ArgumentOverview> Arguments);
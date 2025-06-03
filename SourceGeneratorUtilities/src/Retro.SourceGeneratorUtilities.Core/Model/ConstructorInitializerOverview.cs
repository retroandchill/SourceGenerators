using System.Collections.Immutable;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record struct ConstructorInitializerOverview(InitializerType Type, ImmutableArray<ArgumentOverview> Arguments);
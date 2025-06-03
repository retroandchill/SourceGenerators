using System.Collections.Immutable;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record ConstructorInitializerOverview(InitializerType Type, ImmutableArray<ArgumentOverview> Arguments);
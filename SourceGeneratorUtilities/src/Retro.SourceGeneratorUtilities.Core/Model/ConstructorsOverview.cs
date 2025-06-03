using System.Collections.Immutable;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public record struct ConstructorsOverview(ConstructorOverview? Primary, ImmutableArray<ConstructorOverview> Declared);
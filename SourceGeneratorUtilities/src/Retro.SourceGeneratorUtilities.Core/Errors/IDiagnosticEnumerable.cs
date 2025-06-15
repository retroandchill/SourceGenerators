using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Errors;

public interface IDiagnosticEnumerable<out T> : IEnumerable<T> {
  
  internal ImmutableArray<Diagnostic>.Builder DiagnosticBuilder { get; }
}
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Retro.ReadOnlyParams.Annotations;

namespace Retro.SourceGeneratorUtilities.Core.Errors;

internal class ChainDiagnosticEnumerable<T>([ReadOnly] IEnumerable<T> baseEnumerable,
                                            ImmutableArray<Diagnostic>.Builder diagnosticBuilder) : IDiagnosticEnumerable<T> {
  
  public ImmutableArray<Diagnostic>.Builder DiagnosticBuilder { get; } = diagnosticBuilder;
  
  public IEnumerator<T> GetEnumerator() => baseEnumerable.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
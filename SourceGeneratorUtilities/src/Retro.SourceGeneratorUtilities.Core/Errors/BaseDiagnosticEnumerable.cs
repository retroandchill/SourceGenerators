using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Retro.ReadOnlyParams.Annotations;

namespace Retro.SourceGeneratorUtilities.Core.Errors;

internal class BaseDiagnosticEnumerable<T>([ReadOnly] IEnumerable<DiagnosticResult<T>> source) : IDiagnosticEnumerable<T> {
  public ImmutableArray<Diagnostic>.Builder DiagnosticBuilder { get; init; } = ImmutableArray.CreateBuilder<Diagnostic>();

  public IEnumerator<T> GetEnumerator() {
    foreach (var (result, diagnostics) in source) {
      DiagnosticBuilder.AddRange(diagnostics);
      yield return result;
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
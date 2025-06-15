using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Errors;

public record struct DiagnosticResult<T>(T Result, ImmutableArray<Diagnostic> Diagnostics) {
  public DiagnosticResult(T result) : this(result, []) {
    
  }

  public DiagnosticResult(T result, Diagnostic diagnostic) : this(result, [diagnostic]) {
    
  }

  public static implicit operator DiagnosticResult<T>(T result) {
    return new DiagnosticResult<T>(result);
  }

}
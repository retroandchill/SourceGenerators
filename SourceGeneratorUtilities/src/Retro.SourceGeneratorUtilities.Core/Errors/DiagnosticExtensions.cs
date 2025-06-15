using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Errors;

public static class DiagnosticExtensions {

  public static DiagnosticResult<TResult> Select<TSource, TResult>(this DiagnosticResult<TSource> source,
                                                                   Func<TSource, TResult> mapper) {
    return new DiagnosticResult<TResult>(mapper(source.Result), source.Diagnostics);
  }
  
  public static DiagnosticResult<TResult> Select<TSource, TResult>(this DiagnosticResult<TSource> source,
                                                                   Func<TSource, DiagnosticResult<TResult>> mapper) {
    var result = mapper(source.Result);
    return result with { Diagnostics = [..source.Diagnostics.Concat(result.Diagnostics)] };
  }

  public static DiagnosticResult<TResult> Combine<T0, T1, TResult>(this DiagnosticResult<T0> source,
                                                                   DiagnosticResult<T1> other,
                                                                   Func<T0, T1, TResult> combiner) {
    return new DiagnosticResult<TResult>(combiner(source.Result, other.Result),
                                         [..source.Diagnostics.Concat(other.Diagnostics)]);
  }
  
  public static DiagnosticResult<TResult> Combine<T0, T1, TResult>(this DiagnosticResult<T0> source,
                                                                   DiagnosticResult<T1> other,
                                                                   Func<T0, T1, DiagnosticResult<TResult>> combiner) {
    var result = combiner(source.Result, other.Result);
    return result with { Diagnostics = [..source.Diagnostics.Concat(other.Diagnostics).Concat(result.Diagnostics)] };
  }

  public static IDiagnosticEnumerable<T> Collect<T>(this IEnumerable<DiagnosticResult<T>> source) {
    return new BaseDiagnosticEnumerable<T>(source);
  }

  public static bool ReportDiagnostics<T>(this SourceProductionContext sourceProductionContext,
                                          DiagnosticResult<T> source) {
    var hasErrors = false;
    foreach (var diagnostic in source.Diagnostics) {
      if (diagnostic.Severity == DiagnosticSeverity.Error) {
        hasErrors = true;
      }
      
      sourceProductionContext.ReportDiagnostic(diagnostic);
    }
    
    return hasErrors;
  }
}
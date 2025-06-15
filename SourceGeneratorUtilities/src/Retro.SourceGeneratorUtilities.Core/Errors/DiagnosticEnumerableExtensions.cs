using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Retro.SourceGeneratorUtilities.Core.Utils;

namespace Retro.SourceGeneratorUtilities.Core.Errors;

public static class DiagnosticEnumerableExtensions {
  public static IDiagnosticEnumerable<TResult> Select<T, TResult>(this IDiagnosticEnumerable<T> source,
                                                                  Func<T, TResult> mapper) {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T>)source).Select(mapper),
        source.DiagnosticBuilder);
  }
  
  public static IDiagnosticEnumerable<TResult> Select<T, TResult>(this IDiagnosticEnumerable<T> source,
                                                                  Func<T, int, TResult> mapper) {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T>)source).Select(mapper),
                                                  source.DiagnosticBuilder);
  }
  
  public static IDiagnosticEnumerable<TResult> SelectNonNull<T, TResult>(this IDiagnosticEnumerable<T?> source,
                                                                  Func<T, TResult> mapper) where T : class {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T?>)source).SelectNonNull(mapper),
                                                  source.DiagnosticBuilder);
  }
  
  public static IDiagnosticEnumerable<TResult> SelectNonNull<T, TResult>(this IDiagnosticEnumerable<T?> source,
                                                                         Func<T, TResult> mapper) where T : struct {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T?>)source).SelectNonNull(mapper),
                                                  source.DiagnosticBuilder);
  }
  
  public static IDiagnosticEnumerable<TResult> SelectNonNull<T, TResult>(this IDiagnosticEnumerable<T?> source,
                                                                         Func<T, int, TResult> mapper) where T : class {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T?>)source).SelectNonNull(mapper),
                                                  source.DiagnosticBuilder);
  }
  
  public static IDiagnosticEnumerable<TResult> SelectNonNull<T, TResult>(this IDiagnosticEnumerable<T?> source,
                                                                         Func<T, int, TResult> mapper) where T : struct {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T?>)source).SelectNonNull(mapper),
                                                  source.DiagnosticBuilder);
  }
  

  public static IDiagnosticEnumerable<TResult> SelectMany<T, TResult>(this IDiagnosticEnumerable<T> source,
                                                                  Func<T, IEnumerable<TResult>> mapper) {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T>)source).SelectMany(mapper), source.DiagnosticBuilder);
  }

  public static IDiagnosticEnumerable<TResult> SelectMany<T, TResult>(this IDiagnosticEnumerable<T> source,
                                                                      Func<T, int, IEnumerable<TResult>> mapper) {
    return new ChainDiagnosticEnumerable<TResult>(((IEnumerable<T>)source).SelectMany(mapper), source.DiagnosticBuilder);
  }
  
  public static IDiagnosticEnumerable<T> Where<T>(this IDiagnosticEnumerable<T> source, Func<T, bool> predicate) {
    return new ChainDiagnosticEnumerable<T>(((IEnumerable<T>)source).Where(predicate), source.DiagnosticBuilder);
  }

  public static IDiagnosticEnumerable<T> WhereNotNull<T>(this IDiagnosticEnumerable<T?> source) where T : class {
    return new ChainDiagnosticEnumerable<T>(((IEnumerable<T?>)source).WhereNotNull(), source.DiagnosticBuilder);
  }
  
  public static IDiagnosticEnumerable<T> WhereNotNull<T>(this IDiagnosticEnumerable<T?> source) where T : struct {
    return new ChainDiagnosticEnumerable<T>(((IEnumerable<T?>)source).WhereNotNull(), source.DiagnosticBuilder);
  }

  public static DiagnosticResult<ImmutableArray<T>> ToImmutableArray<T>(this IDiagnosticEnumerable<T> source) {
    var immutableArray = ((IEnumerable<T>)source).ToImmutableArray();
    return new DiagnosticResult<ImmutableArray<T>>(immutableArray, source.DiagnosticBuilder.ToImmutable());
  }

}
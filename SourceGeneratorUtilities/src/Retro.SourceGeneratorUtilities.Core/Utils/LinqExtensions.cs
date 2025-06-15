using System;
using System.Collections.Generic;
using System.Linq;

namespace Retro.SourceGeneratorUtilities.Core.Utils;

public static class LinqExtensions {
  public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T?> source, 
                                                                  Func<T, TResult> selector) where T : class {
    return source.WhereNotNull().Select(selector);
  }

  public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T?> source,
                                                               Func<T, TResult> selector) where T : struct {
    return source.WhereNotNull().Select(selector);
  }

  public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T?> source,
                                                               Func<T, int, TResult> selector) where T : class {
    foreach (var (item, index) in source.Select((item, index) => (item, index))) {
      if (item is not null) {
        yield return selector(item, index);
      }
    }
  }

  public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T?> source,
                                                               Func<T, int, TResult> selector) where T : struct {
    foreach (var (item, index) in source.Select((item, index) => (item, index))) {
      if (item.HasValue) {
        yield return selector(item.Value, index);
      }
    }
  }
  
  public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class {
    return source.Where(item => item is not null)!;
  }

  public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct {
    return source.Where(item => item.HasValue).Select(item => item!.Value);
  }
  
}
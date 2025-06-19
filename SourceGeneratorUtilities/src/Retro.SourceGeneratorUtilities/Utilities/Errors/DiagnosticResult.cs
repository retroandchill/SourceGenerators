using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Errors;


#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal record struct DiagnosticResult<T>(T Result, ImmutableArray<Diagnostic> Diagnostics) {
  public DiagnosticResult(T result) : this(result, []) {
    
  }

  public DiagnosticResult(T result, Diagnostic diagnostic) : this(result, [diagnostic]) {
    
  }

  public static implicit operator DiagnosticResult<T>(T result) {
    return new DiagnosticResult<T>(result);
  }

}
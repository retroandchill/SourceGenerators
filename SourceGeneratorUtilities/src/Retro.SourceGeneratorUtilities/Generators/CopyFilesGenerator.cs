using Microsoft.CodeAnalysis;
using RhoMicro.CodeAnalysis.Generated;
namespace Retro.SourceGeneratorUtilities.Generators;

/// <summary>
/// Represents a source generator that facilitates copying files for use in code generation tasks.
/// </summary>
[Generator]
internal class CopyFilesGenerator : IIncrementalGenerator {

  /// <inheritdoc/>
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    IncludedFileSources.RegisterToContext(context);
  }
}
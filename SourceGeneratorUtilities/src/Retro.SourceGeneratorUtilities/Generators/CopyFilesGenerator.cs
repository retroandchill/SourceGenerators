using Microsoft.CodeAnalysis;
using RhoMicro.CodeAnalysis.Generated;
namespace Retro.SourceGeneratorUtilities.Generators;

[Generator]
public class CopyFilesGenerator : IIncrementalGenerator {

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    IncludedFileSources.RegisterToContext(context);
  }
}
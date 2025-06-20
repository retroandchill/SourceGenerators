using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Retro.SourceGeneratorUtilities.Generators;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Retro.SourceGeneratorUtilities.Test.Utils;

public static class GeneratorTestHelpers {
  public static Compilation CreateCompilation(string source, params IEnumerable<Type> additionalTypes) {
    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Concat(additionalTypes.Select(t => t.Assembly))
        .Select(a => a.Location)
        .Where(a => !string.IsNullOrEmpty(a))
        .Select(l => MetadataReference.CreateFromFile(l));
    
    // Add the global implicit using directives
    const string implicitUsings = """
                                  global using System;
                                  global using System.Collections.Generic;
                                  global using System.Linq;
                                  global using System.Threading;
                                  global using System.Threading.Tasks;
                                  """;

    
    var baseCompilation = CSharpCompilation.Create("compilation",
                                    [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(implicitUsings)],
                                    assemblies,
                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    var generator = new CopyFilesGenerator();
    var driver = CSharpGeneratorDriver.Create(generator);
    driver.RunGeneratorsAndUpdateCompilation(baseCompilation, out var outputCompilation, out _);
    
    return outputCompilation;
  }

  public static INamedTypeSymbol GetTypeSymbol(this Compilation compilation, string typeName) {
    var symbol = compilation.GetTypeByMetadataName(typeName);
    Assert.That(symbol, Is.Not.Null);
    return symbol;
  }
}
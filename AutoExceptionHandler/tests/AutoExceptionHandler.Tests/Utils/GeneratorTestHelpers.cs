using System;
using System.Linq;
using AutoExceptionHandler.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoExceptionHandler.Tests.Utils;

public static class GeneratorTestHelpers {
  public static Compilation CreateCompilation(string source, params Type[] additionalTypes) {
    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Concat(additionalTypes.Select(t => t.Assembly))
        .Select(a => a.Location)
        .Where(a => !string.IsNullOrEmpty(a))
        .Select(l => MetadataReference.CreateFromFile(l));
    var baseCompilation = CSharpCompilation.Create("compilation",
        [CSharpSyntaxTree.ParseText(source)],
        assemblies,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    
    var generator = new CopyFilesGenerator();
    var driver = CSharpGeneratorDriver.Create(generator);
    driver.RunGeneratorsAndUpdateCompilation(baseCompilation, out var outputCompilation, out _);
    
    return outputCompilation;
  }
}
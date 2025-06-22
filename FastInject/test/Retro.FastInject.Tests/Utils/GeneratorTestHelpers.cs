using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Retro.FastInject.Tests.Utils;

public static class GeneratorTestHelpers {
  public static Compilation CreateCompilation(string source, params IEnumerable<Type> additionalTypes) {
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

  public static ITypeSymbol GetTypeSymbol(this Compilation compilation, string typeName) {
    var symbol = compilation.GetTypeByMetadataName(typeName);
    Assert.That(symbol, Is.Not.Null);
    return symbol;
  }

  public static IMethodSymbol GetMethodSymbol(this Compilation compilation, string typeName, string methodName) {
    var typeSymbol = (INamedTypeSymbol)GetTypeSymbol(compilation, typeName);
    return typeSymbol.GetMembers(methodName)
        .OfType<IMethodSymbol>()
        .First();
  }
}
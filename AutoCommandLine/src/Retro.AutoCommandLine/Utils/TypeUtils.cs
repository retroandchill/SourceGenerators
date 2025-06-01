using Microsoft.CodeAnalysis;
namespace Retro.AutoCommandLine.Utils;

public static class TypeUtils {
  public static string GetMetadataName(this ITypeSymbol typeSymbol) {
    return $"{typeSymbol.ContainingNamespace}.{typeSymbol.MetadataName}";
  }
}
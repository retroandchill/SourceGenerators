using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Members;

public static class PropertyExtensions {
  public static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol typeSymbol) {
    return typeSymbol.GetMembers().OfType<IPropertySymbol>();
  }
}
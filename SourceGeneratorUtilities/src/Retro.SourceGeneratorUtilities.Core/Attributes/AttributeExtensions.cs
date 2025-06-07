using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Model;
using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Core.Attributes;

public static class AttributeExtensions {
  public static AttributeInfo<T> GetInfo<T>(this AttributeData attributeData) where T : Attribute {
    return new AttributeInfo<T>(attributeData);
  }

  public static IEnumerable<AttributeInfo<T>> GetInfo<T>(this IEnumerable<AttributeData> attributeData) where T : Attribute {
    return attributeData
        .Where(x => x.AttributeClass?.IsSameType<T>() ?? false)
        .Select(x => x.GetInfo<T>());
  }

  public static bool HasMatchingConstructor(this AttributeData attributeData, params Type[] constructorTypes) {
    if (attributeData.ConstructorArguments.Length != constructorTypes.Length) {
      return false;
    }

    for (var i = 0; i < attributeData.ConstructorArguments.Length; i++) {
      var constructorType = constructorTypes[i];
      var constructorArgument = attributeData.ConstructorArguments[i];

      if (!constructorArgument.Type?.IsSameType(constructorType) ?? false) {
        return false;
      }
    }

    return true;
  }
  
}
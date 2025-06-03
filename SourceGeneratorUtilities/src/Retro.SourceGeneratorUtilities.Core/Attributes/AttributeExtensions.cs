using System;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Attributes;

public static class AttributeExtensions {
  public static AttributeInfo<T> GetInfo<T>(this AttributeData attributeData) where T : Attribute {
    return new AttributeInfo<T>(attributeData);
  }
}
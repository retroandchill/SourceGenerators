using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Attributes;
using Retro.SourceGeneratorUtilities.Generator.Sample.Model;

namespace Retro.SourceGeneratorUtilities.Generator.Sample;

public class Class1 {
  public void ProcessAttribute(AttributeData attributeData) {
    var info = attributeData.GetDummyAttributeInfo();
  }
}
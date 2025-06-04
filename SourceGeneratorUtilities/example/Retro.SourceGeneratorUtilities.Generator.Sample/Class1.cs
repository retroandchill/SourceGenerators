using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Attributes;

namespace Retro.SourceGeneratorUtilities.Generator.Sample;

public class Class1 {
  public void ProcessAttribute(AttributeData attributeData, Compilation compilation) {
    var info = attributeData.GetInfo<ChildAttribute>().New(compilation);
  }
}
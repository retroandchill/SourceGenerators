using System;
using System.Collections.Generic;

namespace Retro.SourceGeneratorUtilities.Generator.Sample;

[AttributeUsage(AttributeTargets.Class)]
public class ChildAttribute() : DummyAttribute(1) {

  public Type GenericTypeValue { get; init; } = typeof(List<int>);

}
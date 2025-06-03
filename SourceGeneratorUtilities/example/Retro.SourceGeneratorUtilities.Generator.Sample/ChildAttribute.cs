using System;

namespace Retro.SourceGeneratorUtilities.Generator.Sample;

[AttributeUsage(AttributeTargets.Class)]
public class ChildAttribute() : DummyAttribute(1) {
}
using System;
namespace Retro.SourceGeneratorUtilities.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class AttributeInfoTypeAttribute(Type type) : Attribute {

  public Type Type { get; } = type;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class AttributeInfoTypeAttribute<T>() : AttributeInfoTypeAttribute(typeof(T)) 
  where T : Attribute;
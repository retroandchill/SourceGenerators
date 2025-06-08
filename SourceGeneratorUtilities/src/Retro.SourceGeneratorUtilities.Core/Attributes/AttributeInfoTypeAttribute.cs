using System;
namespace Retro.SourceGeneratorUtilities.Core.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class AttributeInfoTypeAttribute(Type type) : Attribute {

  public Type Type { get; } = type;

}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class AttributeInfoTypeAttribute<T>() : AttributeInfoTypeAttribute(typeof(T)) 
  where T : Attribute;
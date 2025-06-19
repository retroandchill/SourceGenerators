#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Attributes;

#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal class AttributeInfoTypeAttribute(Type type) : Attribute {

  public Type Type { get; } = type;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal class AttributeInfoTypeAttribute<T>() : AttributeInfoTypeAttribute(typeof(T)) where T : Attribute;
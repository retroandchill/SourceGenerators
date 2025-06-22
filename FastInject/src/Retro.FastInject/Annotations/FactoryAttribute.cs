#if FAST_INJECT_GENERATOR
using RhoMicro.CodeAnalysis;
#else
using System;
#endif

namespace Retro.FastInject.Annotations;

[AttributeUsage(AttributeTargets.Method)]
#if FAST_INJECT_GENERATOR
[IncludeFile]
#endif
internal class FactoryAttribute(ServiceScope scope = ServiceScope.Singleton) : Attribute {

  public ServiceScope Scope { get; } = scope;
  
  public string? Key { get; init; }

}
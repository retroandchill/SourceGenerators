#if FAST_INJECT_GENERATOR
using RhoMicro.CodeAnalysis;
#else
using System;
#endif

namespace Retro.FastInject.Annotations;

/// <summary>
/// Indicates that the annotated type is registered with a transient lifecycle in a dependency injection container.
/// </summary>
/// <remarks>
/// The <c>TransientAttribute</c> is used to register a type as a transient service in the dependency injection container.
/// A transient service is created each time it is requested, ensuring a new instance is provided for each dependency resolution.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
#if FAST_INJECT_GENERATOR
[IncludeFile]
#endif
internal class TransientAttribute(Type serviceType) : DependencyAttribute(serviceType, ServiceScope.Transient);

/// <summary>
/// Marks the annotated type to be registered with a transient lifecycle in the dependency injection container.
/// </summary>
/// <remarks>
/// Applying the <c>TransientAttribute</c> ensures that a new instance of the annotated type is created on each request for dependency resolution.
/// It is particularly useful for stateless or short-lived services where shared state is unnecessary.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
internal class TransientAttribute<TService>() : TransientAttribute(typeof(TService));
using System;
#if READONLYPARAMS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.ReadOnlyParams.Annotations;

/// <summary>
/// Represents an attribute that marks a method parameter as read-only.
/// </summary>
/// <remarks>
/// Applying this attribute to a parameter indicates that its value should not be modified
/// within the method or scope where it is defined. Attempting to modify such parameters
/// can be flagged by static code analyzers.
/// </remarks>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Parameter)]
#if READONLYPARAMS_GENERATOR
[IncludeFile]
#endif
internal class ReadOnlyAttribute : Attribute;
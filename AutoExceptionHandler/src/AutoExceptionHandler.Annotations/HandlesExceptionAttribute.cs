using System;
using System.Diagnostics;

namespace AutoExceptionHandler.Annotations;

#pragma warning disable 9113

/// <summary>
/// Indicates that the decorated method is designed to handle specific exception types.
/// This attribute should be applied to methods that are intended to catch and process exceptions
/// during runtime, providing a mechanism for centralized exception handling logic.
/// This attribute can take one or more exception types as arguments, explicitly defining which
/// exceptions the method is capable of handling. If no exception types are provided, the method's
/// first parameter is considered as the exception type.
/// This attribute is intended for use in scenarios involving automatic exception handling,
/// such as within code generation or reflection-based execution contexts.
/// </summary>
/// <remarks>
/// The methods annotated with this attribute should have at least one parameter, typically matching
/// one of the specified exception types. If exception types are not explicitly mentioned, the first
/// parameter is assumed to represent the handled exception type.
/// </remarks>
/// <example>
/// This attribute is commonly used with methods in Exception Handler classes to denote the set of
/// exceptions they can process. It facilitates automatic mapping and invocation of the respective
/// error handling logic.
/// </example>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("AUTO_EXCEPTION_HANDLER_SCOPE_RUNTIME")]
public class HandlesExceptionAttribute(params Type[] exceptionTypes) : Attribute;

#pragma warning restore 9113
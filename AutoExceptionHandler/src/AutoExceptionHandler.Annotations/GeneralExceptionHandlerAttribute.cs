using System;
using System.Diagnostics;

namespace AutoExceptionHandler.Annotations;

/// <summary>
/// Identifies methods that serve as general exception handlers within a specific context.
/// </summary>
/// <remarks>
/// This attribute is used to mark method definitions that are designed to catch and process
/// exceptions in a centralized manner. Typically, these methods are partial definitions
/// intended for use with code generation or specific exception handling frameworks.
/// </remarks>
/// <example>
/// The attribute is applied to a method to signify that it will handle exceptions of a
/// specific type, usually passed as the first parameter to the method.
/// </example>
/// <seealso cref="ExceptionHandlerAttribute"/>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("AUTO_EXCEPTION_HANDLER_SCOPE_RUNTIME")]
public class GeneralExceptionHandlerAttribute : Attribute;
using Microsoft.CodeAnalysis;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities.Types;

/// <summary>
/// Represents metadata about the nullability of a type and its underlying type.
/// </summary>
/// <remarks>
/// This struct is used to encapsulate the nullability state of a type and provide
/// information about the underlying type if it is nullable.
/// </remarks>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal readonly record struct NullableData(bool IsNullable, ITypeSymbol UnderlyingType);
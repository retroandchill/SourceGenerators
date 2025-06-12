using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Model.Attributes;

/// <summary>
/// Represents information about an attribute type.
/// </summary>
/// <param name="Type">The type symbol representing the attribute type.</param>
public record struct AttributeInfoTypeInfo(ITypeSymbol Type);
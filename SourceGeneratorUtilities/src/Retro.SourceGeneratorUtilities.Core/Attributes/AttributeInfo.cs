using System;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Attributes;

/// <summary>
/// Represents structured information about an attribute of type <typeparamref name="T"/>,
/// providing access to the attribute data in a strongly-typed format for use in source generation or other analyses.
/// </summary>
/// <typeparam name="T">The type of the attribute this instance represents. Must inherit from <see cref="Attribute"/>.</typeparam>
/// <param name="Data">The <see cref="AttributeData"/> instance representing the attribute information.</param>
public record struct AttributeInfo<T>(AttributeData Data) where T : Attribute;
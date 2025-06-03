using System;
using Microsoft.CodeAnalysis;

namespace Retro.SourceGeneratorUtilities.Core.Attributes;

public record struct AttributeInfo<T>(AttributeData Data) where T : Attribute;
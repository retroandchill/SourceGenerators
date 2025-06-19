using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Core.Model.Attributes;

/// <summary>
/// Represents an overview of a child attribute type, encapsulating the model type
/// and the associated attribute type.
/// </summary>
/// <remarks>
/// This struct provides simplified access to the names of the model type and
/// attribute type through the corresponding properties.
/// </remarks>
/// <param name="ModelType">The <see cref="INamedTypeSymbol"/> representing the model type.</param>
/// <param name="AttributeType">The <see cref="INamedTypeSymbol"/> representing the attribute type.</param>
public readonly record struct ChildAttributeTypeInfoOverview(INamedTypeSymbol ModelType, INamedTypeSymbol AttributeType) {
  /// <summary>
  /// Gets the name of the model type.
  /// </summary>
  /// <remarks>
  /// This property retrieves the name of the model type as represented by the <see cref="ModelType"/>
  /// in the <see cref="ChildAttributeTypeInfoOverview"/> struct.
  /// </remarks>
  public string ModelName => ModelType.Name;

  /// <summary>
  /// Gets the name of the attribute type.
  /// </summary>
  /// <remarks>
  /// This property retrieves the name of the attribute type as represented by the <see cref="AttributeType"/>
  /// in the <see cref="ChildAttributeTypeInfoOverview"/> struct.
  /// </remarks>
  public string AttributeName => AttributeType.Name;

  /// <summary>
  /// Gets the type name of the attribute in a format compatible with C# `typeof` syntax.
  /// </summary>
  /// <remarks>
  /// This property retrieves the name of the attribute type within the context of the
  /// <see cref="ChildAttributeTypeInfoOverview"/> struct, leveraging the <see cref="TypeExtensions.GetTypeofName"/> method
  /// to format the type name appropriately for scenarios involving generic types.
  /// </remarks>
  public string AttributeTypeofName => AttributeType.GetTypeofName();

}
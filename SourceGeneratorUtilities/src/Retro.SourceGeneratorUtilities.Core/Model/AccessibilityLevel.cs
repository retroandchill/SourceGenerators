using System.Runtime.Serialization;
namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Specifies the different levels of accessibility that can be applied to a member in a type.
/// </summary>
public enum AccessibilityLevel {
  /// <summary>
  /// Represents the public accessibility level for a type or member.
  /// Indicates that the type or member is accessible from any other code in the same assembly or another assembly that references it.
  /// </summary>
  [EnumMember(Value = "public")]
  Public,

  /// <summary>
  /// Represents the internal accessibility level for a type or member.
  /// Indicates that the type or member is accessible only within its own assembly.
  /// </summary>
  [EnumMember(Value = "internal")]
  Internal,

  /// <summary>
  /// Represents the private accessibility level for a type or member.
  /// Indicates that the type or member is accessible only within the same class or struct.
  /// </summary>
  [EnumMember(Value = "private")]
  Private,

  /// <summary>
  /// Represents the protected accessibility level for a type or member.
  /// Indicates that the type or member is accessible within its own class and by derived class instances.
  /// </summary>
  [EnumMember(Value = "protected")]
  Protected,

  /// <summary>
  /// Represents the protected internal accessibility level for a type or member.
  /// Indicates that the type or member is accessible from the same assembly or
  /// any derived type in another assembly.
  /// </summary>
  [EnumMember(Value = "protected internal")]
  ProtectedInternal
}
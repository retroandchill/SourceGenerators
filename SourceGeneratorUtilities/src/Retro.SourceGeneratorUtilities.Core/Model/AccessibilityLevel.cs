using System.Runtime.Serialization;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public enum AccessibilityLevel {
  [EnumMember(Value = "public")]
  Public,
  
  [EnumMember(Value = "internal")]
  Internal,
  
  [EnumMember(Value = "private")]
  Private,
  
  [EnumMember(Value = "protected")]
  Protected,
  
  [EnumMember(Value = "protected internal")]
  ProtectedInternal
}
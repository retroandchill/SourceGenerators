using System.Runtime.Serialization;
namespace Retro.SourceGeneratorUtilities.Core.Model;

public enum SetterType {
  [EnumMember(Value = "set")]
  Set,
  
  [EnumMember(Value = "init")]
  Init
}
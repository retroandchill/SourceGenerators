using System.Runtime.Serialization;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public enum InitializerType {
  [EnumMember(Value = "base")]
  Base,
  
  [EnumMember(Value = "this")]
  This
}
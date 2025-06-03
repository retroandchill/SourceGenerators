using System.Runtime.Serialization;
using BetterEnumsGen;

namespace Retro.SourceGeneratorUtilities.Core.Model;

[BetterEnum]
public enum InitializerType {
  [EnumMember(Value = "base")]
  Base,
  
  [EnumMember(Value = "this")]
  This
}
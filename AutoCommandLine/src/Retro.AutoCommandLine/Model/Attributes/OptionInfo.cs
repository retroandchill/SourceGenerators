using System.Collections.Immutable;
using Retro.AutoCommandLine.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
namespace Retro.AutoCommandLine.Model.Attributes;

[AttributeInfoType<OptionAttribute>]
public sealed record OptionInfo : CliParameterInfo {
  
  public ImmutableArray<string> Aliases { get; }

  public OptionInfo(string[] aliases) {
    Aliases = [..aliases];
  }
  
}
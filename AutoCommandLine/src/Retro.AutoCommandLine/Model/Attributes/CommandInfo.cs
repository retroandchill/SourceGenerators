using Retro.AutoCommandLine.Core.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
namespace Retro.AutoCommandLine.Model.Attributes;

[AttributeInfoType<CommandAttribute>]
public record struct CommandInfo(string? Name) {
  public string? Description { get; init; }

  public bool IsRootCommand { get; init; }
}
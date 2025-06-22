using System.Collections.Immutable;
using Retro.AutoCommandLine.Core.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
namespace Retro.AutoCommandLine.Model.Attributes;

[AttributeInfoType<CliParameterAttribute>]
public record CliParameterInfo {
  
  public string? Description { get; init; }
}
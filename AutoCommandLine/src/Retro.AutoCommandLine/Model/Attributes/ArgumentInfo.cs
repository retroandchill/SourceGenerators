using System.Collections.Immutable;
using Retro.AutoCommandLine.Core.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
namespace Retro.AutoCommandLine.Model.Attributes;

[AttributeInfoType<ArgumentAttribute>]
public sealed record ArgumentInfo(string? Name) : CliParameterInfo {
}
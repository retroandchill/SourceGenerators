using Retro.AutoCommandLine.Annotations;
namespace Retro.AutoCommandLine.Sample.Commands;

[Command]
public record RootCommandOptions {
  
  public string PositionalParameter { get; init; }
  
  [Option]
  public required int RequiredOption { get; init; }
  
  [Option]
  public bool OptionalOption { get; init; }
}
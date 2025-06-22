namespace Retro.AutoCommandLine.Model;

public record struct OptionAlias {
  public required string Name { get; init; }
  
  public required bool IsLast { get; init; }
}

namespace Retro.AutoCommandLine.Model;

public record OptionBinding {
  public required OptionType Wrapper { get; init; }
  
  public required string Type { get; init; }
  
  public required string Name { get; init; }
  
  public required bool IsLast { get; init; }
}
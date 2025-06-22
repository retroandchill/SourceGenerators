namespace Retro.AutoCommandLine.Model.Commands;

public record OptionBinding {
  public required OptionType Wrapper { get; init; }
  
  public bool IsOption => Wrapper == OptionType.Option;
  
  public required string Type { get; init; }
  
  public required string Name { get; init; }
  
  public required string DisplayName { get; init; }
  
  public required List<OptionAlias> Aliases { get; init; }
  
  public bool HasDescription => Description is not null;
  
  public required string? Description { get; init; }
  
  public required bool IsRequired { get; init; }
  
  public required bool IsLast { get; init; }
}
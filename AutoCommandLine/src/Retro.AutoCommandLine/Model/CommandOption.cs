using System.Collections.Generic;
namespace Retro.AutoCommandLine.Model;

public record CommandOption {
  
  public required string Type { get; init; }
  
  public required string Name { get; init; }
  
  public required List<CommandAlias> Aliases { get; init; }
  
  public required string? Description { get; init; }
  
  public required bool IsLast { get; init; }
  
}
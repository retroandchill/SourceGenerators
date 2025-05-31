namespace Retro.AutoCommandLine.Model;

public record struct CommandAlias {
  public string Name { get; init; }
  public bool IsLast { get; init; }
}
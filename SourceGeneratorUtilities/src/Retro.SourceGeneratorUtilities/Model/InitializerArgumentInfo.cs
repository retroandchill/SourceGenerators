namespace Retro.SourceGeneratorUtilities.Model;

public record struct InitializerArgumentInfo {
  public required string Expression { get; init; }
  
  public required bool IsLast { get; init; }
}
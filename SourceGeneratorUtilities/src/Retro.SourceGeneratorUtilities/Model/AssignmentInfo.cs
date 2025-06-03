namespace Retro.SourceGeneratorUtilities.Model;

public record struct AssignmentInfo {
  public required string Left { get; init; }
  
  public required string Right { get; init; }
}
namespace Retro.SourceGeneratorUtilities.Model;

public record ParameterInfo {
  public required string Type { get; init; }
  
  public required string Name { get; init; }
  
  public required bool HasDefaultValue { get; init; }
  
  public required bool IsLast { get; init; }
  
}
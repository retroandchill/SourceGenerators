using System.Runtime.InteropServices.ComTypes;

namespace Retro.SourceGeneratorUtilities.Core.Model;

public record ParameterOverview(ITypeInfo Type, string Name) {

  public bool HasDefaultValue => DefaultValue is not null;
  
  public DefaultParameterInfo? DefaultValue { get; init; }
  
  public bool IsLast { get; init; }
  
}
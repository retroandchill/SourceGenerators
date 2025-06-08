using System.Runtime.Serialization;

namespace Retro.SourceGeneratorUtilities.Core.Model;

/// <summary>
/// Represents the type of initializer used in a constructor in relation to its base or derived class.
/// </summary>
public enum InitializerType {
  /// <summary>
  /// Represents an initializer type that explicitly invokes the base class constructor
  /// from a derived class constructor.
  /// </summary>
  [EnumMember(Value = "base")]
  Base,

  /// <summary>
  /// Represents an initializer type that invokes another constructor in the same class.
  /// </summary>
  [EnumMember(Value = "this")]
  This
}
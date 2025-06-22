using System;
using System.Threading;
using System.Threading.Tasks;
using Retro.AutoCommandLine.Core;
using Retro.AutoCommandLine.Core.Attributes;
using Retro.AutoCommandLine.Core.Handlers;
namespace Retro.AutoCommandLine.Sample.Commands;

/// <summary>
/// A simple root command
/// </summary>
[Command(IsRootCommand = true)]
public record ProgramRootCommand {
  
  /// <summary>
  /// A simple positional parameter
  /// </summary>
  public string PositionalParameter { get; init; }
  
  /// A required option
  [Option]
  public required int RequiredOption { get; init; }
  
  [Option(Description = "An optional option")]
  public bool OptionalOption { get; init; }

  [CommandHandler]
  public async Task<int> InvokeAsync(CancellationToken cancellationToken = default) {
    await Console.Out.WriteLineAsync("Hello world");
    return 0;
  }
}
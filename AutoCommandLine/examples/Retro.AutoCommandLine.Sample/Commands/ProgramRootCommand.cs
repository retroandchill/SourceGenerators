using System;
using System.Threading;
using System.Threading.Tasks;
using Retro.AutoCommandLine.Core;
using Retro.AutoCommandLine.Core.Attributes;
using Retro.AutoCommandLine.Core.Handlers;
namespace Retro.AutoCommandLine.Sample.Commands;

[Command(HasHandler = true)]
public record ProgramRootCommand {
  
  public string PositionalParameter { get; init; }
  
  [Option]
  public required int RequiredOption { get; init; }
  
  [Option]
  public bool OptionalOption { get; init; }
}

public class ProgramRootCommandHandler : ICommandHandler<ProgramRootCommand> {

  public async Task<int> HandleAsync(ProgramRootCommand options, CancellationToken cancellationToken = default) {
    await Console.Out.WriteLineAsync("Hello world");
    return 0;
  }
}
using System;
using Retro.AutoCommandLine.Attributes;
using Retro.AutoCommandLine.Core.Handlers;
namespace Retro.AutoCommandLine.Sample.Commands;

[CommandLineContext(typeof(ProgramRootCommand))]
public class CommandContext : ICommandLineContext, ICommandLineContext<ProgramRootCommand> {

  public ICommandBinder<TCommand> GetBinder<TCommand>() {
    if (this is ICommandLineContext<TCommand>) {
      return ((ICommandLineContext<TCommand>) this).GetBinder();
    }

    throw new InvalidOperationException($"Cannot get binder for type {typeof(TCommand)}");
  }

  ICommandBinder<ProgramRootCommand> ICommandLineContext<ProgramRootCommand>.GetBinder() {
    throw new NotImplementedException();
  }
}
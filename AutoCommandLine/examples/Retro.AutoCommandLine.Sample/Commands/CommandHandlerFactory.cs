using Retro.AutoCommandLine.Core.Handlers;
namespace Retro.AutoCommandLine.Sample.Commands;

public interface ICommandHandlerFactory<TCommand> {
  ICommandHandler<TCommand> Create();
}

public class CommandHandlerFactory : ICommandHandlerFactory, ICommandHandlerFactory<ProgramRootCommand> {

  public ICommandHandler<TCommand> Create<TCommand>() {
    return ((ICommandHandlerFactory<TCommand>) this).Create();
  }
  
  public ICommandHandler<ProgramRootCommand> Create() {
    return new ProgramRootCommandHandler();
  }
}
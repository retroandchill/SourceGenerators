namespace Retro.AutoCommandLine.Core.Handlers;

public interface ICommandHandlerFactory {

  ICommandHandler<TCommand> Create<TCommand>();

}
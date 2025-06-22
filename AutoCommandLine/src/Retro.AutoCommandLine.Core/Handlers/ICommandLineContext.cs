namespace Retro.AutoCommandLine.Core.Handlers;

public interface ICommandLineContext {

  ICommandBinder<TCommand> GetBinder<TCommand>();

}

public interface ICommandLineContext<out TCommand> {
  ICommandBinder<TCommand> GetBinder();
}
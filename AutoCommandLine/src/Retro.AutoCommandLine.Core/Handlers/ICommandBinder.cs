using System.CommandLine.Invocation;
namespace Retro.AutoCommandLine.Core.Handlers;

public interface ICommandBinder<out TCommand> {
  TCommand Bind(InvocationContext context);
}
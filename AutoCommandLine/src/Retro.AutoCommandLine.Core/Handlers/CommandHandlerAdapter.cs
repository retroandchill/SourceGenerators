using System;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Retro.ReadOnlyParams.Annotations;

namespace Retro.AutoCommandLine.Core.Handlers;

public class CommandHandlerAdapter<TCommand>([ReadOnly] ICommandHandlerFactory factory,
                                             [ReadOnly] ICommandBinder<TCommand> binder) : ICommandHandler {

  
  
  public int Invoke(InvocationContext context) {
    return InvokeAsync(context).GetAwaiter().GetResult();
  }

  public Task<int> InvokeAsync(InvocationContext context) {
    var handler = factory.Create<TCommand>();
    var boundValue = binder.Bind(context);
    return handler.HandleAsync(boundValue, context.GetCancellationToken());
  }
}
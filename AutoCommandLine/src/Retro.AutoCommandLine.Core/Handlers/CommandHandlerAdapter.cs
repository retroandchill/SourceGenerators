using System;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Retro.ReadOnlyParams.Annotations;

namespace Retro.AutoCommandLine.Core.Handlers;

public class CommandHandlerAdapter<TCommand>([ReadOnly] ICommandHandlerFactory factory,
                                             [ReadOnly] BinderBase<TCommand> binder) : ICommandHandler {

  
  
  public int Invoke(InvocationContext context) {
    return InvokeAsync(context).GetAwaiter().GetResult();
  }

  public Task<int> InvokeAsync(InvocationContext context) {
    var handler = factory.Create<TCommand>();
    if (!((IValueSource)binder).TryGetValue(binder, context.BindingContext, out var boundValue)) {
      throw new InvalidOperationException("Could not bind command");
    }

    var options = (TCommand) boundValue!;
    return handler.HandleAsync(options, context.GetCancellationToken());
  }
}
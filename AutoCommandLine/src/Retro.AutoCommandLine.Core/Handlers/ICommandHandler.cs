using System.Threading;
using System.Threading.Tasks;

namespace Retro.AutoCommandLine.Core.Handlers;

public interface ICommandHandler<in TCommand> {
  
  public Task<int> HandleAsync(TCommand options, CancellationToken cancellationToken = default);
  
}
namespace Retro.AutoCommandLine.Model.Commands;

public record HandlerMethodInfo {
 public required string Name { get; init; }
 
 public required HandlerReturnType ReturnType { get; init; }
 
 public required bool HasCancellationToken { get; init; }
 
 public bool ReturnsValue => ReturnType is HandlerReturnType.Int or HandlerReturnType.TaskOfInt;
 
 public bool IsAsync => ReturnType is HandlerReturnType.Task or HandlerReturnType.TaskOfInt;
}
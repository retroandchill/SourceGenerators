using System;
using System.Threading.Tasks;
using Retro.AutoCommandLine.Annotations;
namespace Retro.AutoCommandLine.Sample.Commands;

[RootCommand]
public partial class ProgramRootCommand {

  [HandleCommand]
  public static int HandleCommand() {
    Console.WriteLine("Hello World!");
    return 0;
  }
  
}
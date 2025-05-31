using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
namespace Retro.AutoCommandLine.Utils;

public static class GenerationExtensions {

  public static void AddSource(this SourceProductionContext context, string fileName,  IHandlebars handlebars, string template, object model) {
    context.AddSource(fileName, handlebars.Compile(template)(model));
  }

}
using System.Linq;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.AutoCommandLine.Annotations;
using Retro.AutoCommandLine.Model;
using Retro.AutoCommandLine.Properties;
namespace Retro.AutoCommandLine.Generators;

[Generator]
public class CommandBinderGenerator : IIncrementalGenerator {

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var commands = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => s is ClassDeclarationSyntax or StructDeclarationSyntax or InterfaceDeclarationSyntax or RecordDeclarationSyntax,
            (ctx, _) => {
              var classNode = (TypeDeclarationSyntax)ctx.Node;
              var symbol = ctx.SemanticModel.GetDeclaredSymbol(classNode);

              if (symbol is null || symbol.IsAbstract) {
                return null;
              }

              var hasRootCommandAttribute = symbol.GetAttributes()
                  .Any(attr => attr.AttributeClass?.ToDisplayString() == typeof(CommandAttribute).FullName);

              return hasRootCommandAttribute ? symbol as INamedTypeSymbol : null;
            })
        .Where(m => m is not null);

    var compilationAndClasses = context.CompilationProvider
        .Combine(commands.Collect());

    context.RegisterSourceOutput(compilationAndClasses, (spc, source) => {
      foreach (var classSymbol in source.Right) {
        Execute(source.Left, classSymbol!, spc);
      }
    });
  }

  private static void Execute(Compilation compilation, INamedTypeSymbol classSymbol, SourceProductionContext context) {
    var validProperties = classSymbol.GetMembers()
        .OfType<IPropertySymbol>()
        .Where(x => x.SetMethod is not null && x.SetMethod.DeclaredAccessibility == Accessibility.Public)
        .ToList();
    
    var optionBindings = validProperties
        .Select((x, i) => GetOptionBinding(x, i == validProperties.Count - 1))
        .ToList();

    var templateParams = new {
        Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
        ClassName = classSymbol.Name,
        Options = optionBindings
    };

    var handlebars = Handlebars.Create();
    handlebars.Configuration.TextEncoder = null;
    
    var template = handlebars.Compile(SourceTemplates.CommandBinderTemplate);
    
    var templateResult = template(templateParams);
    
    context.AddSource($"{classSymbol.Name}Binder.g.cs", templateResult);
  }

  private static OptionBinding GetOptionBinding(IPropertySymbol propertySymbol, bool isLast) {
    var optionAttribute = propertySymbol.GetAttributes()
        .Any(x => x.AttributeClass?.ToDisplayString() == typeof(OptionAttribute).FullName);

    return new OptionBinding {
        Wrapper = optionAttribute ? OptionType.Option : OptionType.Argument,
        Type = propertySymbol.Type.ToDisplayString(),
        Name = propertySymbol.Name,
        IsLast = isLast
    };
  } 
}
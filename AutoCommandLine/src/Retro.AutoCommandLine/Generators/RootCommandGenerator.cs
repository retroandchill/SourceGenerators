using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.AutoCommandLine.Annotations;
using Retro.AutoCommandLine.Properties;

namespace Retro.AutoCommandLine.Generators;


[Generator]
public class RootCommandGenerator : IIncrementalGenerator {


  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var rootCommands = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
            (ctx, _) => {
              var classNode = (ClassDeclarationSyntax)ctx.Node;
              var symbol = ModelExtensions.GetDeclaredSymbol(ctx.SemanticModel, classNode);

              if (symbol is null) {
                return null;
              }

              var hasRootCommandAttribute = symbol.GetAttributes()
                  .Any(attr => attr.AttributeClass?.ToDisplayString() == typeof(RootCommandAttribute).FullName);

              return hasRootCommandAttribute ? symbol as INamedTypeSymbol : null;
            })
        .Where(m => m is not null);

    var compilationAndClasses = context.CompilationProvider
        .Combine(rootCommands.Collect());

    context.RegisterSourceOutput(compilationAndClasses, (spc, source) => {
      foreach (var classSymbol in source.Right) {
        Execute(source.Left, classSymbol!, spc);
      }
    });
  }

  private static void Execute(Compilation compilation, INamedTypeSymbol classSymbol, SourceProductionContext context) {
    if (!classSymbol.DeclaringSyntaxReferences
            .Any(x => x.GetSyntax() is ClassDeclarationSyntax classDeclaration
                      && classDeclaration.Modifiers.Any(y => y.IsKind(SyntaxKind.PartialKeyword)))) {
      context.ReportDiagnostic(
          Diagnostic.Create(new DiagnosticDescriptor(
                  "AutoCommandLine001",
                  "Auto Command Line Error",
                  $"Class {classSymbol.Name} must be declared partial",
                  "Command Line",
                  DiagnosticSeverity.Error,
                  true
              ),
              classSymbol.Locations.FirstOrDefault()
          ));
      return;
    }

    var rootCommandAttribute = classSymbol.GetAttributes()
        .Single(x => x.AttributeClass?.ToDisplayString() == typeof(RootCommandAttribute).FullName);
    
    var description = rootCommandAttribute.NamedArguments
        .Where(x => x.Key == "Description")
        .Select(x => x.Value.Value as string)
        .FirstOrDefault();

    var templateParams = new {
        Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
        ClassName = classSymbol.Name,
        HasDescription = description is not null,
        Description = description,
        HasHandler = GetHandleCommandMethod(classSymbol, context, out var handleCommandMethod),
        MethodName = handleCommandMethod?.Name
    };
    
    
    var handlebars = Handlebars.Create();
    handlebars.Configuration.TextEncoder = null;
    
    var template = handlebars.Compile(SourceTemplates.RootCommandTemplate);
    
    var templateResult = template(templateParams);
    
    context.AddSource($"{classSymbol.Name}.g.cs", templateResult);
  }

  private static bool GetHandleCommandMethod(INamedTypeSymbol classSymbol, SourceProductionContext context, 
                                             [NotNullWhen(true)] out IMethodSymbol? handleCommandMethod) {
    try {
      handleCommandMethod = classSymbol.GetMembers()
          .OfType<IMethodSymbol>()
          .SingleOrDefault(x => x.GetAttributes()
              .Any(y => y.AttributeClass?.ToDisplayString() == typeof(HandleCommandAttribute).FullName));
      return handleCommandMethod is not null;
    } catch (InvalidOperationException) {
      handleCommandMethod = null;
      context.ReportDiagnostic(
          Diagnostic.Create(new DiagnosticDescriptor(
                  "AutoCommandLine002",
                  "Auto Command Line Error",
                  $"Class {classSymbol.Name} may only have one method annotated with {nameof(HandleCommandAttribute)}",
                  "Command Line",
                  DiagnosticSeverity.Error,
                  true
              ),
              classSymbol.Locations.FirstOrDefault()
          ));
      return false;
    }
  }

}
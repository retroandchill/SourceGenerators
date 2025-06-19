using System.Collections.Immutable;
using System.Linq;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Formatters;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Errors;
using Retro.SourceGeneratorUtilties.Generator.Properties;

namespace Retro.SourceGeneratorUtilities.Generators;

[Generator]
public class AttributeInfoGenerator : IIncrementalGenerator {
  
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var validTypes = context.SyntaxProvider.CreateSyntaxProvider(
        (s, _) => s is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
        (ctx, _) => {
          var declarationSyntax = (TypeDeclarationSyntax)ctx.Node;
          return ctx.SemanticModel.GetDeclaredSymbol(declarationSyntax);
        })
        .Where(t => t?.HasAttribute<AttributeInfoTypeAttribute>() ?? false)
        .Collect();
    
    context.RegisterSourceOutput(validTypes, Execute);
  }

  private static void Execute(SourceProductionContext context, ImmutableArray<INamedTypeSymbol?> types) {
    var allTypes = types.OfType<INamedTypeSymbol>().ToImmutableArray();
    
    var allClassSymbols = allTypes
        .Select(x => x.ExtractAttributeInfoTypeOverview(allTypes))
        .Where(result => !context.ReportDiagnostics(result))
        .Select(r => r.Result)
        .ToImmutableArray();
    foreach (var templateParams in allClassSymbols) {
      var handlebars = Handlebars.Create();
      handlebars.Configuration.TextEncoder = null;
      handlebars.Configuration.FormatterProviders.Add(new EnumStringValueFormatter());

      var template = handlebars.Compile(SourceTemplates.AttributeInfoTemplate);

      var templateResult = template(templateParams);
      context.AddSource($"{templateParams.Name}.g.cs", templateResult);
    }
  }
}
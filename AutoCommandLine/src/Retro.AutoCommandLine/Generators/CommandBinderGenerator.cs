using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CaseConverter;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.AutoCommandLine.Core;
using Retro.AutoCommandLine.Core.Attributes;
using Retro.AutoCommandLine.Core.Handlers;
using Retro.AutoCommandLine.Model;
using Retro.AutoCommandLine.Properties;
using Retro.AutoCommandLine.Utils;
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

              return hasRootCommandAttribute ? symbol : null;
            })
        .Where(m => m is not null)
        .Collect();

    var handlers = context.SyntaxProvider.CreateSyntaxProvider(
        (s, _) => s is ClassDeclarationSyntax or StructDeclarationSyntax,
        (ctx, _) => {
          var classNode = (TypeDeclarationSyntax)ctx.Node;
          var symbol = ctx.SemanticModel.GetDeclaredSymbol(classNode);
          return symbol?.AllInterfaces
              .Where(x => x.IsGenericType && x.ConstructedFrom.GetMetadataName() == typeof(ICommandHandler<>).FullName)
              .Select(x => new {
                  ClassSymbol = symbol,
                  BoundType = x.TypeArguments[0]
              })
              .FirstOrDefault();
        })
        .Where(m => m is not null)
        .Collect();
    
    var combinedTypes = commands.Combine(handlers);

    context.RegisterSourceOutput(combinedTypes, (spc, source) => {
      var handlerTypes = source.Right
          .GroupBy(x => x!.BoundType, SymbolEqualityComparer.Default)
          .ToDictionary(x => x.Key, x => x
              .Select(y => y!.ClassSymbol)
              .First(), SymbolEqualityComparer.Default);
      
      foreach (var classSymbol in source.Left) {
        handlerTypes.TryGetValue(classSymbol!, out var handlerClassSymbol);
        GenerateIndividualHandlers(classSymbol!, handlerClassSymbol, spc);
      }
    });
  }

  private static void GenerateIndividualHandlers(INamedTypeSymbol classSymbol, INamedTypeSymbol? handlerType, SourceProductionContext context) {
    var commandAttribute = classSymbol.GetAttributes()
        .Single(x => x.AttributeClass?.ToDisplayString() == typeof(CommandAttribute).FullName);
    
    var validProperties = classSymbol.GetMembers()
        .OfType<IPropertySymbol>()
        .Where(x => x.SetMethod is not null && x.SetMethod.DeclaredAccessibility == Accessibility.Public)
        .ToList();
    
    var optionBindings = validProperties
        .Select((x, i) => GetOptionBinding(x, i == validProperties.Count - 1))
        .ToList();

    var description = commandAttribute.NamedArguments
        .FirstOrDefault(x => x.Key == nameof(CommandAttribute.Description)).Value.Value?.ToString()
        ?? classSymbol.GetDocumentationCommentXml().GetSummaryTag();
    
    var templateParams = new {
        Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
        ClassName = classSymbol.Name,
        IsRootCommand = commandAttribute.NamedArguments.Any(x => x is {
            Key: nameof(CommandAttribute.IsRootCommand),
            Value.Value: true
        }),
        CommandName = commandAttribute.ConstructorArguments.Select(x => x.Value).Cast<string>().FirstOrDefault() ?? classSymbol.Name,
        HasDescription = description is not null,
        Description = description is not null ? SymbolDisplay.FormatLiteral(description, true) : null,
        Options = optionBindings,
        HasHandler = handlerType is not null
    };

    var handlebars = Handlebars.Create();
    handlebars.Configuration.TextEncoder = null;
    
    context.AddSource($"{classSymbol.Name}Binder.g.cs", handlebars, SourceTemplates.CommandBinderTemplate, templateParams);
    context.AddSource($"{classSymbol.Name}Factory.g.cs", handlebars, SourceTemplates.CommandFactoryTemplate, templateParams);
  }

  private static OptionBinding GetOptionBinding(IPropertySymbol propertySymbol, bool isLast) {
    var optionAttribute = propertySymbol.GetAttributes()
        .Where(x => x.AttributeClass?.ToDisplayString() == typeof(OptionAttribute).FullName || x.AttributeClass?.ToDisplayString() == typeof(ArgumentAttribute).FullName)
        .Select(x => new {
            IsOption = x.AttributeClass?.ToDisplayString() == typeof(OptionAttribute).FullName,
            Description = x.NamedArguments.FirstOrDefault(y => y.Key == nameof(OptionAttribute.Description)).Value.Value as string,
            Aliases = GetAliases(propertySymbol, x)
        })
        .DefaultIfEmpty(new { IsOption = false, Description = (string?) null, Aliases = new List<OptionAlias>() })
        .FirstOrDefault();
    
    var description = optionAttribute!.Description ?? propertySymbol.GetDocumentationCommentXml().GetSummaryTag();

    return new OptionBinding {
        Wrapper = optionAttribute!.IsOption ? OptionType.Option : OptionType.Argument,
        Type = propertySymbol.Type.ToDisplayString(),
        Name = propertySymbol.Name,
        DisplayName = optionAttribute.IsOption ? $"--{propertySymbol.Name.ToKebabCase()}" : propertySymbol.Name,
        Aliases = optionAttribute.Aliases,
        Description = description is not null ? SymbolDisplay.FormatLiteral(description, true) : null,
        IsRequired = optionAttribute.IsOption && IsPropertyRequired(propertySymbol),
        IsLast = isLast
    };
  }

  private static List<OptionAlias> GetAliases(IPropertySymbol propertySymbol, AttributeData optionAttribute) {
    if (optionAttribute.AttributeClass?.ToDisplayString() != typeof(OptionAttribute).FullName) {
      return [];
    }

    var aliases = optionAttribute.ConstructorArguments[0].Values;
    if (aliases.Length == 0) {
      return [
          new OptionAlias {
              Name = $"--{propertySymbol.Name.ToKebabCase()}",
              IsLast = true
          }
      ];
    }
    
    return aliases
        .Select(x => x.Value)
        .Cast<string>()
        .Select((x, i) => new OptionAlias {
            Name = x,
            IsLast = i == aliases.Length - 1
        })
        .ToList();
  }
  
  private static bool IsPropertyRequired(IPropertySymbol propertySymbol) {
    if (propertySymbol.IsRequired) {
      return true;
    }

    if (propertySymbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == typeof(DefaultValueAttribute).FullName)) {
      return true;
    }
    
    var propertyType = propertySymbol.Type;
    return propertyType is {
        IsReferenceType: true,
        NullableAnnotation: NullableAnnotation.NotAnnotated
    };
  }
}
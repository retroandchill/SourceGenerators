using System.ComponentModel;
using CaseConverter;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.AutoCommandLine.Attributes;
using Retro.AutoCommandLine.Model.Attributes;
using Retro.AutoCommandLine.Model.Commands;
using Retro.AutoCommandLine.Properties;
using Retro.AutoCommandLine.Utils;
using Retro.SourceGeneratorUtilities.Utilities;
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

    context.RegisterSourceOutput(commands, (spc, source) => {
      foreach (var classSymbol in source) {
        GenerateIndividualHandlers(classSymbol!, spc);
      }
    });
  }

  private static void GenerateIndividualHandlers(INamedTypeSymbol classSymbol, SourceProductionContext context) {
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
    
    var handlerMethod = GetHandlerMethod(classSymbol, context);
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
        Handler = ValidateHandlerMethod(handlerMethod, context),
    };

    var handlebars = Handlebars.Create();
    handlebars.Configuration.TextEncoder = null;
    
    context.AddSource($"{classSymbol.Name}Binder.g.cs", handlebars, SourceTemplates.CommandBinderTemplate, templateParams);
    context.AddSource($"{classSymbol.Name}Factory.g.cs", handlebars, SourceTemplates.CommandFactoryTemplate, templateParams);
  }

  private static OptionBinding GetOptionBinding(IPropertySymbol propertySymbol, bool isLast) {
    var optionAttribute = propertySymbol.GetAttributes()
        .GetCliParameterInfos()
        .Select(x => new {
            IsOption = x is OptionInfo,
            x.Description,
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

  private static List<OptionAlias> GetAliases(IPropertySymbol propertySymbol, CliParameterInfo optionAttribute) {
    if (optionAttribute is not OptionInfo optionInfo) {
      return [];
    }

    if (optionInfo.Aliases.Length == 0) {
      return [
          new OptionAlias {
              Name = $"--{propertySymbol.Name.ToKebabCase()}",
              IsLast = true
          }
      ];
    }
    
    return optionInfo.Aliases
        .Select((x, i) => new OptionAlias {
            Name = x,
            IsLast = i == optionInfo.Aliases.Length - 1
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
  
  private static IMethodSymbol? GetHandlerMethod(INamedTypeSymbol classSymbol, SourceProductionContext context) {
    try {
      return classSymbol
          .GetMembers()
          .OfType<IMethodSymbol>()
          .Where(x => x.DeclaredAccessibility == Accessibility.Public)
          .SingleOrDefault(x => x.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == typeof(CommandHandlerAttribute).FullName));
    } catch (InvalidOperationException) {
      context.ReportDiagnostic(Diagnostic.Create(
          new DiagnosticDescriptor(
              "CMD0001",
              "Command has multiple handlers",
              "Command has multiple handlers. Please use only one handler method.",
              "Retro.AutoCommandLine",
              DiagnosticSeverity.Error,
              true),
          Location.None));
      return null;
    }
  }
  
  private static HandlerMethodInfo? ValidateHandlerMethod(IMethodSymbol? handlerMethod, SourceProductionContext context) {
    if (handlerMethod is null) {
      return null;
    }

    var returnType = ValidateHandlerMethodReturnType(handlerMethod, context);
    if (!returnType.HasValue) {
      return null;
    }

    if (!ValidateHandlerMethodParameters(handlerMethod, context)) {
      return null;
    }

    return new HandlerMethodInfo {
        Name = handlerMethod.Name,
        ReturnType = returnType.Value,
        HasCancellationToken = handlerMethod.Parameters.Length == 1
    };
  }

  private static HandlerReturnType? ValidateHandlerMethodReturnType(IMethodSymbol handlerMethod, SourceProductionContext context) {
    if (handlerMethod.ReturnsVoid) {
      return HandlerReturnType.Void;
    }
    
    var returnType = handlerMethod.ReturnType;
    if (returnType.SpecialType == SpecialType.System_Int32) {
      return HandlerReturnType.Int;
    }

    var typeName = returnType.ToDisplayString();
    if (typeName == typeof(Task).FullName) {
      return HandlerReturnType.Task;
    }
    
    if (typeName == $"{typeof(Task).FullName}<int>") {
      return HandlerReturnType.TaskOfInt;
    }

    
    context.ReportDiagnostic(Diagnostic.Create(
        new DiagnosticDescriptor(
            "CMD0002",
            "Command return type invalid",
            $"The command handler has an invalid return type of {typeName}. Please return either void, int, Task, or Task<int>.",
            "Retro.AutoCommandLine",
            DiagnosticSeverity.Error,
            true),
        Location.None));
    return null;
  }

  private static bool ValidateHandlerMethodParameters(IMethodSymbol handlerMethod, SourceProductionContext context) {
    if (handlerMethod.ReturnsVoid || handlerMethod.ReturnType.SpecialType == SpecialType.System_Int32) {
      if (handlerMethod.Parameters.Length <= 0) return true;

      context.ReportDiagnostic(Diagnostic.Create(
          new DiagnosticDescriptor(
              "CMD0003",
              "Command has invalid parameters",
              "The command handler has invalid parameters. For a return type of void or int, please use no parameters.",
              "Retro.AutoCommandLine",
              DiagnosticSeverity.Error,
              true),
          Location.None));
      return false;
    }

    if (handlerMethod.Parameters.Length > 1 || (handlerMethod.Parameters.Length == 1 && handlerMethod.Parameters[0].ToDisplayString() == typeof(CancellationToken).FullName)) {
      context.ReportDiagnostic(Diagnostic.Create(
          new DiagnosticDescriptor(
              "CMD0003",
              "Command has invalid parameters",
              "The command handler has invalid parameters. For a return type of Task or Task<int>, you may have at least one parameter of type CancellationToken.",
              "Retro.AutoCommandLine",
              DiagnosticSeverity.Error,
              true),
          Location.None));
      return false;
    }

    return true;
  }
}
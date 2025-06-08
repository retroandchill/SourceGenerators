using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Core.Attributes;
using Retro.SourceGeneratorUtilities.Core.Members;
using Retro.SourceGeneratorUtilities.Core.Model;
using Retro.SourceGeneratorUtilities.Core.Types;
using Retro.SourceGeneratorUtilities.Formatters;
using Retro.SourceGeneratorUtilties.Generator.Properties;

namespace Retro.SourceGeneratorUtilities.Generators;

[Generator]
public class AttributeInfoGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    context.RegisterSourceOutput(context.CompilationProvider, Execute);
  }

  private static void Execute(SourceProductionContext context, Compilation compilation) {
    var allClassSymbols = compilation.Assembly.GetAttributeInfoTypes().GetDataClassOverviews();
    foreach (var initializer in allClassSymbols) {
      var classSymbol = initializer.Value;
      var templateParams = new {
          Namespace = classSymbol.Namespace.ToDisplayString(),
          AttributeName = classSymbol.Name,
          HasParentClass = classSymbol.Base is not null && !classSymbol.Base.Symbol.IsSameType<Attribute>(),
          ParentAttribute = classSymbol.Base?.Symbol.ToDisplayString(),
          Constructors = classSymbol.Constructors
              .Select(x => ConvertToTypeMetadata(x, compilation))
              .ToImmutableList(),
          Properties = classSymbol.Properties
              .Select(x => ConvertToTypeMetadata(x, compilation))
              .ToImmutableList()
      };

      var handlebars = Handlebars.Create();
      handlebars.Configuration.TextEncoder = null;
      handlebars.Configuration.FormatterProviders.Add(new EnumStringValueFormatter());

      var template = handlebars.Compile(SourceTemplates.AttributeInfoTemplate);

      var templateResult = template(templateParams);
      context.AddSource($"{classSymbol.Name}Info.g.cs", templateResult);
    }
  }

  private static ConstructorOverview ConvertToTypeMetadata(ConstructorOverview constructorOverview, Compilation compilation) {
    return constructorOverview with {
        Assignments = constructorOverview.Assignments
            .Select(x => x.PropertyType.IsSameType<Type>() ? x with {
                PropertyType = compilation.GetNamedType<ITypeSymbol>(),
                Right = x.Right is TypeOfExpressionSyntax typeOfExpression ? ConvertToCompilationFetchExpression(typeOfExpression, compilation) : x.Right
            } : x)
            .ToImmutableList()
    };
  }
  
  private static PropertyOverview ConvertToTypeMetadata(PropertyOverview propertyOverview, Compilation compilation) {
    return propertyOverview with {
        Type = propertyOverview.Type.IsSameType<Type>() ? compilation.GetNamedType<ITypeSymbol>() : propertyOverview.Type,
    };
  }

  private static ExpressionSyntax ConvertToCompilationFetchExpression(TypeOfExpressionSyntax expression, Compilation compilation) {
    var typeArg = expression.Type;

    return SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("compilation"),
            SyntaxFactory.IdentifierName(nameof(TypeExtensions.GetNamedType))),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expression))));

  }
}
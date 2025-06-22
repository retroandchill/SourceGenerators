using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Retro.ReadOnlyParams.Annotations;

namespace Retro.ReadOnlyParams;

/// <summary>
/// Analyzes C# code to detect modifications to parameters marked with the [ReadOnly] attribute.
/// </summary>
/// <remarks>
/// This analyzer ensures that any parameter explicitly marked with the [ReadOnly] attribute
/// is not reassigned or incremented/decremented in any part of the codebase.
/// </remarks>
/// <example>
/// Diagnostics are reported for reassignment, compound assignment, or increment/decrement
/// operations on parameters indicated as read-only.
/// </example>
/// <seealso cref="DiagnosticAnalyzer" />
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReadonlyParameterSemanticAnalyzer : DiagnosticAnalyzer {
  private const string DiagnosticId = "RRP001";
  private const string Title = "Read-only parameter reassigned";
  private const string MessageFormat = "Parameter '{0}' is marked as read-only and cannot be reassigned";
  private const string Description = "Parameters marked with [ReadOnly] attribute should not be reassigned.";
  private const string Category = "Usage";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      Title,
      MessageFormat,
      Category,
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: Description);

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
      ImmutableArray.Create(Rule);

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context) {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
    context.RegisterOperationAction(AnalyzeAssignment, OperationKind.CompoundAssignment);
    context.RegisterOperationAction(AnalyzeUnaryOperation, OperationKind.Increment);
    context.RegisterOperationAction(AnalyzeUnaryOperation, OperationKind.Decrement);
  }

  private static void AnalyzeAssignment(OperationAnalysisContext context) {
    if (context.Operation is not IAssignmentOperation assignment) return;
    if (assignment.Target is not IParameterReferenceOperation parameterReference) return;
    if (!IsReadOnlyParameter(parameterReference.Parameter)) return;
    var diagnostic = Diagnostic.Create(Rule, assignment.Syntax.GetLocation(), parameterReference.Parameter.Name);
    context.ReportDiagnostic(diagnostic);
  }

  private static void AnalyzeUnaryOperation(OperationAnalysisContext context) {
    if (context.Operation is not IIncrementOrDecrementOperation unaryOperation) return;
    if (unaryOperation.Target is not IParameterReferenceOperation parameterReference) return;
    if (!IsReadOnlyParameter(parameterReference.Parameter)) return;
    var diagnostic = Diagnostic.Create(Rule, unaryOperation.Syntax.GetLocation(), parameterReference.Parameter.Name);
    context.ReportDiagnostic(diagnostic);
  }

  private static bool IsReadOnlyParameter(IParameterSymbol parameter) {
    return parameter.GetAttributes()
        .Any(attr => attr.AttributeClass != null &&
                     attr.AttributeClass.ToDisplayString() == typeof(ReadOnlyAttribute).FullName);
  }
}
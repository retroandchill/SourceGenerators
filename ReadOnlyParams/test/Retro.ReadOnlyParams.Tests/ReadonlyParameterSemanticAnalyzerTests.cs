using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Retro.ReadOnlyParams.Annotations;

namespace Retro.ReadOnlyParams.Tests;

public class ReadonlyParameterSemanticAnalyzerTests {
  private static readonly DiagnosticResult ExpectedDiagnostic =
      VerifyCS.Diagnostic("RRP001")
          .WithSeverity(DiagnosticSeverity.Error);

  [Test]
  public async Task SimpleAssignment_ReadOnlyParameterModified_ReportsDiagnostic() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void SetSpeed([ReadOnly] long speed) 
    {
        speed = 100; // This should report a diagnostic
    }
}";

    await VerifyAnalyzerAsync(test,
                              ExpectedDiagnostic.WithLocation(8, 9).WithArguments("speed"));
  }

  [Test]
  public async Task CompoundAssignment_ReadOnlyParameterModified_ReportsDiagnostic() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void AdjustSpeed([ReadOnly] long speed) 
    {
        speed += 50; // This should report a diagnostic
    }
}";

    await VerifyAnalyzerAsync(test,
                              ExpectedDiagnostic.WithLocation(8, 9).WithArguments("speed"));
  }

  [Test]
  public async Task Increment_ReadOnlyParameterModified_ReportsDiagnostic() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void IncrementSpeed([ReadOnly] int speed) 
    {
        speed++; // This should report a diagnostic
    }
}";

    await VerifyAnalyzerAsync(test,
                              ExpectedDiagnostic.WithLocation(8, 9).WithArguments("speed"));
  }

  [Test]
  public async Task Decrement_ReadOnlyParameterModified_ReportsDiagnostic() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void DecrementSpeed([ReadOnly] int speed) 
    {
        speed--; // This should report a diagnostic
    }
}";

    await VerifyAnalyzerAsync(test,
                              ExpectedDiagnostic.WithLocation(8, 9).WithArguments("speed"));
  }

  [Test]
  public async Task MultipleAssignments_ReadOnlyParameterModified_ReportsMultipleDiagnostics() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void ModifySpeed([ReadOnly] int speed) 
    {
        speed = 100; // First diagnostic
        speed += 50; // Second diagnostic
        speed++; // Third diagnostic
    }
}";

    await VerifyAnalyzerAsync(test,
                              ExpectedDiagnostic.WithLocation(8, 9).WithArguments("speed"),
                              ExpectedDiagnostic.WithLocation(9, 9).WithArguments("speed"),
                              ExpectedDiagnostic.WithLocation(10, 9).WithArguments("speed"));
  }

  [Test]
  public async Task ReadingParameter_ReadOnlyParameter_NoDiagnosticReported() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;
using System;

public class Spaceship
{
    public void UseSpeed([ReadOnly] int speed) 
    {
        var doubled = speed * 2; // Just reading, no diagnostic
        Console.WriteLine(speed); // Just reading, no diagnostic
    }
}";

    await VerifyAnalyzerAsync(test);
  }

  [Test]
  public async Task ModifyingRegularParameter_NoDiagnosticReported() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void ModifyRegularSpeed(int speed) // No ReadOnly attribute
    {
        speed = 100; // No diagnostic should be reported
        speed += 50;
        speed++;
    }
}";

    await VerifyAnalyzerAsync(test);
  }

  [Test]
  public async Task ModifyingLocalVariable_NoDiagnosticReported() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void ProcessSpeed([ReadOnly] int originalSpeed) 
    {
        int localSpeed = originalSpeed;
        localSpeed = 100; // No diagnostic for local variable
        localSpeed++;
    }
}";

    await VerifyAnalyzerAsync(test);
  }

  [Test]
  public async Task ReadOnlyAttributeOnMultipleParameters_OnlyModifiedOnesReportDiagnostic() {
    const string test = @"
using Retro.ReadOnlyParams.Annotations;

public class Spaceship
{
    public void ConfigureShip([ReadOnly] int speed, [ReadOnly] string name, [ReadOnly] bool engaged) 
    {
        speed = 200; // Should report diagnostic
        // name is not modified, no diagnostic
        bool status = engaged; // Just reading, no diagnostic
    }
}";

    await VerifyAnalyzerAsync(test,
                              ExpectedDiagnostic.WithLocation(8, 9).WithArguments("speed"));
  }

  private static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected) {
    const string attributeDefinition = """
                                       namespace Retro.ReadOnlyParams.Annotations
                                       {
                                           [System.AttributeUsage(System.AttributeTargets.Parameter)]
                                           public class ReadOnlyAttribute : System.Attribute { }
                                       }
                                       """;

    
    var test = new CSharpAnalyzerTest<ReadonlyParameterSemanticAnalyzer, DefaultVerifier> {
        TestCode = source,
        ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
        TestState = {
            AdditionalReferences = {
                MetadataReference.CreateFromFile(typeof(ReadOnlyAttribute).Assembly.Location)
            },
            Sources = { attributeDefinition }
        }
    };
    test.ExpectedDiagnostics.AddRange(expected);

    return test.RunAsync();
  }
}
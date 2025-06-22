global using NUnit.Framework;
global using Microsoft.CodeAnalysis.CSharp.Testing;
global using Microsoft.CodeAnalysis.Testing;

namespace Retro.ReadOnlyParams.Tests;

internal class VerifyCS : CSharpAnalyzerVerifier<ReadonlyParameterSemanticAnalyzer, DefaultVerifier>
{
}

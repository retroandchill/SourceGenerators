using Retro.SourceGeneratorUtilities.Core.Types;

namespace Retro.SourceGeneratorUtilities.Test.Types;

public class TypeExtensionsTest {

  [Test]
  public void TestToDisplayString() {
    Assert.Multiple(() => {
      Assert.That(typeof(void).ToDisplayString(), Is.EqualTo("void"));
      Assert.That(typeof(bool).ToDisplayString(), Is.EqualTo("bool"));
      Assert.That(typeof(byte).ToDisplayString(), Is.EqualTo("byte"));
      Assert.That(typeof(sbyte).ToDisplayString(), Is.EqualTo("sbyte"));
      Assert.That(typeof(char).ToDisplayString(), Is.EqualTo("char"));
      Assert.That(typeof(decimal).ToDisplayString(), Is.EqualTo("decimal"));
      Assert.That(typeof(double).ToDisplayString(), Is.EqualTo("double"));
      Assert.That(typeof(float).ToDisplayString(), Is.EqualTo("float"));
      Assert.That(typeof(int).ToDisplayString(), Is.EqualTo("int"));
      Assert.That(typeof(uint).ToDisplayString(), Is.EqualTo("uint"));
      Assert.That(typeof(long).ToDisplayString(), Is.EqualTo("long"));
      Assert.That(typeof(ulong).ToDisplayString(), Is.EqualTo("ulong"));
      Assert.That(typeof(short).ToDisplayString(), Is.EqualTo("short"));
      Assert.That(typeof(ushort).ToDisplayString(), Is.EqualTo("ushort"));
      Assert.That(typeof(string).ToDisplayString(), Is.EqualTo("string"));
      Assert.That(typeof(object).ToDisplayString(), Is.EqualTo("object"));
      Assert.That(typeof(Type).ToDisplayString(), Is.EqualTo("System.Type"));

      Assert.That(typeof(List<>).ToDisplayString(), Is.EqualTo("System.Collections.Generic.List<T>"));
      Assert.That(typeof(List<int>).ToDisplayString(), Is.EqualTo("System.Collections.Generic.List<int>"));
      
      
      Assert.That(typeof(int[]).ToDisplayString(), Is.EqualTo("int[]"));
    });
  }
  
}
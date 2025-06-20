using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using Retro.SourceGeneratorUtilities.Test.Utils;

namespace Retro.SourceGeneratorUtilities.Test.Attributes;

public class AttributeExtensionsTest {
  [Test]
  public void HasAttribute_WithTypeParameter_ReturnsTrueForMatchingAttribute() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;

        namespace TestNamespace {
            [AttributeUsage(AttributeTargets.Class)]
            public class TestAttribute : Attribute { }

            [Test]
            public class ClassWithAttribute { }

            public class ClassWithoutAttribute { }
        }
        """);

    var classWithAttribute = compilation.GetTypeSymbol("TestNamespace.ClassWithAttribute");
    var classWithoutAttribute = compilation.GetTypeSymbol("TestNamespace.ClassWithoutAttribute");
    var testAttributeType = typeof(System.Attribute);

    Assert.Multiple(() => {
      // Act & Assert
      Assert.That(classWithAttribute.HasAttribute(testAttributeType), Is.True,
                  "Class with attribute should return true");
      Assert.That(classWithoutAttribute.HasAttribute(testAttributeType), Is.False,
                  "Class without attribute should return false");
    });
  }

  [Test]
  public void HasAttribute_WithGenericTypeParameter_ReturnsTrueForMatchingAttribute() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;

        namespace TestNamespace {
            [AttributeUsage(AttributeTargets.Class)]
            public class TestAttribute : Attribute { }

            [Test]
            public class ClassWithAttribute { }

            public class ClassWithoutAttribute { }
        }
        """);

    var classWithAttribute = compilation.GetTypeSymbol("TestNamespace.ClassWithAttribute");
    var classWithoutAttribute = compilation.GetTypeSymbol("TestNamespace.ClassWithoutAttribute");

    // Act & Assert
    // Note: This test is limited as we can't actually use the type TestAttribute in the generic parameter
    // since it's dynamically created in the compilation, so we're testing with a known attribute type
    Assert.That(classWithAttribute.HasAttribute<Attribute>(), Is.True,
                "Class with attribute should return true");
    Assert.That(classWithoutAttribute.HasAttribute<Attribute>(), Is.False,
                "Class without attribute should return false");
  }

  [Test]
  public void HasMatchingConstructor_ValidatesConstructorParameterTypes() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;

        namespace TestNamespace {
            [AttributeUsage(AttributeTargets.Class)]
            public class StringParamAttribute : Attribute {
                public StringParamAttribute(string value) { }
            }

            [AttributeUsage(AttributeTargets.Class)]
            public class IntParamAttribute : Attribute {
                public IntParamAttribute(int value) { }
            }

            [AttributeUsage(AttributeTargets.Class)]
            public class MultiParamAttribute : Attribute {
                public MultiParamAttribute(string value1, int value2) { }
            }

            [StringParam("test")]
            [IntParam(42)]
            [MultiParam("test", 42)]
            public class ClassWithAttributes { }
        }
        """);

    var classWithAttributes = compilation.GetTypeSymbol("TestNamespace.ClassWithAttributes");
    var attributes = classWithAttributes.GetAttributes();

    var stringParamAttr = attributes.Single(a => a.AttributeClass?.Name == "StringParamAttribute");
    var intParamAttr = attributes.Single(a => a.AttributeClass?.Name == "IntParamAttribute");
    var multiParamAttr = attributes.Single(a => a.AttributeClass?.Name == "MultiParamAttribute");

    Assert.Multiple(() => {
      // Act & Assert
      Assert.That(stringParamAttr.HasMatchingConstructor(typeof(string)), Is.True,
                  "StringParamAttribute should match constructor with string parameter");
      Assert.That(stringParamAttr.HasMatchingConstructor(typeof(int)), Is.False,
                  "StringParamAttribute should not match constructor with int parameter");

      Assert.That(intParamAttr.HasMatchingConstructor(typeof(int)), Is.True,
                  "IntParamAttribute should match constructor with int parameter");
      Assert.That(intParamAttr.HasMatchingConstructor(typeof(string)), Is.False,
                  "IntParamAttribute should not match constructor with string parameter");

      Assert.That(multiParamAttr.HasMatchingConstructor(typeof(string), typeof(int)), Is.True,
                  "MultiParamAttribute should match constructor with string, int parameters");
      Assert.That(multiParamAttr.HasMatchingConstructor(typeof(int), typeof(string)), Is.False,
                  "MultiParamAttribute should not match constructor with reversed parameter types");
      Assert.That(multiParamAttr.HasMatchingConstructor(typeof(string)), Is.False,
                  "MultiParamAttribute should not match constructor with fewer parameters");
    });
  }

  [Test]
  public void GetUsageInfo_ReturnsDefaultInfoWhenNoAttributeUsagePresent() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;

        namespace TestNamespace {
            // No AttributeUsage attribute
            public class BasicAttribute : Attribute { }

            [Basic]
            public class ClassWithAttribute { }
        }
        """);

    var classWithAttributes = compilation.GetTypeSymbol("TestNamespace.ClassWithAttribute");
    var attribute = classWithAttributes.GetAttributes().Single();

    // Act
    var usageInfo = attribute.GetUsageInfo();

    // Assert
    Assert.Multiple(() => {
      Assert.That(usageInfo.ValidOn, Is.EqualTo(AttributeTargets.All));
      Assert.That(usageInfo.AllowMultiple, Is.False);
      Assert.That(usageInfo.Inherited, Is.False);
    });
  }

  [Test]
  public void GetUsageInfo_ReturnsCorrectInfoForAttributeWithUsage() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;

        namespace TestNamespace {
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
            public class CustomAttribute : Attribute { }

            [Custom]
            public class ClassWithAttribute { }
        }
        """);

    var classWithAttributes = compilation.GetTypeSymbol("TestNamespace.ClassWithAttribute");
    var attribute = classWithAttributes.GetAttributes().Single();

    // Act
    var usageInfo = attribute.GetUsageInfo();

    Assert.Multiple(() => {
      // Assert
      Assert.That(usageInfo.ValidOn, Is.EqualTo(AttributeTargets.Class | AttributeTargets.Method));
      Assert.That(usageInfo.AllowMultiple, Is.True);
      Assert.That(usageInfo.Inherited, Is.True);
    });
  }
}
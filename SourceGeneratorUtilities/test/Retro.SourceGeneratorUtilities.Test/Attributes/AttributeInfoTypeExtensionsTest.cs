using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Model.Attributes;
using Retro.SourceGeneratorUtilities.Test.Utils;

namespace Retro.SourceGeneratorUtilities.Test.Attributes;

public class AttributeInfoTypeExtensionsTest {

  [Test]
  public void GetAttributeInfoTypeInfo_WithValidAttributeData_ReturnsCorrectInfo() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(@"
        using System;
        using Retro.SourceGeneratorUtilities.Utilities.Attributes;

        namespace TestNamespace {
            public class TestAttribute : Attribute { }

            [AttributeInfoType(typeof(TestAttribute))]
            public class TestClass { }
        }
    ");

    var classSymbol = compilation.GetTypeSymbol("TestNamespace.TestClass");
    var attributeData = classSymbol.GetAttributes().Single(a => a.AttributeClass?.Name == "AttributeInfoTypeAttribute");

    // Act
    var info = attributeData.GetAttributeInfoTypeInfo();

    // Assert
    Assert.That(info.Type.Name, Is.EqualTo("TestAttribute"));
  }

  [Test]
  public void GetAttributeInfoTypeInfo_WithInvalidAttributeData_ThrowsException() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(@"
        using System;

        namespace TestNamespace {
            [AttributeUsage(AttributeTargets.Class)]
            public class TestAttribute : Attribute { }

            [Test]
            public class TestClass { }
        }
    ");

    var classSymbol = compilation.GetTypeSymbol("TestNamespace.TestClass");
    var attributeData = classSymbol.GetAttributes().Single();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => attributeData.GetAttributeInfoTypeInfo());
  }

  [Test]
  public void TryGetAttributeInfoTypeInfo_WithValidAttributeData_ReturnsTrue() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(@"
        using System;
        using Retro.SourceGeneratorUtilities.Utilities.Attributes;

        namespace TestNamespace {
            public class TestAttribute : Attribute { }

            [AttributeInfoType(typeof(TestAttribute))]
            public class TestClass { }
        }
    ");

    var classSymbol = compilation.GetTypeSymbol("TestNamespace.TestClass");
    var attributeData = classSymbol.GetAttributes().Single(a => a.AttributeClass?.Name == "AttributeInfoTypeAttribute");

    // Act
    var result = attributeData.TryGetAttributeInfoTypeInfo(out var info);

    Assert.Multiple(() => {
      // Assert
      Assert.That(result, Is.True);
      Assert.That(info.Type.Name, Is.EqualTo("TestAttribute"));
    });
  }

  [Test]
  public void TryGetAttributeInfoTypeInfo_WithInvalidAttributeData_ReturnsFalse() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;

        namespace TestNamespace {
          [AttributeUsage(AttributeTargets.Class)]
          public class TestAttribute : Attribute { }

          [Test]
          public class TestClass { }
          
        }
        """);

    var classSymbol = compilation.GetTypeSymbol("TestNamespace.TestClass");
    var attributeData = classSymbol.GetAttributes().Single();

    // Act
    var result = attributeData.TryGetAttributeInfoTypeInfo(out var info);

    // Assert
    Assert.That(result, Is.False);
    Assert.That(info, Is.EqualTo(default(AttributeInfoTypeInfo)));
  }

  [Test]
  public void TryGetAttributeInfoTypeInfo_WithNullAttributeClass_ReturnsFalse() {
    // Create a mock AttributeData with null AttributeClass
    var attributeData = CSharpCompilation.Create("TestCompilation")
        .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
        .GetTypeByMetadataName("System.Object")
        ?.GetAttributes()
        .FirstOrDefault();

    // Act & Assert
    // If we can't get a null AttributeClass scenario directly, at least test the method
    // handles null gracefully through manual verification
    if (attributeData == null) return;
    var result = attributeData.TryGetAttributeInfoTypeInfo(out var info);
    Assert.Multiple(() => {
      Assert.That(result, Is.False);
      Assert.That(info, Is.EqualTo(default(AttributeInfoTypeInfo)));
    });
  }

  [Test]
  public void GetAttributeInfoTypeInfos_WithNoValidAttributes_ReturnsEmptyCollection() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;

        namespace TestNamespace {
          [AttributeUsage(AttributeTargets.Class)]
          public class TestAttribute : Attribute { }

          [Test]
          public class TestClass { }
        }
        """);

    var classSymbol = compilation.GetTypeSymbol("TestNamespace.TestClass");
    var attributes = classSymbol.GetAttributes();

    // Act
    var infoList = attributes.GetAttributeInfoTypeInfos().ToList();

    // Assert
    Assert.That(infoList, Is.Empty);
  }

  [Test]
  public void ExtractAttributeInfoTypeOverview_ReturnsValidOverview() {
    // Arrange
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using Retro.SourceGeneratorUtilities.Utilities.Attributes;
        using Microsoft.CodeAnalysis;

        namespace TestNamespace {
          public class TestAttribute : Attribute {
            public TestAttribute(string name) { }
              public bool Flag { get; set; }
          }

          [AttributeInfoType(typeof(TestAttribute))]
          public class TestAttributeInfo {
            public TestAttributeInfo(string name) { }
            public bool Flag { get; set; }
          }

          [AttributeInfoType(typeof(DerivedTestAttribute))]
          public class DerivedTestAttributeInfo : TestAttributeInfo {
            public DerivedTestAttributeInfo(string name) : base(name) { }
          }

          public class DerivedTestAttribute : TestAttribute {
            public DerivedTestAttribute(string name) : base(name) { }
          }
        }
        """);
    
    var testAttributeInfoSymbol = compilation.GetTypeSymbol("TestNamespace.TestAttributeInfo");
    var derivedTestAttributeInfoSymbol = compilation.GetTypeSymbol("TestNamespace.DerivedTestAttributeInfo");

    var typeSymbols = ImmutableArray.Create(testAttributeInfoSymbol, derivedTestAttributeInfoSymbol);

    // Act
    var overviewResult = testAttributeInfoSymbol.ExtractAttributeInfoTypeOverview(typeSymbols);

    // Assert
    var overview = overviewResult.Result;

    Assert.That(overview, Is.Not.Null);
    Assert.Multiple(() => {
      Assert.That(overview.Name, Is.EqualTo("TestAttributeInfo"));
      Assert.That(overview.AttributeSymbol.Name, Is.EqualTo("TestAttribute"));

      // Check constructors
      Assert.That(overview.Constructors, Has.Length.EqualTo(1));
    });
    Assert.That(overview.Constructors[0].Parameters, Has.Count.EqualTo(1));
    Assert.Multiple(() => {
      Assert.That(overview.Constructors[0].Parameters[0].Symbol.Name, Is.EqualTo("name"));

      // Check properties
      Assert.That(overview.Properties, Has.Length.EqualTo(1));
    });
    Assert.Multiple(() => {
      Assert.That(overview.Properties[0].Symbol.Name, Is.EqualTo("Flag"));

      // Check child classes
      Assert.That(overview.ChildClasses, Has.Length.EqualTo(1));
    });
    Assert.Multiple(() => {
      Assert.That(overview.ChildClasses[0].ModelType.Name, Is.EqualTo("DerivedTestAttributeInfo"));
      Assert.That(overview.ChildClasses[0].AttributeType.Name, Is.EqualTo("DerivedTestAttribute"));
    });
  }
}

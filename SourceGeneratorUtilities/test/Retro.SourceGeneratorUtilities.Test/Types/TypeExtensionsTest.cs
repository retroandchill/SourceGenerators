using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Types;
using Retro.SourceGeneratorUtilities.Test.Utils;

namespace Retro.SourceGeneratorUtilities.Test.Types;

public class TypeExtensionsTest {

  [Test]
  public void TestIsSameType() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;
        """);

    var typeSymbols = new Dictionary<Type, ITypeSymbol?> {
        // Primitives
        { typeof(void), compilation.GetTypeByMetadataName("System.Void") },
        { typeof(int), compilation.GetTypeByMetadataName("System.Int32") },
        { typeof(string), compilation.GetTypeByMetadataName("System.String") },
        { typeof(object), compilation.GetTypeByMetadataName("System.Object") },

        // Regular class
        { typeof(Attribute), compilation.GetTypeByMetadataName("System.Attribute") },

        // Generic class (open)
        { typeof(List<>), compilation.GetTypeByMetadataName("System.Collections.Generic.List`1") },

        // Generic class (closed)
        {
            typeof(List<int>),
            compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")
                ?.Construct(compilation.GetSpecialType(SpecialType.System_Int32))
        },

        // Interface vs concrete type
        { typeof(IEnumerable<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1") },

        // Different generic parameter counts
        { typeof(Func<>), compilation.GetTypeByMetadataName("System.Func`1") },
        { typeof(Func<,>), compilation.GetTypeByMetadataName("System.Func`2") },

        // Array types
        { typeof(int[]), compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32)) },
        { typeof(string[]), compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_String)) },
        { typeof(int[,]), compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32), 2) },

        // Pointer types
        { typeof(void*), compilation.CreatePointerTypeSymbol(compilation.GetTypeByMetadataName("System.Void")!) },
        { typeof(int*), compilation.CreatePointerTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32)) }
    };

    Assert.Multiple(() => {
      foreach (var (type, symbol) in typeSymbols) {
        Assert.That(symbol?.IsSameType(type), Is.True,
                    $"Failed comparing {symbol?.ToDisplayString()} with {type}");
      }

      // Verify non-matching types
      Assert.That(typeSymbols[typeof(int)]?.IsSameType(typeof(string)), Is.False,
                  "int should not match string");

      Assert.That(typeSymbols[typeof(List<>)]?.IsSameType(typeof(List<int>)), Is.False,
                  "Open generic should not match closed generic");

      Assert.That(typeSymbols[typeof(List<int>)]?.IsSameType(typeof(List<string>)), Is.False,
                  "List<int> should not match List<string>");

      // Generic interface vs concrete type
      Assert.That(typeSymbols[typeof(IEnumerable<>)]?.IsSameType(typeof(List<>)), Is.False,
                  "IEnumerable<T> should not match List<T>");

      // Different generic parameter counts
      Assert.That(typeSymbols[typeof(Func<>)]?.IsSameType(typeof(Func<,>)), Is.False,
                  "Func<T> should not match Func<T1,T2>");

      // Array type tests
      Assert.That(typeSymbols[typeof(int[])]?.IsSameType(typeof(string[])), Is.False,
                  "int[] should not match string[]");

      Assert.That(typeSymbols[typeof(int[])]?.IsSameType(typeof(int[,])), Is.False,
                  "int[] should not match int[,]");

      Assert.That(typeSymbols[typeof(int[])]?.IsSameType(typeof(int)), Is.False,
                  "int[] should not match int");

      // Pointer type tests
      Assert.That(typeSymbols[typeof(void*)]?.IsSameType(typeof(int*)), Is.False,
                  "void* should not match int*");

      Assert.That(typeSymbols[typeof(int*)]?.IsSameType(typeof(int)), Is.False,
                  "int* should not match int");

      Assert.That(typeSymbols[typeof(int*)]?.IsSameType(typeof(int[])), Is.False,
                  "int* should not match int[]");
    });
  }

  [Test]
  public void TestIsOfType() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;
        using Microsoft.CodeAnalysis;
        """);

    var typeSymbols = new Dictionary<Type, INamedTypeSymbol?> {
        // Base types and interfaces
        { typeof(object), compilation.GetTypeByMetadataName("System.Object") },
        { typeof(Attribute), compilation.GetTypeByMetadataName("System.Attribute") },
        { typeof(IEquatable<>), compilation.GetTypeByMetadataName("System.IEquatable`1") },

        // Covariant interfaces
        { typeof(IEnumerable<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1") },
        { typeof(IReadOnlyList<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1") },

        // Contravariant interfaces
        { typeof(IComparer<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IComparer`1") }, {
            typeof(IEqualityComparer<>),
            compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1")
        },

        // Concrete implementations
        { typeof(List<>), compilation.GetTypeByMetadataName("System.Collections.Generic.List`1") }
    };

    // Construct some closed generic types
    var listOfString = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var listOfObject = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var ienumOfString =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var ienumOfObject =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var comparerOfString =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var comparerOfObject =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));

    Assert.Multiple(() => {
      // Basic inheritance
      Assert.That(typeSymbols[typeof(Attribute)]!.IsAssignableTo<object>(), Is.True,
                  "Attribute should be of type object");
      Assert.That(typeSymbols[typeof(object)]!.IsAssignableTo<Attribute>(), Is.False,
                  "Object should not be of type Attribute");

      // Generic interface implementation
      Assert.That(listOfString.IsAssignableTo<IEnumerable<string>>(), Is.True,
                  "List<string> should implement IEnumerable<string>");

      // Covariant interface tests (if T : U then IEnumerable<T> : IEnumerable<U>)
      Assert.That(ienumOfString.IsAssignableTo<IEnumerable<object>>(), Is.True,
                  "IEnumerable<string> should be convertible to IEnumerable<object>");
      Assert.That(listOfString.IsAssignableTo<IEnumerable<object>>(), Is.True,
                  "List<string> should be convertible to IEnumerable<object>");
      Assert.That(ienumOfObject.IsAssignableTo<IEnumerable<string>>(), Is.False,
                  "IEnumerable<object> should not be convertible to IEnumerable<string>");

      // Contravariant interface tests (if T : U then IComparer<U> : IComparer<T>)
      Assert.That(comparerOfObject.IsAssignableTo<IComparer<string>>(), Is.True,
                  "IComparer<object> should be convertible to IComparer<string>");
      Assert.That(comparerOfString.IsAssignableTo<IComparer<object>>(), Is.False,
                  "IComparer<string> should not be convertible to IComparer<object>");

      // Generic type identity
      Assert.That(listOfString.IsAssignableTo<List<object>>(), Is.False,
                  "List<string> should not be convertible to List<object>");
      Assert.That(listOfObject.IsAssignableTo<List<string>>(), Is.False,
                  "List<object> should not be convertible to List<string>");
    });
  }

  [Test]
  public void TestIsOfTypeReverse() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;
        using Microsoft.CodeAnalysis;
        """);

    var typeSymbols = new Dictionary<Type, INamedTypeSymbol?> {
        // Base types and interfaces
        { typeof(object), compilation.GetTypeByMetadataName("System.Object") },
        { typeof(Attribute), compilation.GetTypeByMetadataName("System.Attribute") },
        { typeof(IEquatable<>), compilation.GetTypeByMetadataName("System.IEquatable`1") },

        // Covariant interfaces
        { typeof(IEnumerable<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1") },
        { typeof(IReadOnlyList<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1") },

        // Contravariant interfaces
        { typeof(IComparer<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IComparer`1") }, {
            typeof(IEqualityComparer<>),
            compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1")
        },

        // Concrete implementations
        { typeof(List<>), compilation.GetTypeByMetadataName("System.Collections.Generic.List`1") }
    };

    // Construct some closed generic types
    var listOfString = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var listOfObject = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var ienumOfString =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var ienumOfObject =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var comparerOfString =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var comparerOfObject =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));

    Assert.Multiple(() => {
      // Basic inheritance
      Assert.That(typeSymbols[typeof(Attribute)]!.IsAssignableFrom<object>(), Is.False,
                  "Object should not be of type Attribute");
      Assert.That(typeSymbols[typeof(object)]!.IsAssignableFrom<Attribute>(), Is.True,
                  "Attribute should be of type object");

      // Generic interface implementation
      Assert.That(ienumOfString.IsAssignableFrom<List<string>>(), Is.True,
                  "List<string> should implement IEnumerable<string>");

      // Covariant interface tests (if T : U then IEnumerable<T> : IEnumerable<U>)
      Assert.That(ienumOfObject.IsAssignableFrom<IEnumerable<string>>(), Is.True,
                  "IEnumerable<string> should be convertible to IEnumerable<object>");
      Assert.That(ienumOfObject.IsAssignableFrom<List<string>>(), Is.True,
                  "List<string> should be convertible to IEnumerable<object>");
      Assert.That(ienumOfString.IsAssignableFrom<IEnumerable<object>>(), Is.False,
                  "IEnumerable<object> should not be convertible to IEnumerable<string>");

      // Contravariant interface tests (if T : U then IComparer<U> : IComparer<T>)
      Assert.That(comparerOfString.IsAssignableFrom<IComparer<object>>(), Is.True,
                  "IComparer<object> should be convertible to IComparer<string>");
      Assert.That(comparerOfObject.IsAssignableFrom<IComparer<string>>(), Is.False,
                  "IComparer<string> should not be convertible to IComparer<object>");

      // Generic type identity
      Assert.That(listOfObject.IsAssignableFrom<List<string>>(), Is.False,
                  "List<string> should not be convertible to List<object>");
      Assert.That(listOfString.IsAssignableFrom<List<object>>(), Is.False,
                  "List<object> should not be convertible to List<string>");
    });
  }

  [Test]
  public void TestIsAssignableToWithTypeSymbols() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        namespace TestNamespace {
            public interface IBase {}
            public interface IDerived : IBase {}
            public class BaseClass {}
            public class DerivedClass : BaseClass, IBase {}
            public class MoreDerivedClass : DerivedClass, IDerived {}
        }
        """);

    // Get the type symbols from the compilation
    var baseInterface = compilation.GetTypeByMetadataName("TestNamespace.IBase")!;
    var derivedInterface = compilation.GetTypeByMetadataName("TestNamespace.IDerived")!;
    var baseClass = compilation.GetTypeByMetadataName("TestNamespace.BaseClass")!;
    var derivedClass = compilation.GetTypeByMetadataName("TestNamespace.DerivedClass")!;
    var moreDerivedClass = compilation.GetTypeByMetadataName("TestNamespace.MoreDerivedClass")!;

    Assert.Multiple(() => {
      // Test class inheritance relationships
      Assert.That(derivedClass.IsAssignableTo(baseClass), Is.True,
                  "DerivedClass should be assignable to BaseClass");
      Assert.That(moreDerivedClass.IsAssignableTo(baseClass), Is.True,
                  "MoreDerivedClass should be assignable to BaseClass");
      Assert.That(moreDerivedClass.IsAssignableTo(derivedClass), Is.True,
                  "MoreDerivedClass should be assignable to DerivedClass");
      Assert.That(baseClass.IsAssignableTo(derivedClass), Is.False,
                  "BaseClass should not be assignable to DerivedClass");

      // Test interface implementation relationships
      Assert.That(derivedClass.IsAssignableTo(baseInterface), Is.True,
                  "DerivedClass should be assignable to IBase");
      Assert.That(moreDerivedClass.IsAssignableTo(baseInterface), Is.True,
                  "MoreDerivedClass should be assignable to IBase");
      Assert.That(moreDerivedClass.IsAssignableTo(derivedInterface), Is.True,
                  "MoreDerivedClass should be assignable to IDerived");
      Assert.That(derivedClass.IsAssignableTo(derivedInterface), Is.False,
                  "DerivedClass should not be assignable to IDerived");

      // Test interface inheritance relationships
      Assert.That(derivedInterface.IsAssignableTo(baseInterface), Is.True,
                  "IDerived should be assignable to IBase");
      Assert.That(baseInterface.IsAssignableTo(derivedInterface), Is.False,
                  "IBase should not be assignable to IDerived");

      // Test self-assignability
      Assert.That(baseClass.IsAssignableTo(baseClass), Is.True,
                  "BaseClass should be assignable to itself");
      Assert.That(baseInterface.IsAssignableTo(baseInterface), Is.True,
                  "IBase should be assignable to itself");
    });
  }

  [Test]
  public void TestGetNamedType() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace TestNamespace {
            public class SimpleClass { }
            public interface ISimpleInterface { }
            public class GenericClass<T> { }
            public class NestedGenericClass<T, U> { }
            public interface IGenericInterface<T> { }
            public interface ICovariantInterface<out T> { }
            public interface IContravariantInterface<in T> { }

            namespace Nested {
                public class NestedClass { }
            }
        }
        """);

    // Test data structure: Type and expected metadata name
    var testTypes = new List<Type> {
        // Primitive types
        typeof(int),
        typeof(string),
        typeof(bool),
        typeof(void),
        typeof(object),

        // Simple class and interface
        typeof(List<>),
        typeof(IEnumerable<>),

        // Constructed generic types
        typeof(List<int>),
        typeof(List<string>),
        typeof(Dictionary<,>),
        typeof(Dictionary<string, int>),

        // Array types
        typeof(int[]),
        typeof(string[]),
        typeof(int[,]),
        typeof(List<int>[]),

        // Nested types from BCL
        typeof(Enumerable),
        
        // Pointer types
        typeof(void*),
        typeof(int*),
    };

    // Test each type and verify the expected symbol is returned
    Assert.Multiple(() => {
      foreach (var type in testTypes) {
        try {
          var typeSymbol = compilation.GetType(type);
          Assert.That(typeSymbol, Is.Not.Null, $"Should get symbol for {type.FullName}");

          // For constructed generic types, verify the type arguments match
          if (type.IsConstructedGenericType) {
            Assert.That(typeSymbol is INamedTypeSymbol, Is.True,
                        $"{type.FullName} should be an array type symbol");
            var namedType = (INamedTypeSymbol) typeSymbol;
            Assert.That(namedType.IsGenericType, Is.True, 
                        $"{type.FullName} should be recognized as generic");
            Assert.That(namedType.TypeArguments, Has.Length.EqualTo(type.GenericTypeArguments.Length),
                        $"Generic argument count mismatch for {type.FullName}");
          }

          // For array types, verify dimensions match
          if (type.IsArray) {
            Assert.That(typeSymbol is IArrayTypeSymbol, Is.True,
                        $"{type.FullName} should be an array type symbol");
            if (typeSymbol is IArrayTypeSymbol arraySymbol) {
              Assert.That(arraySymbol.Rank, Is.EqualTo(type.GetArrayRank()),
                          $"Array rank mismatch for {type.FullName}");
            }
          }
        }
        catch (InvalidOperationException ex) {
          Assert.Fail($"Failed to get symbol for {type.FullName}: {ex.Message}");
        }
      }

      // Test the generic version GetNamedType<T>
      var intSymbol = compilation.GetType<int>();
      Assert.That(intSymbol.Name, Is.EqualTo("Int32"));

      var listOfIntSymbol = compilation.GetType<List<int>>() as INamedTypeSymbol;
      Assert.That(listOfIntSymbol, Is.Not.Null);
      Assert.That(listOfIntSymbol!.Name, Is.EqualTo("List"));
      Assert.That(listOfIntSymbol.TypeArguments, Has.Length.EqualTo(1));
      Assert.That(listOfIntSymbol.TypeArguments[0].Name, Is.EqualTo("Int32"));
    });
  }

  [Test]
  public void TestGetTypeofName() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;

        namespace TestNamespace {
            public class SimpleClass { }
            public class Generic<T> { }
            public class MultiGeneric<T, U, V> { }

            namespace Nested {
                public class NestedClass { }
                public class NestedGeneric<T> { }
            }
        }
        """);

    // Test cases with type names and expected typeof-compatible format
    var testCases = new Dictionary<string, string> {
        // Non-generic types
        { "System.Int32", "System.Int32" },
        { "System.String", "System.String" },
        { "System.Object", "System.Object" },
        { "TestNamespace.SimpleClass", "TestNamespace.SimpleClass" },
        { "TestNamespace.Nested.NestedClass", "TestNamespace.Nested.NestedClass" },

        // Open generic types (should produce placeholder commas)
        { "System.Collections.Generic.List`1", "System.Collections.Generic.List<>" },
        { "System.Collections.Generic.Dictionary`2", "System.Collections.Generic.Dictionary<,>" },
        { "TestNamespace.Generic`1", "TestNamespace.Generic<>" },
        { "TestNamespace.MultiGeneric`3", "TestNamespace.MultiGeneric<,,>" },
        { "TestNamespace.Nested.NestedGeneric`1", "TestNamespace.Nested.NestedGeneric<>" }
    };

    Assert.Multiple(() => {
      foreach (var (typeName, expectedTypeofFormat) in testCases) {
        var typeSymbol = compilation.GetTypeByMetadataName(typeName);
        Assert.That(typeSymbol, Is.Not.Null, $"Could not get type symbol for {typeName}");

        var typeofName = typeSymbol!.GetTypeofName();
        Assert.That(typeofName, Is.EqualTo(expectedTypeofFormat), 
                    $"GetTypeofName for {typeName} returned incorrect format");
      }

      // Test with constructed generic types
      var listType = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!;
      var intType = compilation.GetTypeByMetadataName("System.Int32")!;
      var listOfInt = listType.Construct(intType);

      Assert.That(listOfInt.GetTypeofName(), Is.EqualTo("System.Collections.Generic.List<>"), 
                  "Constructed generic should show base name with correct generic argument placeholders");

      // Test with nested generic type with multiple type parameters
      var dictionaryType = compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2")!;
      var stringType = compilation.GetTypeByMetadataName("System.String")!;
      var dictOfStringInt = dictionaryType.Construct(stringType, intType);

      Assert.That(dictOfStringInt.GetTypeofName(), Is.EqualTo("System.Collections.Generic.Dictionary<,>"), 
                  "Dictionary should have one comma placeholder for two type arguments");

      // Test with triple generic type parameter
      var multiGenericType = compilation.GetTypeByMetadataName("TestNamespace.MultiGeneric`3")!;
      Assert.That(multiGenericType.GetTypeofName(), Is.EqualTo("TestNamespace.MultiGeneric<,,>"), 
                  "Triple generic type should have two comma placeholders");
    });
  }
}
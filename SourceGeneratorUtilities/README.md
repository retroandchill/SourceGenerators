# Retro.SourceGeneratorUtilities

[![NuGet Version](https://img.shields.io/nuget/v/Retro.SourceGeneratorUtilities?logo=nuget)](https://www.nuget.org/packages/Retro.SourceGeneratorUtilities)[![GitHub Release](https://img.shields.io/github/v/release/retroandchill/Retro.SourceGeneratorUtilities?logo=github)](https://github.com/retroandchill/Retro.SourceGeneratorUtilities)[![Quality Gate 
Status](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_Retro.SourceGeneratorUtilities&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=retroandchill_Retro.SourceGeneratorUtilities)[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_Retro.SourceGeneratorUtilities&metric=coverage)](https://sonarcloud.io/summary/new_code?id=retroandchill_Retro.SourceGeneratorUtilities)

This repo contains a number of useful utilities for usage within a source generator.

## Attribute Data Models

The primary thing this package provides is the ability to create data models for your attributes. This is done by 
using the `[AttributeInfoType]` attribute.

Say we have the following attribute defined:
```csharp
public enum ServiceScope {
  Singleton,
  Scoped,
  Transient
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class DependencyAttribute(Type type, ServiceScope scope) : Attribute {
  public Type Type { get; } = type;
  public ServiceScope Scope { get; } = scope;
  public string? Key { get; init; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class SingletonAttribute(Type serviceType) : DependencyAttribute(serviceType, ServiceScope.Singleton);

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class SingletonAttribute<TService>() : SingletonAttribute(typeof(TService));
```

We could then define a model that follows:
```csharp
[AttributeInfoType<DependencyAttribute>]
public record DependencyOverview(ITypeSymbol Type, ServiceScope Scope) {
  public string? Key { get; init; }
}

[AttributeInfoType<SingletonAttribute>]
public record SingletonOverview(ITypeSymbol Type) : DependencyOverview(Type, ServiceScope.Singleton);

[AttributeInfoType(typeof(SingletonAttribute<>))]
public record SingletonOneParamOverview(ITypeSymbol Type) : SingletonOverview(Type);
```

The way the matching is done is by validating that the model type has the same constructor signatures as attribute 
as well as all the same settable properties (requiring either the set or init keyword). With only caveat is that all 
instances of `Type` must be replaced with `ITypeSymbol` and if modeling a generic attribute the model must be 
non-generic and contain additional constructor arguments of type `ITypeSymbol` at the beginning of the constructor 
for each generic type parameter.

By doing so you get generated code that looks something like this:
```csharp
public static class DependencyOverviewExtensions {

  public static DependencyOverview GetDependencyOverview(this AttributeData data) {
    return data.TryGetDependencyOverview(out var info) ? info : throw new InvalidOperationException("Cannot create Info");
  }  

  public static bool TryGetDependencyOverview(this AttributeData data, [NotNullWhen(true)] out DependencyOverview? info) {
    var args = data.ConstructorArguments;

    if (data.AttributeClass is null) {
      info = null;
      return false;
    }

    if (!data.AttributeClass.IsAssignableFrom(typeof(Retro.SourceGeneratorUtilities.Generator.Sample.Attributes.SingletonAttribute))) {
      if (Retro.SourceGeneratorUtilities.Generator.Sample.Model.SingletonOverviewExtensions.TryGetSingletonOverview(data, out var childInfo)) {
        info = childInfo;
        return true;
      } else {
        info = null;
        return false;
      }
    }

    if (!data.AttributeClass.IsSameType(typeof(Retro.SourceGeneratorUtilities.Generator.Sample.Attributes.DependencyAttribute))) {
      info = null;
      return false;
    }

    if (data.HasMatchingConstructor(typeof(Microsoft.CodeAnalysis.ITypeSymbol), typeof(Retro.SourceGeneratorUtilities.Generator.Sample.Attributes.ServiceScope))) {
      var namedArguments = data.NamedArguments.ToDictionary();
      info = new DependencyOverview(data.ConstructorArguments[0].GetTypedValue<Microsoft.CodeAnalysis.ITypeSymbol>(), data.ConstructorArguments[0].GetTypedValue<Retro.SourceGeneratorUtilities.Generator.Sample.Attributes.ServiceScope>()) {
          Key = namedArguments.TryGetValue("Key", out var valueKey) ? valueKey.GetTypedValue<string?>() : default,
      };
      return true;
    }
    

    info = null;
    return false;
  }

  public static IEnumerable<DependencyOverview> GetDependencyOverviews(this IEnumerable<AttributeData> attributeDatas) {
    foreach (var data in attributeDatas) {
      if (data.TryGetDependencyOverview(out var info)) {
        yield return info;
      }
    }
  }

}

public static class SingletonOverviewExtensions {

  public static SingletonOverview GetSingletonOverview(this AttributeData data) {
    return data.TryGetSingletonOverview(out var info) ? info : throw new InvalidOperationException("Cannot create Info");
  }  

  public static bool TryGetSingletonOverview(this AttributeData data, [NotNullWhen(true)] out SingletonOverview? info) {
    var args = data.ConstructorArguments;

    if (data.AttributeClass is null) {
      info = null;
      return false;
    }

    if (!data.AttributeClass.IsAssignableFrom(typeof(Retro.SourceGeneratorUtilities.Generator.Sample.Attributes.SingletonAttribute<>))) {
      if (Retro.SourceGeneratorUtilities.Generator.Sample.Model.SingletonOneParamOverviewExtensions.TryGetSingletonOneParamOverview(data, out var childInfo)) {
        info = childInfo;
        return true;
      } else {
        info = null;
        return false;
      }
    }

    if (!data.AttributeClass.IsSameType(typeof(Retro.SourceGeneratorUtilities.Generator.Sample.Attributes.SingletonAttribute))) {
      info = null;
      return false;
    }

    if (data.HasMatchingConstructor(typeof(Microsoft.CodeAnalysis.ITypeSymbol))) {
      var namedArguments = data.NamedArguments.ToDictionary();
      info = new SingletonOverview(data.ConstructorArguments[0].GetTypedValue<Microsoft.CodeAnalysis.ITypeSymbol>()) {
          Key = namedArguments.TryGetValue("Key", out var valueKey) ? valueKey.GetTypedValue<string?>() : default,
      };
      return true;
    }
    

    info = null;
    return false;
  }

  public static IEnumerable<SingletonOverview> GetSingletonOverviews(this IEnumerable<AttributeData> attributeDatas) {
    foreach (var data in attributeDatas) {
      if (data.TryGetSingletonOverview(out var info)) {
        yield return info;
      }
    }
  }

}

public static class SingletonOneParamOverviewExtensions {

  public static SingletonOneParamOverview GetSingletonOneParamOverview(this AttributeData data) {
    return data.TryGetSingletonOneParamOverview(out var info) ? info : throw new InvalidOperationException("Cannot create Info");
  }  

  public static bool TryGetSingletonOneParamOverview(this AttributeData data, [NotNullWhen(true)] out SingletonOneParamOverview? info) {
    var args = data.ConstructorArguments;

    if (data.AttributeClass is null) {
      info = null;
      return false;
    }


    if (!data.AttributeClass.IsSameType(typeof(Retro.SourceGeneratorUtilities.Generator.Sample.Attributes.SingletonAttribute<>))) {
      info = null;
      return false;
    }

    if (data.HasMatchingConstructor(typeof(Microsoft.CodeAnalysis.ITypeSymbol))) {
      var namedArguments = data.NamedArguments.ToDictionary();
      info = new SingletonOneParamOverview(data.ConstructorArguments[0].GetTypedValue<Microsoft.CodeAnalysis.ITypeSymbol>()) {
          Key = namedArguments.TryGetValue("Key", out var valueKey) ? valueKey.GetTypedValue<string?>() : default,
      };
      return true;
    }
    

    info = null;
    return false;
  }

  public static IEnumerable<SingletonOneParamOverview> GetSingletonOneParamOverviews(this IEnumerable<AttributeData> attributeDatas) {
    foreach (var data in attributeDatas) {
      if (data.TryGetSingletonOneParamOverview(out var info)) {
        yield return info;
      }
    }
  }

}
```

## Attributions
[Construction and tools icons created by Dewi Sari - Flaticon](https://www.flaticon.com/free-icons/construction-and-tools "construction and tools icons")
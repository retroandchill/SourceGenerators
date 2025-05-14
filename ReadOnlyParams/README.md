# Retro.ReadOnlyParams

[![NuGet Version](https://img.shields.io/nuget/v/Retro.ReadOnlyParams?logo=nuget)](https://www.nuget.org/packages/Retro.ReadOnlyParams/)[![GitHub Release](https://img.shields.io/github/v/release/retroandchill/Retro.ReadOnlyParams?logo=github)](https://github.com/retroandchill/Retro.ReadOnlyParams/releases)[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_Retro.ReadOnlyParams&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=retroandchill_Retro.ReadOnlyParams)[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=retroandchill_Retro.ReadOnlyParams&metric=coverage)](https://sonarcloud.io/summary/new_code?id=retroandchill_Retro.ReadOnlyParams)

Code analyzer to enable readonly semantics on method parameters (including primary constructors).

## Installation
Install using Nuget: https://www.nuget.org/packages/AutoExceptionHandler/

```powershell
dotnet add package Retro.ReadOnlyParams
```
## Usage
All you need to do is annotate a method parameter with `[ReadOnly]` and the compiler will not permit a reassignment.
```csharp
using System;
using Retro.ReadOnlyParams.Annotations;

namespace Retro.ReadOnlyParams.Sample;

public class ExampleClass([ReadOnly] IInjectedService injectedService) {
  
  public void SomeMethod(IInjectedService serviceInstance) {
    injectedService = serviceInstance; // This will trigger a compiler error
  }
  
}
```
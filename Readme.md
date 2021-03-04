# JsonMergePatch

JsonMergePatch provides an implementation for Json Merge Patch, RFC7396. This library uses C# source generators to generate the types required for serialization. The Http package provides extension methods for HTTP requests, while the AspNetCore package provides an InputReader implementation.

[![CI](https://github.com/ladeak/JsonMergePatch/workflows/CI/badge.svg)](https://github.com/ladeak/JsonMergePatch/actions) [![CodeCoverage](https://codecov.io/gh/ladeak/JsonMergePatch/branch/master/graph/badge.svg)](https://app.codecov.io/gh/ladeak/JsonMergePatch) [![NuGet](https://img.shields.io/nuget/v/LaDeak.JsonMergePatch.AspNetCore.svg)](https://www.nuget.org/packages/LaDeak.JsonMergePatch.AspNetCore/)

## Getting Started

JsonMergePatch library helps to deserialize http request's json body content for merge patch operation. Merge patch operation is detailed by [RFC7396](https://tools.ietf.org/html/rfc7396). If the merge patch request contains members that appear as null on the target object, those members are added. If the target object does contain the member, the value is replaced. Members with null values in the merge patch are set to null (or default) on the target object.

JsonMergePatch library is based on C# source generators. For http body content to be deserialized into a type, the SourceGenerator library generates a helper class. Helper classes are called Wrappers, capturing all the features of the type intended to use for the deserialization. Once the request is deserialized into a Wrapper object, it can be used to apply the patch on an object typed as the source class. The JsonMergePatch library is designed to be used with POCO classes and record types.

Source Generations requires Visual Studio 16.9 or later.



Based on the given application type different packages may be installed from NuGet by running one or more of the following commands:

```
dotnet add package LaDeak.JsonMergePatch.Http
dotnet add package LaDeak.JsonMergePatch.SourceGenerator
dotnet add package LaDeak.JsonMergePatch.AspNetCore
dotnet add package LaDeak.JsonMergePatch.SourceGenerator.AspNetCore
```

## Using with AspNetCore

1. Install packages
1. Add the required usings
1. Add a new controller with a parameter types ```Patch<T>``` where ```T``` is a custom type chosen by the user
1. Extend application startup with ```AddJsonMergePatch()``` extension method.

### Install via NuGet

To use the JsonMergePatch library with AspNetCore install the following packages:

```
dotnet add package LaDeak.JsonMergePatch.AspNetCore
dotnet add package LaDeak.JsonMergePatch.SourceGenerator.AspNetCore
```
### Add the required usings

Add the following usings for controller classes:

```csharp
using LaDeak.JsonMergePatch.Abstractions;
```

### Add a new controller action

In the controller action load the object on which the patch may be applied. Call the ```ApplyPatch()``` method on the input object, passing the target object as a parameter.

```csharp
[HttpPatch("PatchWeather")]
public WeatherForecast PatchForecast(Patch<WeatherForecast> input)
{
    var target = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", TemperatureC = 24 };
    var result = input.ApplyPatch(target);
    return result;
}
```

During a build, the source generator scans for methods with type parameters of ```Patch<T>```. When such a parameter is found a Wrapper type is generated for ```T```. The base class provides the necessary operations to work with the generated type.

### Extend application startup

In **Startup.cs** file add the following using:

```csharp
using LaDeak.JsonMergePatch.Generated;
```

Extend ```ConfigureServices``` method:

```csharp
 public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddControllers().AddJsonMergePatch();
    // ...
}
```

> Note, that ```AddJsonMergePatch``` is a generated method, which means the current Visual Studio's intellisense and editor will not recognize and shows it as an error. At the same time builds shall succeed, which will be indicated on the status bar and the output window. The current suggestion for source generators is to restart Visual Studio after build so intellisense and editor window swallow the errors.

```AddJsonMergePatch()``` has two optional parameters to set a ```Microsoft.AspNetCore.Http.Json.JsonOptions``` and an ```ITypeRepository``` parameter. The method returns ```IMvcBuilder```.

The AspNetCore input reader supports requests with ```application/merge-patch+json``` media type and UTF8 and Unicode encodings.

### Samples

Sample web applications can be found in the [sample folder](https://github.com/ladeak/JsonMergePatch/tree/master/sample)

## Using with Http Requests

Azure Functions

### Samples

Sample console applications and app library can be found in the [sample folder](https://github.com/ladeak/JsonMergePatch/tree/master/sample)

## Example Requests

PATCH /target HTTP/1.1
Host: example.org
Content-Type: application/merge-patch+json

{
  "a":"z",
  "c": {
    "f": null
  }
}

## Supported Types and Use-Cases

* C# POCO classes
* Record types
* Properties with Get and Set methods
* Init only properties
* Classes and Records with non-default constructor
* Built-in and custom types for properties
* Property and Class attributes

## Advanced Scenarios

Dictionary

Multiple projects 

Benefits of the setup
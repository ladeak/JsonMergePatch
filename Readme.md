# JsonMergePatch

JsonMergePatch library provides an implementation for json merge patch operations, detailed in RFC7396. This library uses C# source generators to generate the types required for serialization. The Http package provides extension methods for HTTP requests and responses, while the AspNetCore package provides an InputReader implementation.

[![CI](https://github.com/ladeak/JsonMergePatch/workflows/CI/badge.svg)](https://github.com/ladeak/JsonMergePatch/actions) [![NuGet](https://img.shields.io/nuget/v/LaDeak.JsonMergePatch.AspNetCore.svg)](https://www.nuget.org/packages/LaDeak.JsonMergePatch.AspNetCore/)

## Getting Started

JsonMergePatch library helps to deserialize http requests' and responses' json body content for merge patch operation. Merge patch operation is detailed by [RFC7396](https://tools.ietf.org/html/rfc7396). If the merge patch request contains members that appear as null on the target object, those members are added. If the target object contains the member, the value is replaced. Members with null values in the merge patch requests, are removed from the target object (set to null or default).

JsonMergePatch library is based on C# source generators. For the http body content to be deserialized into a type, the SourceGenerator library generates helper classes. Helper classes are called Wrappers, capturing all the features of the type intended to be used for the deserialization. Once the request is deserialized into a Wrapper object, the object can be used to apply the patch on the user defined target object. The JsonMergePatch library is designed to be used with POCO classes and record types.

Source Generations requires Visual Studio 17.12 or later.

Based on the given application type different packages may be installed from NuGet by running one or more of the following commands:

```
dotnet add package LaDeak.JsonMergePatch.SourceGenerator
dotnet add package LaDeak.JsonMergePatch.Http
dotnet add package LaDeak.JsonMergePatch.AspNetCore
```

## JsonMergePatch with AspNetCore

1. Install AspNetCore package via NuGet
1. Add the required usings
1. Add a new controller with a parameter types ```Patch<T>``` where ```T``` is a custom target type chosen by the user. Make sure that `[Patchable]` is applied on the `T` target type.
1. Extend application startup

### Install AspNetCore packages via NuGet

To use the JsonMergePatch library with AspNetCore install the following packages:

```
dotnet add package LaDeak.JsonMergePatch.AspNetCore
dotnet add package LaDeak.JsonMergePatch.SourceGenerator
```

### Add the required usings

Add the following using to the controller class:

```csharp
using LaDeak.JsonMergePatch.Abstractions;
```

### Add a new controller action

In the controller action implementation, load the target object. Call the ```ApplyPatch()``` method on the input (patch) object, passing the target object as a parameter.

```csharp
[HttpPatch("PatchWeather")]
public WeatherForecast PatchForecast(Patch<WeatherForecast> input)
{
    var target = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", TemperatureC = 24 };
    var result = input.ApplyPatch(target);
    return result;
}
```

During build, the source generator scans types has `[Patchable]` attribute applied. When such a type is found a Wrapper type is generated for it.

### Extend application startup

In **Program.cs** or **Startup.cs** file add the following using:

```csharp
using LaDeak.JsonMergePatch.AspNetCore;
```

Extend ```AddMvcOptions``` method:

```csharp
.AddControllers().AddMvcOptions(options =>
{
    LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.TypeRepository.Instance;
    options.InputFormatters.Insert(0, new JsonMergePatchInputReader());
});
```

> Note, that ```TypeRepository.Instance``` is a generated type's property. The current Visual Studio's intellisense and editor might not recognize it, and might show it as an error. At the same time builds operation shall succeed, which will be indicated on the status bar and the output window.

```JsonMergePatchInputReader```'s constructor has two parameters, one to set a ```Microsoft.AspNetCore.Http.Json.JsonOptions``` and an ```ITypeRepository``` parameter.

The AspNetCore input reader supports requests with ```application/merge-patch+json``` media type and UTF8 and Unicode encodings.

### Patchable

To generate wrapper types with the source generation add the `[Patchable]` attribute:
```csharp
[Patchable]
public class WeatherForecast
{
  //...
}
```

### Using it with System.Text.Json source generation

In order to use multiple source generators, we need to *stack* them. Today the only way to do it is by enforcing a build order between two projects, while adding the first source generator to the first project built, and the second one to the second project built. To make sure JsonMergePatch source generator works with System.Text.Json's source generator create two projects:

1. Entities class library
1. Application executable

Make sure `Application` references the `Entities` project. Add entity/POCO/Patchable types to the `Entities` project. Mark these types with `[Patchable]` attribute.
Add the following nuget packages to the `Entities` project:

1. LaDeak.JsonMergePatch.SourceGenerator
1. LaDeak.JsonMergePatch.Abstractions

This will generate the wrapper types in the `Entities` project.

In the `Application` project add a `JsonSerializerContext` as detailed by System.Text.Json library. In the `[JsonSerializable]` attributes list the generated wrapper types from the `Entities` project using `LaDeak.JsonMergePatch.Generated.Safe{TypeName}` naming pattern.

```csharp
[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.WeatherForecastWrapped))]
[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.CitiesDataWrapped))]
public partial class SampleJsonContext : JsonSerializerContext
{
}
```

For Asp.Net Core applications, create a new `JsonOptions` object and extend the serialization options with the derived `JsonSerializerContext` type. Pass the `JsonOptions` object to the constructor of `JsonMergePatchInputReader`.

```csharp
var mvcBuilder = builder.Services.AddControllers().AddMvcOptions(options =>
{
    LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.TypeRepository.Instance;
    var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();
    jsonOptions.SerializerOptions.AddContext<SampleJsonContext>();
    options.InputFormatters.Insert(0, new JsonMergePatchInputReader(jsonOptions));
});
```

### Samples

Sample web applications can be found in the [sample folder](https://github.com/ladeak/JsonMergePatch/tree/master/sample)

## Using with Http Content

Another approach to leverage Json Marge Patch is through the HttpContent extension method, ```ReadJsonPatchAsync<T>()```. This way a content of an Http request or response can be deserialized into a Wrapper object representing the json merge patch operation.

### Install Http packages via NuGet

To use the JsonMergePatch library with Http Content install the following packages:

```
dotnet add package LaDeak.JsonMergePatch.Http
dotnet add package LaDeak.JsonMergePatch.SourceGenerator
```

### Add the required usings

Add the following usings to the class using json merge patch with http content:

```csharp
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.Http;
```

### Deserialize the Http Content

```csharp
var data = await response.Content.ReadJsonPatchAsync<WeatherForecast>().ConfigureAwait(false);
var target = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", Temp = 24 };
var result = responseData.ApplyPatch(original);
```

To enable using the default ```ITypeRespository``` implementation, configure ```JsonMergePatchOptions``` during application startup:

```csharp
LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeConsoleApp.TypeRepository.Instance;
```

During build, the source generator scans for method invocations of ```ReadJsonPatchAsync<T>``` extension method. When such an invocation is found, a Wrapper type is generated for the generic type parameter ```T```. The base class of the generated type provides the necessary operations to work with the Wrapper type.

The ```ReadJsonPatchAsync<T>``` extension method has 3 optional parameters:

* ```ITypeRepository``` to pass in a customized list of Wrapper types
* ```JsonSerializerOptions``` to override the default options used by ```System.Text.Json.JsonSerializer```
* ```CancellationToken``` to cancel the current operation

The extension method requires a content type header value of ```application/merge-patch+json```, ```application/json``` or empty content type.

### Samples

Sample console application and app library can be found in the [sample folder](https://github.com/ladeak/JsonMergePatch/tree/master/sample)

## Example Requests

This sample request shows how to invoke a JsonMergePatch endpoint with curl. The sample invokes the ```/Sample/PatchWeather``` endpoint defined by the AspNetCore sample application. The sample updates the Temperature to 23 C degrees and deletes the summary.

```curl
curl -X PATCH "https://localhost:5001/Sample/PatchWeather" -H  "accept: text/plain" -H  "Content-Type: application/merge-patch+json" -d "{\"temp\":23,\"summary\":null}"
```

The following example shows how arrays are best to handle with json merge patch operations. Instead of having an array, it is recommended to use a map using a ```Dictionary<string,string>``` property.

> Note, when using maps always use concrete property types on the C# class. The interface of ```IDictionary<K,V>``` is not supported.


```curl
curl -X PATCH "https://localhost:5001/Sample/PatchCities" -H  "accept: text/plain" -H  "Content-Type: application/merge-patch+json" -d "{\"cities\":{\"Dublin\":\"Ireland\",\"London\":\"GB\",\"New York\":null}}"
```

When the resource below is patched with the above command, 3 things happens: Dublin is added with the value of Ireland; London is updated with the value of GB; and New York is removed from the map.

```json
{
  "cities": {
    "Frankfurt": "Germany",
    "New York": "US",
    "London": "UK"
  }
}
```

Resulting the following output of the patched resource:

```json
{
  "cities": {
    "Frankfurt": "Germany",
    "London": "GB",
    "Dublin": "Ireland"
  }
}
```

## Supported Types and Use-Cases

* C# POCO classes
* Record types
* Properties with Get and Set methods
* Init only properties
* Classes or Records with non-default constructors
* Built-in and custom types for properties
* Property and Class attributes (```JsonSerializer``` attributes are preserved and migrated to the Wrapper type and properties)

## Advanced Scenarios

### Use Maps

It is suggested to use maps with the json merge patch operation instead of arrays. A map has several advantages compared to an array. To update an array, a client needs to download the whole array, amend it, sort it if required then send the whole array back to the server. As an array can be large, this might require large volumes of data to be transferred over the wire. A better approach is to use a map instead. A map may contain only the updated/deleted and newly added items. To use a map add a ```Dictionary<string, string>``` typed property to the POCO type declaration, which is used as the source of the generated Wrapper type.

### Merge ITypeRepositories

In larger solutions the source generator packages might be required to be added to multiple projects. Each source generator package operates on top of the project where it is installed, generating types for the POCO classes used for json merge patch operation in the given project. Each generated type is distinguished using a base namespace and the namespace of the POCO type. At runtime the each generated type must be aggregated into a single type repository used across the application.

During application startup use the ```Extend``` extension method to create a union of types in a single type repository. Set the ```JsonMergePatchOptions.Repository``` property (or for AspNetCore pass the type repository to the ```AddJsonMergePatch()``` method) with the return value of the ```Extend()``` method call. In each project a type repository is generated under the following full name schema: ```LaDeak.JsonMergePatch.Generated.Safe{root-namespace-of-the-project}.TypeRepository.Instance```

Use ```Extend``` extension method to merge multiple type repositories:

```csharp
LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeConsoleApp.TypeRepository.Instance.Extend(LaDeak.JsonMergePatch.Generated.SafeConsoleAppLibrary.TypeRepository.Instance);
```

The default type repository implementation is not thread-safe, do not modify it during runtime, it is suggested to do the extension at application startup.

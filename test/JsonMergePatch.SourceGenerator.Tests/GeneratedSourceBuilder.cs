﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public static class GeneratedSourceBuilder
    {
        public static Func<IMvcBuilder, JsonOptions, IMvcBuilder> GetMethod(Assembly assembly, string type, string methodName)
        {
            var method = assembly.GetType(type).GetMethod(methodName);
            Func<IMvcBuilder, JsonOptions, IMvcBuilder> result = method.CreateDelegate<Func<IMvcBuilder, JsonOptions, IMvcBuilder>>();
            return result;
        }

        public static Assembly CompileToAssembly(string code, IEnumerable<MetadataReference> metadataReferences = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var enumerableLocation = typeof(object).Assembly.Location;
            var coreDir = Directory.GetParent(enumerableLocation);
            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(IMvcBuilder).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(ITypeRepository).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(JsonOptions).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(FormatterCollection<>).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll"));
            references.Add(MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll"));
            references.AddRange(metadataReferences ?? Enumerable.Empty<MetadataReference>());
            var compilation = CSharpCompilation.Create($"{Guid.NewGuid()}.dll", new[] { syntaxTree }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var stream = new MemoryStream();
            var result = compilation.Emit(stream);
            if (!result.Success)
                throw new Exception(result.Diagnostics.First().ToString());
            stream.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(stream.ToArray());
            return assembly;
        }
    }
}
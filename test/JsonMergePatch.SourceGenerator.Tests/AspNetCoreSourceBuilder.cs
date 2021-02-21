using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.AspNetCore;
using LaDeak.JsonMergePatch.SourceGenerator.Tests;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests
{
    public static class AspNetCoreSourceBuilder
    {
        public static Assembly CompileMvcToAssembly(string code, IEnumerable<MetadataReference> metadataReferences = null)
        {
            metadataReferences ??= Enumerable.Empty<MetadataReference>();
            List<MetadataReference> references = GetMvcMetadataReferences();
            return SourceBuilder.CompileToAssembly(code, metadataReferences.Concat(references));
        }

        public static (CSharpCompilation Compilation, SyntaxTree Tree) CompileMvc(string code)
        {
            return SourceBuilder.Compile(code, GetMvcMetadataReferences());
        }

        private static List<MetadataReference> GetMvcMetadataReferences()
        {
            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(IMvcBuilder).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(ITypeRepository).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(JsonOptions).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(FormatterCollection<>).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(JsonMergePatchInputReader).Assembly.Location));
            return references;
        }

        public static Func<IMvcBuilder, JsonOptions, IMvcBuilder> GetMethod(Assembly assembly, string type, string methodName)
        {
            var method = assembly.GetType(type).GetMethod(methodName);
            Func<IMvcBuilder, JsonOptions, IMvcBuilder> result = method.CreateDelegate<Func<IMvcBuilder, JsonOptions, IMvcBuilder>>();
            return result;
        }



    }
}

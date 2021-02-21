using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using LaDeak.JsonMergePatch.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions.Tests
{
    public static class SourceBuilder
    {
        public static Assembly CompileToAssembly(string code, IEnumerable<MetadataReference> metadataReferences = null)
        {
            var compilation = Compile(code, metadataReferences);
            return EmitToAssembly(compilation.Compilation);
        }

        public static Assembly EmitToAssembly(Compilation compilation)
        {
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream);
            if (!result.Success)
                throw new Exception(result.Diagnostics.First().ToString());
            stream.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(stream.ToArray());
            return assembly;
        }

        public static (CSharpCompilation Compilation, SyntaxTree Tree) Compile(string code, IEnumerable<MetadataReference> metadataReferences = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var enumerableLocation = typeof(object).Assembly.Location;
            var coreDir = Directory.GetParent(enumerableLocation);
            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll"));
            references.Add(MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll"));
            references.Add(MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Patch<>).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(JsonPropertyNameAttribute).Assembly.Location));
            references.AddRange(metadataReferences ?? Enumerable.Empty<MetadataReference>());
            var compilation = CSharpCompilation.Create($"{Guid.NewGuid()}.dll", new[] { syntaxTree }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            return (compilation, syntaxTree);
        }
    }
}

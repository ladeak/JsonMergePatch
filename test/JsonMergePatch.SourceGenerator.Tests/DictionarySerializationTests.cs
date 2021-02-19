using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class DictionarySerializationTests
    {
        [Theory]
        [ClassData(typeof(DictionarySerializationData))]
        public void ApplyPatch_PropertyToDelete_SetsNullOnTarget(
           Dictionary<string, int> input,
           Dictionary<string, int> expected,
            string jsonInput)
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var dtoWrappedMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.DtoWrapped");
            var targetMetadata = outputAssembly.GetType("TestCode.Dto");
            var target = targetMetadata.GetConstructor(new Type[0]).Invoke(null);
            targetMetadata.GetProperty("Values").SetValue(target, input);

            var sut = JsonSerializer.Deserialize(jsonInput, dtoWrappedMetadata);
            var patchedParent = dtoWrappedMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { target });

            Assert.Equal(expected, targetMetadata.GetProperty("Values").GetValue(patchedParent));
        }

        private static (Compilation Input, Compilation Output) CreateInputOutputCompilation()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode
{
    public class Dto { public System.Collections.Generic.Dictionary<string, int> Values { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            return (inputCompilation, outputCompilation);
        }

        private static Compilation CreateWrappedTypeCompilation() => CreateInputOutputCompilation().Output;

        private static Compilation CreateCompilation(string source) => SourceBuilder.CompileMvc(source).Compilation;
    }

    public class DictionarySerializationData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] { new Dictionary<string, int> { { "one", 1 }, { "two", 2 }, { "three", 3 } }, new Dictionary<string, int> { { "one", 1 }, { "three", 33 }, { "four", 4 } }, @"{ ""Values"": { ""two"" : null, ""three"" : 33, ""four"" : 4 } }"  },
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}


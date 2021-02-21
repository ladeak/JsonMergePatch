using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.Http;
using LaDeak.JsonMergePatch.SourceGenerator.Abstractions;
using LaDeak.JsonMergePatch.SourceGenerator.Abstractions.Tests;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Http.Tests
{
    public class PatchParameterWalkerTests
    {
        [Fact]
        public async Task Returns_ReadJsonPatchAsync_Types()
        {
            string code = @"
using LaDeak.JsonMergePatch.Http;
public class Controller
{
    public class SomeType { }
    public async System.Threading.Tasks.Task TestMethodAsync()
    {
        var client = new System.Net.Http.HttpClient();
        var response = await client.GetAsync(""https://test.com"");
        await response.Content.ReadJsonPatchAsync<SomeType>();
    }
}";

            var compilation = SourceBuilder.Compile(code, new[] { 
                MetadataReference.CreateFromFile(typeof(PatchParameterWalkerTests).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Patch<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HttpContentExtensions).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonSerializerOptions).Assembly.Location)});

            var sut = new PatchParametersWalker();
            var result = sut.Process(await compilation.Tree.GetRootAsync(), compilation.Compilation.GetSemanticModel(compilation.Tree));
            Assert.Equal("SomeType", result.First().Name);
        }
    }
}

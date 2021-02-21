using System.Linq;
using LaDeak.JsonMergePatch.SourceGenerator;
using LaDeak.JsonMergePatch.SourceGenerator.AspNetCore;
using LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace JsonMergePatch.SourceGenerator.AspNetCore.Tests
{
    public class AspNetJsonMergePatchSourceGeneratorTests
    {
        [Fact]
        public void AddJsonMergePatch_AddsExtensionMethod()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public class Dto { public int Property { get; set; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            var commonGenerator = new JsonMergePatchSourceGenerator();
            var sut = new AspNetJsonMergePatchSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(commonGenerator, sut);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.True(diagnostics.IsEmpty);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult testedGenerator = runResult.Results[1];
            Assert.Single(testedGenerator.GeneratedSources);
            Assert.Empty(testedGenerator.Diagnostics);
        }

        [Fact]
        public void EmptyContenxt_Initialize_DoesNotThrow()
        {
            var generator = new AspNetJsonMergePatchSourceGenerator();
            generator.Initialize(new GeneratorInitializationContext());
        }

        private static Compilation CreateCompilation(string source) => AspNetCoreSourceBuilder.CompileMvc(source).Compilation;
    }
}

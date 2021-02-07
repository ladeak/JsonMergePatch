using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class PatchParameterWalkerTests
    {
        [Fact]
        public async Task ReturnsGeneric_TypeOf_Patch()
        {
            string code = @"
public class Controller
{
    public class SomeType { }
    public void TestMethod(LaDeak.JsonMergePatch.Patch<SomeType> input) { }
}";

            var compilation = GeneratedSourceBuilder.Compile(code, new[] { MetadataReference.CreateFromFile(typeof(PatchParameterWalkerTests).Assembly.Location), MetadataReference.CreateFromFile(typeof(Patch<>).Assembly.Location) });

            var sut = new PatchParametersWalker(compilation.Compilation.GetSemanticModel(compilation.Tree));
            var result = sut.Process(await compilation.Tree.GetRootAsync());
            Assert.Equal("SomeType", result.Single().Name);
        }

        [Fact]
        public async Task Returns_Multiple_PatchTypes()
        {
            string code = @"
public class Controller
{
    public class SomeType { }
    public void TestMethod1(LaDeak.JsonMergePatch.Patch<SomeType> input) { }
    public void TestMethod2(LaDeak.JsonMergePatch.Patch<bool> input) { }
}";

            var compilation = GeneratedSourceBuilder.Compile(code, new[] { MetadataReference.CreateFromFile(typeof(PatchParameterWalkerTests).Assembly.Location), MetadataReference.CreateFromFile(typeof(Patch<>).Assembly.Location) });

            var sut = new PatchParametersWalker(compilation.Compilation.GetSemanticModel(compilation.Tree));
            var result = sut.Process(await compilation.Tree.GetRootAsync());
            Assert.Equal("SomeType", result.First().Name);
            Assert.Equal("Boolean", result.Last().Name);
            Assert.Equal("System", result.Last().ContainingNamespace.Name);
        }

        [Fact]
        public async Task OtherTypesNamed_Patch_Ignored()
        {
            string code = @"
public class Controller
{
    public class Patch<T> { }
    public void TestMethod1(LaDeak.JsonMergePatch.Patch<SomeType> input) { }
    public void TestMethod2(Patch<bool> input) { }
}";

            var compilation = GeneratedSourceBuilder.Compile(code, new[] { MetadataReference.CreateFromFile(typeof(PatchParameterWalkerTests).Assembly.Location), MetadataReference.CreateFromFile(typeof(Patch<>).Assembly.Location) });

            var sut = new PatchParametersWalker(compilation.Compilation.GetSemanticModel(compilation.Tree));
            var result = sut.Process(await compilation.Tree.GetRootAsync());
            Assert.Equal("SomeType", result.Single().Name);
        }

        [Fact]
        public async Task UsingNamespace_PatchedType_Returned()
        {
            string code = @"
using LaDeak.JsonMergePatch;
public class Controller
{
    public class SomeType { }
    public void TestMethod1(Patch<SomeType> input) { }
}";

            var compilation = GeneratedSourceBuilder.Compile(code, new[] { MetadataReference.CreateFromFile(typeof(PatchParameterWalkerTests).Assembly.Location), MetadataReference.CreateFromFile(typeof(Patch<>).Assembly.Location) });

            var sut = new PatchParametersWalker(compilation.Compilation.GetSemanticModel(compilation.Tree));
            var result = sut.Process(await compilation.Tree.GetRootAsync());
            Assert.Equal("SomeType", result.Single().Name);
            Assert.Equal("Controller", result.Single().ContainingType.Name);
        }

        [Fact]
        public async Task No_PatchedType_EmptyResult()
        {
            string code = @"
public class Controller
{
    public void TestMethod1(SomeType input) { }
}";

            var compilation = GeneratedSourceBuilder.Compile(code, new[] { MetadataReference.CreateFromFile(typeof(PatchParameterWalkerTests).Assembly.Location), MetadataReference.CreateFromFile(typeof(Patch<>).Assembly.Location) });

            var sut = new PatchParametersWalker(compilation.Compilation.GetSemanticModel(compilation.Tree));
            var result = sut.Process(await compilation.Tree.GetRootAsync());
            Assert.Empty(result);
        }
    }
}

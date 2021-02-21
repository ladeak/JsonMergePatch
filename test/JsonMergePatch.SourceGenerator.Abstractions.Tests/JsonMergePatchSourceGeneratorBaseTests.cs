using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions.Tests
{
    /// <summary>
    /// Testing documentation: https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.cookbook.md#unit-testing-of-generators
    /// </summary>
    public class JsonMergePatchSourceGeneratorBaseTests
    {
        [Fact]
        public void SinglePatchType_ExtensionAndTypeAddedToSource()
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
            JsonMergePatchSourceGeneratorBase generator = new JsonMergePatchSourceGeneratorBase();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.True(diagnostics.IsEmpty);
            Assert.True(outputCompilation.SyntaxTrees.Count() == 3);
            Assert.True(outputCompilation.GetDiagnostics().IsEmpty);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            Assert.Equal(2, runResult.GeneratedTrees.Length);
            Assert.Empty(runResult.Diagnostics);
        }

        [Fact]
        public void MultiplePatchType_ExtensionAndTypeAddedToSource()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode2
{
    public class Dto0 { public double Property { get; set; }  }
    public class Dto1 { public Dto0 Property { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto0> data)
        {
        }

        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto1> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGeneratorBase generator = new JsonMergePatchSourceGeneratorBase();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.True(diagnostics.IsEmpty);
            Assert.True(outputCompilation.SyntaxTrees.Count() == 4);
            Assert.True(outputCompilation.GetDiagnostics().IsEmpty);

            GeneratorDriverRunResult runResult = driver.GetRunResult();

            Assert.Equal(3, runResult.GeneratedTrees.Length);
            Assert.Empty(runResult.Diagnostics);
        }

        [Fact]
        public void DtoWithProperties_WrapperTypeAddedToSource()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode3
{
    public class Dto4 { public System.String Property { get; set; } }

    public class Program2
    {
        public void SomeMethod2(LaDeak.JsonMergePatch.Abstractions.Patch<Dto4> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGeneratorBase generator = new JsonMergePatchSourceGeneratorBase();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.True(diagnostics.IsEmpty);
            Assert.True(outputCompilation.SyntaxTrees.Count() == 3);
            Assert.True(outputCompilation.GetDiagnostics().IsEmpty);
        }

        private static (Compilation Input, Compilation Output) CreateInputOutputCompilation()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode3
{
    public class Dto1 { public System.Int32 NumberProp { get; set; } }

    public class Dto4 { public System.String Property { get; set; } public Dto1 OtherDto { get; set; } }

    public class Program2
    {
        public void SomeMethod2(LaDeak.JsonMergePatch.Abstractions.Patch<Dto4> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGeneratorBase generator = new JsonMergePatchSourceGeneratorBase();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            return (inputCompilation, outputCompilation);
        }

        private static Compilation CreateCompilation(string source) => SourceBuilder.Compile(source).Compilation;
    }
}

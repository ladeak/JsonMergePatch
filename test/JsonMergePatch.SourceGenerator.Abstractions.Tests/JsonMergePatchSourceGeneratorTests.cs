using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    /// <summary>
    /// Testing documentation: https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.cookbook.md#unit-testing-of-generators
    /// </summary>
    public class JsonMergePatchSourceGeneratorTests
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
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.True(diagnostics.IsEmpty);
            Assert.True(outputCompilation.SyntaxTrees.Count() == 3);
            Assert.True(outputCompilation.GetDiagnostics().IsEmpty);
        }

        [Fact]
        public void RecordType_ExtensionAndTypeAddedToSource()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public record Dto { public int Property { get; set; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void RecordTypeWithInitValueProperty_ExtensionAndTypeAddedToSource()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public record Dto { public int Property { get; init; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void MultiplePatchType_WithInitProperties_ExtensionAndTypeAddedToSource()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode2
{
    public record Dto0 { public double Property { get; init; }  }
    public record Dto1 { public Dto0 Property { get; init; } }

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
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void RecordTypeWithInitReferenceProperty_ExtensionAndTypeAddedToSource()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public record Dto { public string Property { get; init; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void ClassTypeWithInitOnlyValueProperty_ExtensionAndTypeAddedToSource()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public class Dto { public int Property { get; init; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void MultipleClassTypeType_WithInitProperties_ExtensionAndTypeAddedToSource()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode2
{
    public class Dto0 { public double Property { get; init; }  }
    public class Dto1 { public Dto0 Property { get; init; } }

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
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void ClassType_WithInitReferenceProperty_ExtensionAndTypeAddedToSource()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public class Dto { public string Property { get; init; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void ClassType_InitOnlyWithGetSetProperty_ExtensionAndTypeAddedToSource()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public class Dto { public string Property { get; init; }  public string Property1 { get; set; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
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
        public void DefaultRecordType_ExtensionAndTypeAddedToSource()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode1
{
    public record Dto(int Property0, string Property1);
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
");
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.True(diagnostics.IsEmpty);
            Assert.True(outputCompilation.SyntaxTrees.Count() == 3);
            Assert.True(outputCompilation.GetDiagnostics().IsEmpty);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            Assert.Equal(2, runResult.GeneratedTrees.Length);
            Assert.Empty(runResult.Diagnostics);
        }

        private static Compilation CreateCompilation(string source) => SourceBuilder.Compile(source).Compilation;
    }
}

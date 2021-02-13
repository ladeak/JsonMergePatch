using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class GeneratedWrapperTypeTests
    {
        [Fact]
        public void WrappedDtoType_Ctr_CreatesObject()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var sut = assembly.GetType("LaDeak.JsonMergePatch.Generated.Dto4Wrapped").GetConstructor(new Type[0]).Invoke(null);

            Assert.NotNull(sut);
        }

        [Fact]
        public void WrappedDtoType_Sets_GeneratedProperties()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedTypeMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.Dto4Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedTypeMetadata.GetProperty("Property").SetValue(sut, "test");

            Assert.Equal("test", wrappedTypeMetadata.GetProperty("Property").GetValue(sut));
        }

        [Fact]
        public void WrappedSubDtoType_Sets_GeneratedProperties()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedTypeMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.Dto1Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedTypeMetadata.GetProperty("NumberProp").SetValue(sut, 100);

            Assert.Equal(100, wrappedTypeMetadata.GetProperty("NumberProp").GetValue(sut));
        }

        [Fact]
        public void WrappedDtoType_Sets_SubDtoProperty()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedDtoMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.Dto4Wrapped");
            var wrappedSubDtoMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.Dto1Wrapped");
            var sut = wrappedDtoMetadata.GetConstructor(new Type[0]).Invoke(null);
            var subDto = wrappedSubDtoMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedDtoMetadata.GetProperty("OtherDto").SetValue(sut, subDto);

            Assert.Same(subDto, wrappedDtoMetadata.GetProperty("OtherDto").GetValue(sut));
        }

        [Fact]
        public void ApplyPatch_SetsPropertiesWithValues_ToTargetObject()
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var wrappedTypeMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.Dto1Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedTypeMetadata.GetProperty("NumberProp").SetValue(sut, 100);

            var targetTypeMetadata = outputAssembly.GetType("TestCode3.Dto1");
            var targetObject = targetTypeMetadata.GetConstructor(new Type[0]).Invoke(null);

            wrappedTypeMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { targetObject });

            Assert.Equal(100, targetTypeMetadata.GetProperty("NumberProp").GetValue(targetObject));
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
        public void SomeMethod2(LaDeak.JsonMergePatch.Patch<Dto4> data)
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
}

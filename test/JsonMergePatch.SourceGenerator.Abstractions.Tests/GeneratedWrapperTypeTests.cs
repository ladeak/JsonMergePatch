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

            var sut = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto2Wrapped").GetConstructor(new Type[0]).Invoke(null);

            Assert.NotNull(sut);
        }

        [Fact]
        public void WrappedDtoType_Sets_GeneratedProperties()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedTypeMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto2Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedTypeMetadata.GetProperty("Property").SetValue(sut, "test");

            Assert.Equal("test", wrappedTypeMetadata.GetProperty("Property").GetValue(sut));
        }

        [Fact]
        public void WrappedSubDtoType_Sets_GeneratedProperties()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedTypeMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto1Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedTypeMetadata.GetProperty("NumberProp").SetValue(sut, 100);

            Assert.Equal(100, wrappedTypeMetadata.GetProperty("NumberProp").GetValue(sut));
        }

        [Fact]
        public void WrappedDtoType_Sets_SubDtoProperty()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedDtoMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto2Wrapped");
            var wrappedSubDtoMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto1Wrapped");
            var sut = wrappedDtoMetadata.GetConstructor(new Type[0]).Invoke(null);
            var subDto = wrappedSubDtoMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedDtoMetadata.GetProperty("OtherDto").SetValue(sut, subDto);

            Assert.Same(subDto, wrappedDtoMetadata.GetProperty("OtherDto").GetValue(sut));
        }

        [Fact]
        public void NullArgument_ApplyPatch_ReturnsObject()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedSubDtoMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto1Wrapped");
            var sut = wrappedSubDtoMetadata.GetConstructor(new Type[0]).Invoke(null);

            var result = wrappedSubDtoMetadata.GetMethod("ApplyPatch").Invoke(sut, new object[] { null });
            Assert.NotNull(result);
        }

        [Fact]
        public void ApplyPatch_CallsApplyPath_OnSubDto()
        {
            Compilation outputCompilation = CreateWrappedTypeCompilation();
            var assembly = SourceBuilder.EmitToAssembly(outputCompilation);

            var wrappedDtoMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto2Wrapped");
            var wrappedSubDtoMetadata = assembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto1Wrapped");
            var sut = wrappedDtoMetadata.GetConstructor(new Type[0]).Invoke(null);
            var subDto = wrappedSubDtoMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedDtoMetadata.GetProperty("OtherDto").SetValue(sut, subDto);
            wrappedSubDtoMetadata.GetProperty("NumberProp").SetValue(subDto, 100);

            var result = wrappedDtoMetadata.GetMethod("ApplyPatch").Invoke(sut, new object[] { null });
            var targetDtoMetadata = assembly.GetType("TestCode.Dto2");
            var targetSubDtoMetadata = assembly.GetType("TestCode.Dto1");
            var subDtoResult = targetDtoMetadata.GetProperty("OtherDto").GetValue(result);
            Assert.NotNull(subDtoResult);
            Assert.Equal(100, targetSubDtoMetadata.GetProperty("NumberProp").GetValue(subDtoResult));
        }

        [Fact]
        public void ApplyPatch_SetsPropertiesWithValues_ToTargetObject()
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var wrappedTypeMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto1Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedTypeMetadata.GetProperty("NumberProp").SetValue(sut, 100);

            var targetTypeMetadata = outputAssembly.GetType("TestCode.Dto1");
            var targetObject = targetTypeMetadata.GetConstructor(new Type[0]).Invoke(null);

            wrappedTypeMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { targetObject });

            Assert.Equal(100, targetTypeMetadata.GetProperty("NumberProp").GetValue(targetObject));
        }

        [Fact]
        public void ApplyPatch_DoesNotSet_ValuePropertyWithoutValue()
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var wrappedTypeMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto1Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);

            var targetTypeMetadata = outputAssembly.GetType("TestCode.Dto1");
            var targetObject = targetTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            targetTypeMetadata.GetProperty("NumberProp").SetValue(targetObject, 100);

            wrappedTypeMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { targetObject });

            Assert.Equal(100, targetTypeMetadata.GetProperty("NumberProp").GetValue(targetObject));
        }

        [Fact]
        public void ApplyPatch_DoesNotSet_ReferencePropertyWithoutValue()
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var wrappedTypeMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto2Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);

            var targetTypeMetadata = outputAssembly.GetType("TestCode.Dto2");
            var targetObject = targetTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            targetTypeMetadata.GetProperty("Property").SetValue(targetObject, "hello world");

            wrappedTypeMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { targetObject });

            Assert.Equal("hello world", targetTypeMetadata.GetProperty("Property").GetValue(targetObject));
        }

        [Fact]
        public void ApplyPatch_PropertyToDelete_SetsNullOnTarget()
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var wrappedTypeMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.Dto2Wrapped");
            var sut = wrappedTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            wrappedTypeMetadata.GetProperty("Property").SetValue(sut, null);

            var targetTypeMetadata = outputAssembly.GetType("TestCode.Dto2");
            var targetObject = targetTypeMetadata.GetConstructor(new Type[0]).Invoke(null);
            targetTypeMetadata.GetProperty("Property").SetValue(targetObject, "hello world");

            wrappedTypeMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { targetObject });

            Assert.Null(targetTypeMetadata.GetProperty("Property").GetValue(targetObject));
        }

        private static (Compilation Input, Compilation Output) CreateInputOutputCompilation()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode
{
    public class Dto1 { public System.Int32 NumberProp { get; set; } }

    public class Dto2 { public System.String Property { get; set; } public Dto1 OtherDto { get; set; } }

    public class Program2
    {
        public void SomeMethod2(LaDeak.JsonMergePatch.Abstractions.Patch<Dto2> data)
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

        private static Compilation CreateCompilation(string source) => SourceBuilder.Compile(source).Compilation;
    }
}

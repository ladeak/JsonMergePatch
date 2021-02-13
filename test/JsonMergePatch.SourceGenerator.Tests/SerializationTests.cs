using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class SerializationTests
    {
        [Theory]
        [ClassData(typeof(SerializationData))]
        public void ApplyPatch_PropertyToDelete_SetsNullOnTarget(
            bool hasTargetParent,
            bool hasTargetSub,
            string parentStringPropertyInput,
            string parentStringPropertyExpected,
            int numberPropInput,
            int numberPropExpected,
            DateTime? nullableDateTimePropertyInput,
            DateTime? nullableDateTimePropertyExpected,
            double camelCasePropertyInput,
            double camelCasePropertyExpected,
            string jsonInput)
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var parentDtoWrappedMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.ParentDtoWrapped");
            var targetParentMetadata = outputAssembly.GetType("TestCode.ParentDto");
            var targetSubMetadata = outputAssembly.GetType("TestCode.SubDto");

            var sut = JsonSerializer.Deserialize(jsonInput, parentDtoWrappedMetadata);

            object targetParent = null;
            if (hasTargetParent)
            {
                targetParent = targetParentMetadata.GetConstructor(new Type[0]).Invoke(null);
                targetParentMetadata.GetProperty("ParentStringProperty").SetValue(targetParent, parentStringPropertyInput);
            }
            if (hasTargetSub)
            {
                var targetSub = targetSubMetadata.GetConstructor(new Type[0]).Invoke(null);
                targetParentMetadata.GetProperty("OtherDto").SetValue(targetParent, targetSub);
                targetSubMetadata.GetProperty("NumberProp").SetValue(targetSub, numberPropInput);
                targetSubMetadata.GetProperty("NullableDateTimeProperty").SetValue(targetSub, nullableDateTimePropertyInput);
                targetSubMetadata.GetProperty("CamelCaseProperty").SetValue(targetSub, camelCasePropertyInput);
            }

            var patchedParent = parentDtoWrappedMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { targetParent });
            var patchedSub = targetParentMetadata.GetProperty("OtherDto").GetValue(patchedParent);

            Assert.Equal(parentStringPropertyExpected, targetParentMetadata.GetProperty("ParentStringProperty").GetValue(patchedParent));
            Assert.Equal(numberPropExpected, targetSubMetadata.GetProperty("NumberProp").GetValue(patchedSub));
            Assert.Equal(nullableDateTimePropertyExpected, targetSubMetadata.GetProperty("NullableDateTimeProperty").GetValue(patchedSub));
            Assert.Equal(camelCasePropertyExpected, targetSubMetadata.GetProperty("CamelCaseProperty").GetValue(patchedSub));
        }



        private static (Compilation Input, Compilation Output) CreateInputOutputCompilation()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode
{
    public class SubDto { public System.Int32 NumberProp { get; set; } public System.DateTime NullableDateTimeProperty { get; set; } [System.Text.Json.Serialization.JsonPropertyNameAttribute(""camelCaseProperty"")] public System.Double CamelCaseProperty { get; set; } }

    public class ParentDto { public System.String ParentStringProperty { get; set; } public SubDto OtherDto { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Patch<ParentDto> data)
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

    public class SerializationData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] {false, false, null, "hello", 0, 1, DateTime.MinValue, new DateTime(2021,2,20),0,3.5, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""camelCaseProperty"": 3.5 } }"  },
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

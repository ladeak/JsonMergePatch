using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class BasicSerializationTests
    {
        [Theory]
        [ClassData(typeof(BasicSerializationData))]
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
            int[] valuesInput,
            int[] valuesExpected,
            string jsonInput)
        {
            var compilation = CreateWrappedTypeCompilation();
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var parentDtoWrappedMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.STestCode.ParentDtoWrapped");
            var targetParentMetadata = outputAssembly.GetType("TestCode.ParentDto");
            var targetSubMetadata = outputAssembly.GetType("TestCode.SubDto");

            var sut = JsonSerializer.Deserialize(jsonInput, parentDtoWrappedMetadata);

            object targetParent = null;
            if (hasTargetParent)
            {
                targetParent = targetParentMetadata.GetConstructor(new Type[0]).Invoke(null);
                targetParentMetadata.GetProperty("ParentStringProperty").SetValue(targetParent, parentStringPropertyInput);
                targetParentMetadata.GetProperty("Values").SetValue(targetParent, valuesInput);
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
            Assert.Equal(valuesExpected, targetParentMetadata.GetProperty("Values").GetValue(patchedParent));
            Assert.Equal(numberPropExpected, targetSubMetadata.GetProperty("NumberProp").GetValue(patchedSub));
            Assert.Equal(nullableDateTimePropertyExpected, targetSubMetadata.GetProperty("NullableDateTimeProperty").GetValue(patchedSub));
            Assert.Equal(camelCasePropertyExpected, targetSubMetadata.GetProperty("CamelCaseProperty").GetValue(patchedSub));
        }



        private static (Compilation Input, Compilation Output) CreateInputOutputCompilation()
        {
            Compilation inputCompilation = CreateCompilation(@"
namespace TestCode
{
    public class SubDto { public System.Int32 NumberProp { get; set; } public System.DateTime? NullableDateTimeProperty { get; set; } [System.Text.Json.Serialization.JsonPropertyNameAttribute(""camelCaseProperty"")] public System.Double CamelCaseProperty { get; set; } }

    public class ParentDto { public System.String ParentStringProperty { get; set; } public SubDto OtherDto { get; set; } public System.Collections.Generic.IEnumerable<int> Values { get; set; } }

    public class SomeBase {  public System.Int32 NumberPropBase { get; set; } }

    public class SomeDerived : SomeBase {  public System.String SomeDerivedProp { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<ParentDto> data)
        {
        }

        public void SomeMethod2(LaDeak.JsonMergePatch.Abstractions.Patch<SomeDerived> data)
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

    public class BasicSerializationData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] {false, false, default(string), "hello", default(int), 1, default(DateTime), new DateTime(2021, 2, 20), default(double), 3.5, new int[0], new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""camelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }"  },
            new object[] {false, false, default(string), "hello", default(int), 1, default(DateTime), new DateTime(2021, 2, 20), default(double), 3.5, new int[0], new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""camelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }"  },
            new object[] {true, true, "world", "hello", default(int), 1, default(DateTime), new DateTime(2021, 2, 20), default(double), 3.5, new int[0], new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""camelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }"  },
            new object[] {true, true, "world", "hello", 10, 1, new DateTime(2021, 1, 20), new DateTime(2021, 2, 20), 100, 3.5, new int[] { 1, 2 }, new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""camelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }"  },
            new object[] {true, true, "world", "world", 10, 10, new DateTime(2021, 1, 20), new DateTime(2021, 1, 20), 100, 100, new int[] { 1, 2 }, new[] { 1, 2, 3 }, @"{ ""Values"": [ 1, 2, 3 ] }"  },
            new object[] {true, true, "world", "world", 10, 10, new DateTime(2021, 1, 20), new DateTime(2021, 1, 20), 100, 100, new int[] { 1, 2, 3, 4 }, new[] { 1, 2, 3 }, @"{ ""Values"": [ 1, 2, 3 ] }"  },
            new object[] {true, true, "world", "world", 10, 1, new DateTime(2021, 1, 20), new DateTime(2021, 1, 20), 100, 3.5, new int[0], new int[0], @"{ ""OtherDto"": { ""NumberProp"": 1, ""camelCaseProperty"": 3.5 } }"  },
            new object[] {true, true, "world", default(string), 10, 1, new DateTime(2021, 1, 20), default(DateTime?), 100, 3.5, new int[] { 1, 2 }, default(int[]), @"{ ""ParentStringProperty"": null, ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":null, ""camelCaseProperty"": 3.5 }, ""Values"": null }"  },
            new object[] {true, true, "world", default(string), 10, 10, new DateTime(2021, 1, 20), default(DateTime?), 100, 3.5, new int[] { 1, 2 }, default(int[]), @"{ ""ParentStringProperty"": null, ""OtherDto"": { ""NullableDateTimeProperty"":null, ""camelCaseProperty"": 3.5 }, ""Values"": null }"  },
            new object[] {true, true, "world", "world", 5, 0, default(DateTime), default(DateTime), 10, 0, new int[0], new int[0], @"{ ""OtherDto"": { ""NumberProp"": null, ""camelCaseProperty"": null } }"  },
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}


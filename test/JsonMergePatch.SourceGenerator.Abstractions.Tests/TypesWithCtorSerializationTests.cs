using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class TypesWithCtorSerializationTests
    {
        [Theory]
        [ClassData(typeof(TypesWithCtorSerializationData))]
        public void ApplyPatch_PropertyToDelete_SetsNullOnTarget(
            string sourceCode,
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
            var compilation = CreateInputOutputCompilation(sourceCode);
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var parentDtoWrappedMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.SafeTestCode.ParentDtoWrapped");
            var targetParentMetadata = outputAssembly.GetType("TestCode.ParentDto");
            var targetSubMetadata = outputAssembly.GetType("TestCode.SubDto");

            var sut = JsonSerializer.Deserialize(jsonInput, parentDtoWrappedMetadata);

            object targetParent = null;
            if (hasTargetParent)
            {
                var ctor = targetParentMetadata.GetConstructors().OrderByDescending(x => x.GetParameters().Count()).First();
                targetParent = ctor.Invoke(new object[ctor.GetParameters().Count()]);
                targetParentMetadata.GetProperty("ParentStringProperty").SetValue(targetParent, parentStringPropertyInput);
                targetParentMetadata.GetProperty("Values").SetValue(targetParent, valuesInput);
            }
            if (hasTargetSub)
            {
                var ctor = targetSubMetadata.GetConstructors().OrderByDescending(x => x.GetParameters().Count()).First();
                var targetSub = ctor.Invoke(new object[ctor.GetParameters().Count()]);
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

        private static Compilation CreateInputOutputCompilation(string sourceCode)
        {
            Compilation inputCompilation = SourceBuilder.Compile(sourceCode).Compilation;
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            return outputCompilation;
        }
    }

    public class TypesWithCtorSerializationData : IEnumerable<object[]>
    {
        public TypesWithCtorSerializationData()
        {
            _data.Add(new object[] { _vanillaRecordTypesProperties, false, false, default(string), "hello", default(int), 1, default(DateTime), new DateTime(2021, 2, 20), default(double), 3.5, new int[0], new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""CamelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, false, false, default(string), "hello", default(int), 1, default(DateTime), new DateTime(2021, 2, 20), default(double), 3.5, new int[0], new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""CamelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", "hello", default(int), 1, default(DateTime), new DateTime(2021, 2, 20), default(double), 3.5, new int[0], new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""CamelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", "hello", 10, 1, new DateTime(2021, 1, 20), new DateTime(2021, 2, 20), 100, 3.5, new int[] { 1, 2 }, new[] { 1, 2, 3 }, @"{ ""ParentStringProperty"": ""hello"", ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":""2021-02-20T00:00:00"", ""CamelCaseProperty"": 3.5 }, ""Values"": [ 1, 2, 3 ] }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", "world", 10, 10, new DateTime(2021, 1, 20), new DateTime(2021, 1, 20), 100, 100, new int[] { 1, 2 }, new[] { 1, 2, 3 }, @"{ ""Values"": [ 1, 2, 3 ] }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", "world", 10, 10, new DateTime(2021, 1, 20), new DateTime(2021, 1, 20), 100, 100, new int[] { 1, 2, 3, 4 }, new[] { 1, 2, 3 }, @"{ ""Values"": [ 1, 2, 3 ] }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", "world", 10, 1, new DateTime(2021, 1, 20), new DateTime(2021, 1, 20), 100, 3.5, new int[0], new int[0], @"{ ""OtherDto"": { ""NumberProp"": 1, ""CamelCaseProperty"": 3.5 } }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", default(string), 10, 1, new DateTime(2021, 1, 20), default(DateTime?), 100, 3.5, new int[] { 1, 2 }, default(int[]), @"{ ""ParentStringProperty"": null, ""OtherDto"": { ""NumberProp"": 1, ""NullableDateTimeProperty"":null, ""CamelCaseProperty"": 3.5 }, ""Values"": null }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", default(string), 10, 10, new DateTime(2021, 1, 20), default(DateTime?), 100, 3.5, new int[] { 1, 2 }, default(int[]), @"{ ""ParentStringProperty"": null, ""OtherDto"": { ""NullableDateTimeProperty"":null, ""CamelCaseProperty"": 3.5 }, ""Values"": null }" });
            _data.Add(new object[] { _vanillaRecordTypesProperties, true, true, "world", "world", 5, 0, default(DateTime), default(DateTime), 10, 0, new int[0], new int[0], @"{ ""OtherDto"": { ""NumberProp"": null, ""CamelCaseProperty"": null } }" });
        }

        private readonly List<object[]> _data = new List<object[]>();

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly string _vanillaRecordTypesProperties = @"
namespace TestCode
{
    public record SubDto(System.Int32 NumberProp, System.DateTime? NullableDateTimeProperty, System.Double CamelCaseProperty);

    public record ParentDto(System.String ParentStringProperty, SubDto OtherDto, System.Collections.Generic.IEnumerable<int> Values);

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<ParentDto> data)
        {
        }
    }
}
";
    }
}


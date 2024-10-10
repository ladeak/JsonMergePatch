using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class DictionarySerializationTests
    {
        [Theory]
        [ClassData(typeof(DictionarySerializationData))]
        public void ApplyPatch_PropertyToDelete_SetsNullOnTarget(
            string source,
            object input,
            object expected,
            string jsonInput)
        {
            var compilation = CreateWrappedTypeCompilation(source);
            var outputAssembly = SourceBuilder.EmitToAssembly(compilation);

            var dtoWrappedMetadata = outputAssembly.GetType("LaDeak.JsonMergePatch.Generated.SafeTestCode.DtoWrapped");
            var targetMetadata = outputAssembly.GetType("TestCode.Dto");
            var target = targetMetadata.GetConstructor(new Type[0]).Invoke(null);
            targetMetadata.GetProperty("Values").SetValue(target, input);

            var sut = JsonSerializer.Deserialize(jsonInput, dtoWrappedMetadata);
            var patchedParent = dtoWrappedMetadata.GetMethod("ApplyPatch").Invoke(sut, new[] { target });

            Assert.Equal(expected, targetMetadata.GetProperty("Values").GetValue(patchedParent));
        }

        private static Compilation CreateWrappedTypeCompilation(string source)
        {
            Compilation inputCompilation = CreateCompilation(source);
            JsonMergePatchSourceGenerator generator = new JsonMergePatchSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            return outputCompilation;
        }

        private static Compilation CreateCompilation(string source) => SourceBuilder.Compile(source).Compilation;
    }

    public class DictionarySerializationData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] { ValueType, new Dictionary<string, int> { { "one", 1 }, { "two", 2 }, { "three", 3 } }, new Dictionary<string, int> { { "one", 1 }, { "three", 33 }, { "four", 4 } }, @"{ ""Values"": { ""two"" : null, ""three"" : 33, ""four"" : 4 } }"  },
            new object[] { ReferenceType, new Dictionary<string, string> { { "one", "1" }, { "two", "2" }, { "three", "3" } }, new Dictionary<string, string> { { "one", "1" }, { "three", "33" }, { "four", "4" } }, @"{ ""Values"": { ""two"" : null, ""three"" : ""33"", ""four"" : ""4"" } }"  },
            new object[] { NullableReferenceType, new Dictionary<string, string?> { { "one", "1" }, { "two", "2" }, { "three", null } }, new Dictionary<string, string?> { { "one", "1" }, { "three", "33" }, { "four", "4" } }, @"{ ""Values"": { ""two"" : null, ""three"" : ""33"", ""four"" : ""4"" } }"  },
            new object[] { NullableType, new Dictionary<string, int?> { { "one", 1 }, { "two", 2 }, { "three", null } }, new Dictionary<string, int?> { { "one", 1 }, { "three", 33 }, { "four", 4 } }, @"{ ""Values"": { ""two"" : null, ""three"" : 33, ""four"" : 4 } }"  },
            new object[] { NullableType, new Dictionary<string, int?> { { "two", null }, { "three", null } }, new Dictionary<string, int?> { { "two", null } }, @"{ ""Values"": { ""three"" : null } }"  },
            new object[] { RecordWithDictionaryType, new Dictionary<string, string> { { "one", "1" }, { "two", "2" }, { "three", "3" } }, new Dictionary<string, string> { { "one", "1" }, { "three", "33" }, { "four", "4" } }, @"{ ""Values"": { ""two"" : null, ""three"" : ""33"", ""four"" : ""4"" } }"  },
            new object[] { InitOnlyDictionaryOnRecordType, new Dictionary<string, string> { { "one", "1" }, { "two", "2" }, { "three", "3" } }, new Dictionary<string, string> { { "one", "1" }, { "three", "33" }, { "four", "4" } }, @"{ ""Values"": { ""two"" : null, ""three"" : ""33"", ""four"" : ""4"" } }"  },
            new object[] { InitOnlyDictionaryOnClassType, new Dictionary<string, string> { { "one", "1" }, { "two", "2" }, { "three", "3" } }, new Dictionary<string, string> { { "one", "1" }, { "three", "33" }, { "four", "4" } }, @"{ ""Values"": { ""two"" : null, ""three"" : ""33"", ""four"" : ""4"" } }"  },
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private const string NullableType = @"
namespace TestCode
{
    [LaDeak.JsonMergePatch.Abstractions.Patchable]
    public class Dto { public System.Collections.Generic.Dictionary<string, int?> Values { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";

        private const string ValueType = @"
namespace TestCode
{
    [LaDeak.JsonMergePatch.Abstractions.Patchable]
    public class Dto { public System.Collections.Generic.Dictionary<string, int> Values { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";


        private const string ReferenceType = @"
namespace TestCode
{
    [LaDeak.JsonMergePatch.Abstractions.Patchable]
    public class Dto { public System.Collections.Generic.Dictionary<string, string> Values { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";

        private const string NullableReferenceType = @"
namespace TestCode
{
    [LaDeak.JsonMergePatch.Abstractions.Patchable]
    public class Dto { public System.Collections.Generic.Dictionary<string, string?> Values { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";

        private const string RecordWithDictionaryType = @"
namespace TestCode
{
    [LaDeak.JsonMergePatch.Abstractions.Patchable]
    public record Dto { public System.Collections.Generic.Dictionary<string, string> Values { get; set; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";

        private const string InitOnlyDictionaryOnRecordType = @"
namespace TestCode
{
    [LaDeak.JsonMergePatch.Abstractions.Patchable]
    public record Dto { public System.Collections.Generic.Dictionary<string, string> Values { get; init; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";

        private const string InitOnlyDictionaryOnClassType = @"
namespace TestCode
{
    [LaDeak.JsonMergePatch.Abstractions.Patchable]
    public class Dto { public System.Collections.Generic.Dictionary<string, string> Values { get; init; } }

    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";

    }
}


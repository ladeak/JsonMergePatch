using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests;

public class TypeBuilderTests
{
    [Fact]
    public void BuildWrapperType_SourceCode_NotNull()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.NotNull(result.SourceCode);
    }

    [Fact]
    public void BuildWrapperType_FileName_UsesTypeName()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal("LaDeakJsonMergePatchSafeTestTypeWrapped", result.FileName);
    }

    [Fact]
    public void EmptyType_Returns_EmptyTypeAndNamespace()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void TypeWithField_Returns_EmptyTypeAndNamespace()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var members = ImmutableArray.Create<ISymbol>(
            Substitute.For<IFieldSymbol>(),
            Substitute.For<IEventSymbol>(),
            Substitute.For<ITypeParameterSymbol>(),
            Substitute.For<ITypeSymbol>(),
            Substitute.For<IParameterSymbol>(),
            Substitute.For<INamespaceSymbol>(),
            Substitute.For<IMethodSymbol>(),
            Substitute.For<IFunctionPointerTypeSymbol>(),
            Substitute.For<IAliasSymbol>(),
            Substitute.For<IArrayTypeSymbol>());
        typeSymbol.GetMembers().Returns(members);
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void ReadonlyWriteonlyStaticIndexerAbstract_Property_Ignored()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var readonlyProperty = Substitute.For<IPropertySymbol>();
        readonlyProperty.IsReadOnly.Returns(true);
        var abstractProperty = Substitute.For<IPropertySymbol>();
        abstractProperty.IsAbstract.Returns(true);
        var writeonlyProperty = Substitute.For<IPropertySymbol>();
        writeonlyProperty.IsWriteOnly.Returns(true);
        var staticProperty = Substitute.For<IPropertySymbol>();
        staticProperty.IsStatic.Returns(true);
        var indexerProperty = Substitute.For<IPropertySymbol>();
        indexerProperty.IsIndexer.Returns(true);
        var members = ImmutableArray.Create<ISymbol>(readonlyProperty, writeonlyProperty, staticProperty, indexerProperty, abstractProperty);
        typeSymbol.GetMembers().Returns(members);
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void ClassAttribute_Added_ToGeneratedClass()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var attributes = ImmutableArray.Create<AttributeData>(new TestAttribute(), new TestAttribute("Hello"));
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    [TestAttribute]
    [Hello]
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void ReadWriteProperty_Added_BackingFieldAndProperty()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetProperty("System", "String", "TestProp");
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.String? _testProp;
        public System.String? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp = TestProp;
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void MultipleReadWriteProperty_Added_BackingFieldsAndProperties()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var prop0 = GetProperty("System", "String", "TestProp0");
        var prop1 = GetProperty("System", "Int32", "TestProp1");
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(prop0, prop1));
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[2];
        }

        private System.String? _testProp0;
        public System.String? TestProp0
        {
            get { return _testProp0; }
            set
            {
                Properties[0] = true;
                _testProp0 = value;
            }
        }

        private System.Nullable<System.Int32> _testProp1;
        public System.Nullable<System.Int32> TestProp1
        {
            get { return _testProp1; }
            set
            {
                Properties[1] = true;
                _testProp1 = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp0 = TestProp0;
            if (Properties[1])
                input.TestProp1 = TestProp1.HasValue ? TestProp1.Value : default;
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void MultipleReadWriteProperty_AddsTypeDataToProcessSymbols()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var prop0 = GetProperty("Test", "Dto", "TestProp0");
        var prop1 = GetProperty("System", "Int32", "TestProp1");
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(prop0, prop1));
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[2];
        }

        private LaDeak.JsonMergePatch.Generated.SafeTest.DtoWrapped? _testProp0;
        public LaDeak.JsonMergePatch.Generated.SafeTest.DtoWrapped? TestProp0
        {
            get { return _testProp0; }
            set
            {
                Properties[0] = true;
                _testProp0 = value;
            }
        }

        private System.Nullable<System.Int32> _testProp1;
        public System.Nullable<System.Int32> TestProp1
        {
            get { return _testProp1; }
            set
            {
                Properties[1] = true;
                _testProp1 = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp0 = TestProp0?.ApplyPatch(input.TestProp0);
            if (Properties[1])
                input.TestProp1 = TestProp1.HasValue ? TestProp1.Value : default;
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void PropertyWitAttribute_Adds_AttributeOnProperty()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetProperty("System", "String", "TestProp", new TestAttribute("JsonPropertyName(\"temp\")"));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.String? _testProp;
        [JsonPropertyName(""temp"")]
        public System.String? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp = TestProp;
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void DictionaryWithValueProperty_CreatesPatchToPatchEachValue()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetGenericProperty("System.Collections.Generic", "Dictionary", "TestProp", GetType("System", "String"), GetType("System", "Int32"));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        typeSymbol.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? _testProp;
        public System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp ??= new();
            if(TestProp != null)
            {
                foreach(var item in TestProp)
                {
                    if(item.Value is null)
                        input.TestProp.Remove(item.Key);
                    else
                        input.TestProp[item.Key] = item.Value.Value;
                }
            }
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void MultipleDictionaryProperty_CreatesPatchToPatchEachValue()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property0 = GetGenericProperty("System.Collections.Generic", "Dictionary", "TestProp0", GetType("System", "String"), GetType("System", "Int32"));
        var property1 = GetGenericProperty("System.Collections.Generic", "Dictionary", "TestProp1", GetType("System", "String"), GetType("System", "Int32"));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property0, property1));
        typeSymbol.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[2];
        }

        private System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? _testProp0;
        public System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? TestProp0
        {
            get { return _testProp0; }
            set
            {
                Properties[0] = true;
                _testProp0 = value;
            }
        }

        private System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? _testProp1;
        public System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? TestProp1
        {
            get { return _testProp1; }
            set
            {
                Properties[1] = true;
                _testProp1 = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp0 ??= new();
            if (Properties[1])
                input.TestProp1 ??= new();
            if(TestProp0 != null)
            {
                foreach(var item in TestProp0)
                {
                    if(item.Value is null)
                        input.TestProp0.Remove(item.Key);
                    else
                        input.TestProp0[item.Key] = item.Value.Value;
                }
            }
            if(TestProp1 != null)
            {
                foreach(var item in TestProp1)
                {
                    if(item.Value is null)
                        input.TestProp1.Remove(item.Key);
                    else
                        input.TestProp1[item.Key] = item.Value.Value;
                }
            }
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void DictionaryWithReferenceProperty_CreatesPatchToPatchEachValue()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetGenericProperty("System.Collections.Generic", "Dictionary", "TestProp", GetType("System", "String"), GetType("System", "String"));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        typeSymbol.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.Collections.Generic.Dictionary<System.String, System.String?>? _testProp;
        public System.Collections.Generic.Dictionary<System.String, System.String?>? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp ??= new();
            if(TestProp != null)
            {
                foreach(var item in TestProp)
                {
                    if(item.Value is null)
                        input.TestProp.Remove(item.Key);
                    else
                        input.TestProp[item.Key] = item.Value;
                }
            }
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void DictionaryWithNullableValueProperty_CreatesPatchToPatchEachValue()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetGenericProperty("System.Collections.Generic", "Dictionary", "TestProp", GetType("System", "String"), GetNullableType(GetType("System", "Int32")));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        typeSymbol.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? _testProp;
        public System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp ??= new();
            if(TestProp != null)
            {
                foreach(var item in TestProp)
                {
                    if(item.Value is null)
                        input.TestProp.Remove(item.Key);
                    else
                        input.TestProp[item.Key] = item.Value;
                }
            }
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void DictionaryWithNullableReferenceProperty_CreatesPatchToPatchEachValue()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetGenericProperty("System.Collections.Generic", "Dictionary", "TestProp", GetType("System", "String"), GetNullableReferenceType(GetType("System", "String")));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        typeSymbol.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.Collections.Generic.Dictionary<System.String, System.String?>? _testProp;
        public System.Collections.Generic.Dictionary<System.String, System.String?>? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp ??= new();
            if(TestProp != null)
            {
                foreach(var item in TestProp)
                {
                    if(item.Value is null)
                        input.TestProp.Remove(item.Key);
                    else
                        input.TestProp[item.Key] = item.Value;
                }
            }
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void PropertiesInBaseTypeIncluded()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        var property = GetProperty("System", "String", "TestProp");

        var baseTypeSymbol = Substitute.For<INamedTypeSymbol>();
        baseTypeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var propertyBase = GetProperty("System", "Int32", "TestPropBase");
        baseTypeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(propertyBase));
        typeSymbol.BaseType.Returns(baseTypeSymbol);

        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        var attributes = ImmutableArray.Create<AttributeData>();
        typeSymbol.GetAttributes().Returns(attributes);
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[2];
        }

        private System.String? _testProp;
        public System.String? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        private System.Nullable<System.Int32> _testPropBase;
        public System.Nullable<System.Int32> TestPropBase
        {
            get { return _testPropBase; }
            set
            {
                Properties[1] = true;
                _testPropBase = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp = TestProp;
            if (Properties[1])
                input.TestPropBase = TestPropBase.HasValue ? TestPropBase.Value : default;
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void ListOfComplexType_CreatesPatchToPatchEachValue()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetGenericProperty("System.Collections.Generic", "List", "TestProp", GetType("Test", "Dto"));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        typeSymbol.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.Collections.Generic.List<LaDeak.JsonMergePatch.Generated.SafeTest.DtoWrapped>? _testProp;
        public System.Collections.Generic.List<LaDeak.JsonMergePatch.Generated.SafeTest.DtoWrapped>? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp = new();
            if(TestProp != null)
            {
                foreach(var item in TestProp)
                {
                    input.TestProp.Add(item?.ApplyPatch(null));
                }
            }
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    [Fact]
    public void ListOfKnownType_CreatesPatchToPatchEachValue()
    {
        var sut = new TypeBuilder();
        var typeSymbol = Substitute.For<INamedTypeSymbol>();
        typeSymbol.Name.Returns("TestType");
        typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
        var property = GetGenericProperty("System.Collections.Generic", "List", "TestProp", GetType("System", "String"));
        typeSymbol.GetMembers().Returns(ImmutableArray.Create<ISymbol>(property));
        typeSymbol.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        var result = sut.BuildWrapperType(new GeneratorClassInfo(typeSymbol, "SourceName"));
        Assert.Equal(
@"#nullable enable
namespace LaDeak.JsonMergePatch.Generated.Safe
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Abstractions.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.Collections.Generic.List<System.String>? _testProp;
        public System.Collections.Generic.List<System.String>? TestProp
        {
            get { return _testProp; }
            set
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp = TestProp;
            return input;
        }
    }
}
#nullable disable
", result.SourceCode);
    }

    private ITypeSymbol GetType(string namespaceName, string typeName)
    {
        var propertyTypeSymbol = Substitute.For<INamedTypeSymbol>();
        propertyTypeSymbol.Name.Returns(typeName);
        SpecialType specialType = GetSpecialTypeFlag(typeName);
        propertyTypeSymbol.SpecialType.Returns(specialType);
        propertyTypeSymbol.ToDisplayString().ReturnsForAnyArgs($"{namespaceName}.{typeName}");
        var namespaceSymbol = Substitute.For<INamespaceSymbol>();
        namespaceSymbol.ToDisplayString().Returns(namespaceName);
        propertyTypeSymbol.ContainingNamespace.Returns(namespaceSymbol);
        propertyTypeSymbol.IsValueType.Returns(GetIsValueTypeFlag(typeName));
        propertyTypeSymbol.NullableAnnotation.Returns(NullableAnnotation.NotAnnotated);
        return propertyTypeSymbol;
    }

    private INamedTypeSymbol GetNullableType(ITypeSymbol typeParameter)
    {
        var type = Substitute.For<INamedTypeSymbol>();
        type.IsGenericType.Returns(true);
        type.IsAnonymousType.Returns(false);
        type.IsAbstract.Returns(false);
        type.SpecialType.Returns(SpecialType.System_Nullable_T);
        type.TypeArguments.Returns(ImmutableArray.Create<ITypeSymbol>(typeParameter));
        string name = $"System.Nullable<{typeParameter.ToDisplayString()}>";
        type.ToDisplayString(GeneratedTypeFilter.SymbolFormat).ReturnsForAnyArgs(name);
        type.NullableAnnotation.Returns(NullableAnnotation.Annotated);
        return type;
    }

    private ITypeSymbol GetNullableReferenceType(ITypeSymbol typeParameter)
    {
        typeParameter.NullableAnnotation.Returns(NullableAnnotation.Annotated);
        return typeParameter;
    }

    private IPropertySymbol GetProperty(string namespaceName, string typeName, string name, AttributeData attribute = null)
    {
        var propertyTypeSymbol = GetType(namespaceName, typeName);
        var property = Substitute.For<IPropertySymbol>();
        property.Name.Returns(name);
        property.Type.Returns(propertyTypeSymbol);
        property.GetAttributes().Returns(attribute == null ? ImmutableArray.Create<AttributeData>() : ImmutableArray.Create(attribute));
        return property;
    }

    private IPropertySymbol GetGenericProperty(string namespaceName, string typeName, string name, ITypeSymbol propertyTypeSymbol0, ITypeSymbol propertyTypeSymbol1)
    {
        return GetGenericProperty(namespaceName, typeName, name, new[] { propertyTypeSymbol0, propertyTypeSymbol1 });
    }

    private IPropertySymbol GetGenericProperty(string namespaceName, string typeName, string name, ITypeSymbol propertyTypeSymbol0)
    {
        return GetGenericProperty(namespaceName, typeName, name, new[] { propertyTypeSymbol0 });
    }

    private IPropertySymbol GetGenericProperty(string namespaceName, string typeName, string name, ITypeSymbol[] genericTypes)
    {
        var propertyTypeSymbol = Substitute.For<INamedTypeSymbol>();
        propertyTypeSymbol.Name.Returns(typeName);
        SpecialType specialType = GetSpecialTypeFlag(typeName);
        propertyTypeSymbol.SpecialType.Returns(specialType);
        propertyTypeSymbol.ToDisplayString().ReturnsForAnyArgs($"{namespaceName}.{typeName}");
        var namespaceSymbol = Substitute.For<INamespaceSymbol>();
        namespaceSymbol.ToDisplayString().Returns(namespaceName);
        propertyTypeSymbol.ContainingNamespace.Returns(namespaceSymbol);
        propertyTypeSymbol.IsValueType.Returns(GetIsValueTypeFlag(typeName));
        propertyTypeSymbol.IsGenericType.Returns(true);
        propertyTypeSymbol.TypeArguments.Returns(ImmutableArray.Create<ITypeSymbol>(genericTypes, 0, genericTypes.Length));
        var property = Substitute.For<IPropertySymbol>();
        property.Name.Returns(name);
        property.Type.Returns(propertyTypeSymbol);
        property.GetAttributes().Returns(ImmutableArray.Create<AttributeData>());
        return property;
    }

    private SpecialType GetSpecialTypeFlag(string typeName) =>
        typeName switch
        {
            "Int32" => SpecialType.System_Int32,
            "String" => SpecialType.System_String,
            "Dictionary<System.String, System.Int32>" => SpecialType.None,
            _ => SpecialType.None,
        };

    private bool GetIsValueTypeFlag(string typeName) =>
        typeName switch
        {
            "Int32" => true,
            "String" => false,
            _ => false,
        };
}
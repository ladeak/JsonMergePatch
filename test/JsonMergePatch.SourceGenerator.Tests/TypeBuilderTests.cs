using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class TypeBuilderTests
    {
        [Fact]
        public void BuildWrapperType_SourceTypeName_ReturnedInResultsSourceTypeName()
        {
            var sut = new TypeBuilder();
            var typeSymbol = Substitute.For<ITypeSymbol>();
            typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
            var attributes = ImmutableArray.Create<AttributeData>();
            typeSymbol.GetAttributes().Returns(attributes);
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal("SourceName", result.SourceTypeFullName);
        }

        [Fact]
        public void BuildWrapperType_TargetTypeName_ReturnedTypeNameAndWrapped()
        {
            var sut = new TypeBuilder();
            var typeSymbol = Substitute.For<ITypeSymbol>();
            typeSymbol.Name.Returns("TestType");
            typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
            typeSymbol.ContainingNamespace.ToDisplayString().Returns("TestTypeNamespace");
            var attributes = ImmutableArray.Create<AttributeData>();
            typeSymbol.GetAttributes().Returns(attributes);
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal("LaDeak.JsonMergePatch.Generated.STestTypeNamespace.TestTypeWrapped", result.TargetTypeFullName);
        }

        [Fact]
        public void BuildWrapperType_ToProcessTypes_NotNull()
        {
            var sut = new TypeBuilder();
            var typeSymbol = Substitute.For<ITypeSymbol>();
            typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
            var attributes = ImmutableArray.Create<AttributeData>();
            typeSymbol.GetAttributes().Returns(attributes);
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.NotNull(result.ToProcessTypes);
        }

        [Fact]
        public void BuildWrapperType_SourceCode_NotNull()
        {
            var sut = new TypeBuilder();
            var typeSymbol = Substitute.For<ITypeSymbol>();
            typeSymbol.BaseType.Returns((INamedTypeSymbol)null);
            var attributes = ImmutableArray.Create<AttributeData>();
            typeSymbol.GetAttributes().Returns(attributes);
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal("LaDeakJsonMergePatchSTestTypeWrapped", result.FileName);
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
@"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch(SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
@"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch(SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
@"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch(SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
@"namespace LaDeak.JsonMergePatch.Generated.S
{
    [TestAttribute]
    [Hello]
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[0];
        }

        public override SourceName ApplyPatch(SourceName input)
        {
            input ??= new SourceName();
            return input;
        }
    }
}
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
@"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.String _testProp;
        public System.String TestProp
        {
            get { return _testProp; }
            init
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch(SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp = TestProp;
            return input;
        }
    }
}
", result.SourceCode);
            Assert.Contains(result.ToProcessTypes, x => x.ToDisplayString() == "System.String");
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
    @"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[2];
        }

        private System.String _testProp0;
        public System.String TestProp0
        {
            get { return _testProp0; }
            init
            {
                Properties[0] = true;
                _testProp0 = value;
            }
        }

        private System.Nullable<System.Int32> _testProp1;
        public System.Nullable<System.Int32> TestProp1
        {
            get { return _testProp1; }
            init
            {
                Properties[1] = true;
                _testProp1 = value;
            }
        }

        public override SourceName ApplyPatch(SourceName input)
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
    @"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[2];
        }

        private LaDeak.JsonMergePatch.Generated.STest.DtoWrapped _testProp0;
        public LaDeak.JsonMergePatch.Generated.STest.DtoWrapped TestProp0
        {
            get { return _testProp0; }
            init
            {
                Properties[0] = true;
                _testProp0 = value;
            }
        }

        private System.Nullable<System.Int32> _testProp1;
        public System.Nullable<System.Int32> TestProp1
        {
            get { return _testProp1; }
            init
            {
                Properties[1] = true;
                _testProp1 = value;
            }
        }

        public override SourceName ApplyPatch(SourceName input)
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
", result.SourceCode);
            Assert.Contains(result.ToProcessTypes, x => x.ToDisplayString() == "Test.Dto");
            Assert.Contains(result.ToProcessTypes, x => x.ToDisplayString() == "System.Int32");
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
@"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[1];
        }

        private System.String _testProp;
        [JsonPropertyName(""temp"")]
        public System.String TestProp
        {
            get { return _testProp; }
            init
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        public override SourceName ApplyPatch(SourceName input)
        {
            input ??= new SourceName();
            if (Properties[0])
                input.TestProp = TestProp;
            return input;
        }
    }
}
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
            var result = sut.BuildWrapperType(typeSymbol, "SourceName");
            Assert.Equal(
@"namespace LaDeak.JsonMergePatch.Generated.S
{
    public class TestTypeWrapped : LaDeak.JsonMergePatch.Patch<SourceName>
    {
        public TestTypeWrapped()
        {
            Properties = new bool[2];
        }

        private System.String _testProp;
        public System.String TestProp
        {
            get { return _testProp; }
            init
            {
                Properties[0] = true;
                _testProp = value;
            }
        }

        private System.Nullable<System.Int32> _testPropBase;
        public System.Nullable<System.Int32> TestPropBase
        {
            get { return _testPropBase; }
            init
            {
                Properties[1] = true;
                _testPropBase = value;
            }
        }

        public override SourceName ApplyPatch(SourceName input)
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
", result.SourceCode);
            Assert.Contains(result.ToProcessTypes, x => x.ToDisplayString() == "System.String");
        }

        private IPropertySymbol GetProperty(string namespaceName, string typeName, string name, AttributeData attribute = null)
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
            var property = Substitute.For<IPropertySymbol>();
            property.Name.Returns(name);
            property.Type.Returns(propertyTypeSymbol);
            property.GetAttributes().Returns(attribute == null ? ImmutableArray.Create<AttributeData>() : ImmutableArray.Create(attribute));
            return property;
        }

        private SpecialType GetSpecialTypeFlag(string typeName) =>
            typeName switch
            {
                "Int32" => SpecialType.System_Int32,
                "String" => SpecialType.System_String,
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
}
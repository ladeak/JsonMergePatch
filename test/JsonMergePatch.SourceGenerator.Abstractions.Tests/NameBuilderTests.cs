using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests;

public class NameBuilderTests
{
    [Fact]
    public void GetName_ReturnsTypeName_WithWrappedSuffix()
    {
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.Name.Returns("TestName");
        var result = NameBuilder.GetName(typeSymbol);
        Assert.Equal("TestNameWrapped", result);
    }

    [Fact]
    public void GetNamespaceExtension_Returns_SafeExtension()
    {
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.ContainingNamespace.ToDisplayString().Returns("Test.Namespace");
        var result = NameBuilder.GetNamespaceExtension(typeSymbol);
        Assert.Equal("SafeTest.Namespace", result);
    }

    [Fact]
    public void GetNamespace_Returns_Namespace_SafeExtension()
    {
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.ContainingNamespace.ToDisplayString().Returns("Test.Namespace");
        var result = NameBuilder.GetNamespace(typeSymbol);
        Assert.Equal("LaDeak.JsonMergePatch.Generated.SafeTest.Namespace", result);
    }


    [Fact]
    public void GetNamespaceOnString_Returns_Namespace_SafeExtension()
    {
        var result = NameBuilder.GetNamespace("Test.Namespace");
        Assert.Equal("LaDeak.JsonMergePatch.Generated.SafeTest.Namespace", result);
    }

    [Fact]
    public void GetGetFullTypeName_Returns_Namespace_SafeExtension_TypeNameWrapped()
    {
        var typeSymbol = Substitute.For<ITypeSymbol>();
        typeSymbol.Name.Returns("TestName");
        typeSymbol.ContainingNamespace.ToDisplayString().Returns("Test.Namespace");
        var result = NameBuilder.GetFullTypeName(typeSymbol);
        Assert.Equal("LaDeak.JsonMergePatch.Generated.SafeTest.Namespace.TestNameWrapped", result);
    }
}

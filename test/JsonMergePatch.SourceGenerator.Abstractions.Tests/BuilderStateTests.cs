using System;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests;

public class BuilderStateTests
{
    [Fact]
    public void Increment_Increases_Identation()
    {
        var sut = new BuilderState(GeneratorClassInfo.Default);
        var state = sut.IncrementIdentation();
        Assert.Equal(0, sut.Identation);
        Assert.Equal(4, state.Identation);
    }

    [Fact]
    public void Increment_DoesNotChange_GeneratorClassInfo_Builder_ToProcessTypeSymbols()
    {
        var sut = new BuilderState(GeneratorClassInfo.Default);
        var state = sut.IncrementIdentation();
        Assert.Same(sut.Builder, state.Builder);
        Assert.Same(sut.TypeInfo, state.TypeInfo);
    }

    [Fact]
    public void AppendLine_Adds_EmptyLine()
    {
        var sut = new BuilderState(GeneratorClassInfo.Default);
        sut.AppendLine();
        Assert.Equal(Environment.NewLine, sut.Builder.ToString());
    }

    [Fact]
    public void AppendLineWithText_Adds_Text()
    {
        var sut = new BuilderState(GeneratorClassInfo.Default);
        sut.AppendLine("hello");
        Assert.Equal($"hello{Environment.NewLine}", sut.Builder.ToString());
    }

    [Fact]
    public void AppendLineWithText_Adds_IdentationSpacesAndText()
    {
        var baseIdentation = new BuilderState(GeneratorClassInfo.Default);
        var sut = baseIdentation.IncrementIdentation();
        sut.AppendLine("world");
        Assert.Equal($"    world{Environment.NewLine}", sut.Builder.ToString());
    }

    [Fact]
    public void Builder_AppendsLine_BasedOnItsIdentation()
    {
        var baseIdentation = new BuilderState(GeneratorClassInfo.Default);
        baseIdentation.AppendLine("hello");
        var sut = baseIdentation.IncrementIdentation();
        sut.AppendLine("world");
        baseIdentation.AppendLine("!");
        Assert.Equal($"hello{Environment.NewLine}    world{Environment.NewLine}!{Environment.NewLine}", sut.Builder.ToString());
    }

    [Fact]
    public void DoubleIncrement_Adds_DoubleIdentation()
    {
        var sut = new BuilderState(GeneratorClassInfo.Default).IncrementIdentation().IncrementIdentation();
        sut.AppendLine("hello");
        Assert.Equal($"        hello{Environment.NewLine}", sut.Builder.ToString());
    }

}

using System;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class BuilderStateTests
    {
        [Fact]
        public void NewBuilder_Initialized()
        {
            var typeInfo = new TypeInformation();
            var sut = new BuilderState(typeInfo);
            Assert.Same(typeInfo, sut.TypeInfo);
            Assert.Equal(0, sut.Identation);
            Assert.Empty(sut.ToProcessTypeSymbols);
            Assert.NotNull(sut.Builder);
        }

        [Fact]
        public void Increment_Increases_Identation()
        {
            var sut = new BuilderState(new TypeInformation());
            var state = sut.IncrementIdentation();
            Assert.Equal(0, sut.Identation);
            Assert.Equal(4, state.Identation);
        }

        [Fact]
        public void Increment_DoesNotChange_TypeInformation_Builder_ToProcessTypeSymbols()
        {
            var sut = new BuilderState(new TypeInformation());
            var state = sut.IncrementIdentation();
            Assert.Same(sut.Builder, state.Builder);
            Assert.Same(sut.TypeInfo, state.TypeInfo);
            Assert.Same(sut.ToProcessTypeSymbols, state.ToProcessTypeSymbols);
        }

        [Fact]
        public void AppendLine_Adds_EmptyLine()
        {
            var sut = new BuilderState(new TypeInformation());
            sut.AppendLine();
            Assert.Equal(Environment.NewLine, sut.Builder.ToString());
        }

        [Fact]
        public void AppendLineWithText_Adds_Text()
        {
            var sut = new BuilderState(new TypeInformation());
            sut.AppendLine("hello");
            Assert.Equal($"hello{Environment.NewLine}", sut.Builder.ToString());
        }

        [Fact]
        public void AppendLineWithText_Adds_IdentationSpacesAndText()
        {
            var baseIdentation = new BuilderState(new TypeInformation());
            var sut = baseIdentation.IncrementIdentation();
            sut.AppendLine("world");
            Assert.Equal($"    world{Environment.NewLine}", sut.Builder.ToString());
        }

        [Fact]
        public void Builder_AppendsLine_BasedOnItsIdentation()
        {
            var baseIdentation = new BuilderState(new TypeInformation());
            baseIdentation.AppendLine("hello");
            var sut = baseIdentation.IncrementIdentation();
            sut.AppendLine("world");
            baseIdentation.AppendLine("!");
            Assert.Equal($"hello{Environment.NewLine}    world{Environment.NewLine}!{Environment.NewLine}", sut.Builder.ToString());
        }

        [Fact]
        public void DoubleIncrement_Adds_DoubleIdentation()
        {
            var sut = new BuilderState(new TypeInformation()).IncrementIdentation().IncrementIdentation();
            sut.AppendLine("hello");
            Assert.Equal($"        hello{Environment.NewLine}", sut.Builder.ToString());
        }

    }
}

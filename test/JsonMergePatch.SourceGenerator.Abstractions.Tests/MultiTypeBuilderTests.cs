using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class MultiTypeBuilderTests
    {
        private const string SimpleTestCode = @"
namespace TestCode1
{
    public class Dto { public int Property { get; set; }  }
    public class Program
    {
        public void SomeMethod(LaDeak.JsonMergePatch.Abstractions.Patch<Dto> data)
        {
        }
    }
}
";
        [Fact]
        public void MissingDependency_AtConstruction_ThrowsArgumentNullException()
        {
            Compilation inputCompilation = CreateCompilation(SimpleTestCode);
            Assert.Throws<ArgumentNullException>(() => new MultiTypeBuilder(null, inputCompilation, Substitute.For<ITypeBuilder>(), Substitute.For<IPatchParametersWalker>()));
            Assert.Throws<ArgumentNullException>(() => new MultiTypeBuilder(new[] { CSharpSyntaxTree.ParseText(SimpleTestCode) }, null, Substitute.For<ITypeBuilder>(), Substitute.For<IPatchParametersWalker>()));
            Assert.Throws<ArgumentNullException>(() => new MultiTypeBuilder(new[] { CSharpSyntaxTree.ParseText(SimpleTestCode) }, inputCompilation, null, Substitute.For<IPatchParametersWalker>()));
            Assert.Throws<ArgumentNullException>(() => new MultiTypeBuilder(new[] { CSharpSyntaxTree.ParseText(SimpleTestCode) }, inputCompilation, Substitute.For<ITypeBuilder>(), null));
        }

        [Fact]
        public void WithAllDependencies_AtConstruction_CreatesMultiTypeBuilder()
        {
            Compilation inputCompilation = CreateCompilation(SimpleTestCode);
            var sut = new MultiTypeBuilder(new[] { CSharpSyntaxTree.ParseText(SimpleTestCode) }, inputCompilation, Substitute.For<ITypeBuilder>(), Substitute.For<IPatchParametersWalker>());
        }

        [Fact]
        public void SimpleTestCode_Generate_ReturnsWrappedDtoType()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Single(results);
        }

        [Fact]
        public void MultipleTypesFound_Generate_ReturnsWrappedForAllTypes()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>(), Substitute.For<ITypeSymbol>(), Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Equal(3, results.Count());
        }

        [Fact]
        public void MultipleSyntaxTrees_Generate_ReturnsWrappedForAllTrees()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(new[] { tree, tree, tree }, inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Equal(3, results.Count());
        }

        [Fact]
        public void GeneratedTypeReturnInnerType_Generate_ReturnsWrappedTypeForInner()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() { Substitute.For<ITypeSymbol>() } }, new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void MultipleInnerType_Generate_ReturnsWrappedMultipleInnerType()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>())
                .Returns(
                new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() { Substitute.For<ITypeSymbol>(), Substitute.For<ITypeSymbol>() } },
                new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() },
                new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Equal(3, results.Count());
        }

        [Fact]
        public void NoSyntaxTree_Generate_ReturnsEmptyResults()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(Enumerable.Empty<SyntaxTree>(), inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Empty(results);
            walker.DidNotReceive().Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>());
        }

        [Fact]
        public void WalkerReturnsNoFinding_Generate_ReturnsEmptyResults()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(Enumerable.Empty<ITypeSymbol>());
            var typeBuilder = Substitute.For<ITypeBuilder>();
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Empty(results);
            typeBuilder.DidNotReceive().BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>());
        }

        [Fact]
        public void MultipleGenerateCalls_ReturnsEqualResults()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var results0 = sut.Generate();
            var results1 = sut.Generate();

            Assert.Equal(results0.Single(), results1.Single());
        }

        [Fact]
        public void GeneratedWrappersSource_SameAsReturnedByTypeBuilder()
        {
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.Compile(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            var typeBuilderResult = new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>(), FileName = "file.cs", SourceCode = "namespace TestNameSpace { }", SourceTypeFullName = "Source.FullName", TargetTypeFullName = "Target.FullName" };
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(typeBuilderResult);
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var result = sut.Generate().Single();

            Assert.Equal(typeBuilderResult.FileName, result.FileName);
            Assert.Equal(typeBuilderResult.SourceCode, result.SourceCode);
            Assert.Equal(typeBuilderResult.SourceTypeFullName, result.SourceTypeFullName);
            Assert.Equal(typeBuilderResult.TargetTypeFullName, result.TargetTypeFullName);
        }

        private static Compilation CreateCompilation(string source) => SourceBuilder.Compile(source).Compilation;

    }
}

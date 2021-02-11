using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaDeak.JsonMergePatch.SourceGenerator;
using LaDeak.JsonMergePatch.SourceGenerator.Tests;
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
        public void SomeMethod(LaDeak.JsonMergePatch.Patch<Dto> data)
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
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.CompileMvc(SimpleTestCode);
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
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.CompileMvc(SimpleTestCode);
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
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.CompileMvc(SimpleTestCode);
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
            (Compilation inputCompilation, SyntaxTree tree) = SourceBuilder.CompileMvc(SimpleTestCode);
            var walker = Substitute.For<IPatchParametersWalker>();
            walker.Process(Arg.Any<SyntaxNode>(), Arg.Any<SemanticModel>()).Returns(new[] { Substitute.For<ITypeSymbol>() });
            var typeBuilder = Substitute.For<ITypeBuilder>();
            typeBuilder.BuildWrapperType(Arg.Any<ITypeSymbol>(), Arg.Any<string>()).Returns(new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() { Substitute.For<ITypeSymbol>() } }, new GeneratedWrapper() { ToProcessTypes = new List<ITypeSymbol>() });
            var sut = new MultiTypeBuilder(new[] { tree }, inputCompilation, typeBuilder, walker);

            var results = sut.Generate();

            Assert.Equal(2, results.Count());
        }

        private static Compilation CreateCompilation(string source) => SourceBuilder.CompileMvc(source).Compilation;

    }
}

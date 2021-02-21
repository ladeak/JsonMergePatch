using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NSubstitute;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class TestAttribute : AttributeData
    {
        private readonly string _name;

        public TestAttribute() : this(nameof(TestAttribute)) { }

        public TestAttribute(string name)
        {
            _name = name;
        }

        protected override INamedTypeSymbol CommonAttributeClass => Substitute.For<INamedTypeSymbol>();

        protected override IMethodSymbol CommonAttributeConstructor => Substitute.For<IMethodSymbol>();

        protected override SyntaxReference CommonApplicationSyntaxReference => Substitute.For<SyntaxReference>();

        protected override ImmutableArray<TypedConstant> CommonConstructorArguments => ImmutableArray.Create<TypedConstant>();

        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments => ImmutableArray.Create<KeyValuePair<string, TypedConstant>>();

        public override string ToString() => _name;
    }
}
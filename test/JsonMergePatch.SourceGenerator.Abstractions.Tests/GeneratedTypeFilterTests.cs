using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions.Tests
{
    public class GeneratedTypeFilterTests
    {
        [Fact]
        public void GenericType_TryGetGeneratableType_ReturnsTrueAndInnerType()
        {
            INamedTypeSymbol innerTypeArgument = GetGeneratableType();
            var input = Substitute.For<INamedTypeSymbol>();
            input.IsGenericType.Returns(true);
            input.IsUnboundGenericType.Returns(false);
            input.TypeArguments.Returns(ImmutableArray.Create<ITypeSymbol>(innerTypeArgument));
            input.SpecialType.Returns(SpecialType.System_Collections_Generic_IEnumerable_T);

            var result = GeneratedTypeFilter.TryGetGeneratableType(input, out var outParam);

            Assert.True(result);
            Assert.Equal(innerTypeArgument, outParam);
        }

        [Fact]
        public void NullableTypeInDictionaryType_TryGetGeneratableType_ReturnsTrueAndValueType()
        {
            var dictionaryKeyType = GetGeneratableType();
            var dictionaryValueType = GetGeneratableType();
            INamedTypeSymbol nullableValueTypeArgument = GetNullableType(dictionaryValueType);
            var input = Substitute.For<INamedTypeSymbol>();
            input.IsGenericType.Returns(true);
            input.IsUnboundGenericType.Returns(false);
            input.TypeArguments.Returns(ImmutableArray.Create<ITypeSymbol>(dictionaryKeyType, nullableValueTypeArgument));
            input.SpecialType.Returns(SpecialType.None);
            input.Name.Returns("Dictionary");
            input.ContainingNamespace.ToDisplayString().Returns("System.Collections.Generic");

            var result = GeneratedTypeFilter.TryGetGeneratableType(input, out var outParam);

            Assert.True(result);
            Assert.Equal(dictionaryValueType, outParam);
        }

        [Fact]
        public void DictionaryType_TryGetGeneratableType_ReturnsTrueAndValueType()
        {
            var dictionaryKeyType = GetGeneratableType();
            var dictionaryValueType = GetGeneratableType();
            var input = Substitute.For<INamedTypeSymbol>();
            input.IsGenericType.Returns(true);
            input.IsUnboundGenericType.Returns(false);
            input.TypeArguments.Returns(ImmutableArray.Create<ITypeSymbol>(dictionaryKeyType, dictionaryValueType));
            input.SpecialType.Returns(SpecialType.None);
            input.Name.Returns("Dictionary");
            input.ContainingNamespace.ToDisplayString().Returns("System.Collections.Generic");

            var result = GeneratedTypeFilter.TryGetGeneratableType(input, out var outParam);

            Assert.True(result);
            Assert.Equal(dictionaryValueType, outParam);
        }

        [Theory]
        [MemberData(nameof(GetTestTypes))]
        public void NonGenericType_TryGetGeneratableType_ReturnsExpected(INamedTypeSymbol input, bool expected)
        {
            var result = GeneratedTypeFilter.TryGetGeneratableType(input, out var outParam);
            Assert.Equal(expected, result);
            Assert.Equal(input, outParam);
        }

        [Theory]
        [MemberData(nameof(GetTestTypes))]
        public void GenericType_IsGeneratableType_ReturnsExpected(INamedTypeSymbol input, bool expected)
        {
            Assert.Equal(expected, GeneratedTypeFilter.IsGeneratableType(input));
        }

        public static IEnumerable<object[]> GetTestTypes()
        {
            yield return new object[] { GetGeneratableType(), true };
            yield return new object[] { GetSpecialType(), false };
            yield return new object[] { GetGenericType(), false };
            yield return new object[] { GetAnonymusType(), false };
            yield return new object[] { GetAbstractType(), false };
        }

        private static INamedTypeSymbol GetGeneratableType()
        {
            var innerTypeArgument = Substitute.For<INamedTypeSymbol>();
            innerTypeArgument.IsGenericType.Returns(false);
            innerTypeArgument.IsAnonymousType.Returns(false);
            innerTypeArgument.IsAbstract.Returns(false);
            innerTypeArgument.SpecialType.Returns(SpecialType.None);
            return innerTypeArgument;
        }

        private static INamedTypeSymbol GetSpecialType()
        {
            var input = Substitute.For<INamedTypeSymbol>();
            input.IsGenericType.Returns(false);
            input.IsAnonymousType.Returns(false);
            input.IsAbstract.Returns(true);
            input.SpecialType.Returns(SpecialType.System_Int32);
            return input;
        }

        private static INamedTypeSymbol GetGenericType()
        {
            var input = Substitute.For<INamedTypeSymbol>();
            input.IsGenericType.Returns(true);
            input.TypeArguments.Returns(ImmutableArray.Create<ITypeSymbol>());
            input.IsAnonymousType.Returns(false);
            input.IsAbstract.Returns(false);
            input.SpecialType.Returns(SpecialType.None);
            return input;
        }

        private static INamedTypeSymbol GetAnonymusType()
        {
            var input = Substitute.For<INamedTypeSymbol>();
            input.IsGenericType.Returns(false);
            input.IsAnonymousType.Returns(true);
            input.IsAbstract.Returns(false);
            input.SpecialType.Returns(SpecialType.None);
            return input;
        }

        private static INamedTypeSymbol GetAbstractType()
        {
            var input = Substitute.For<INamedTypeSymbol>();
            input.IsGenericType.Returns(false);
            input.IsAnonymousType.Returns(false);
            input.IsAbstract.Returns(true);
            input.SpecialType.Returns(SpecialType.None);
            return input;
        }

        private static INamedTypeSymbol GetNullableType(INamedTypeSymbol typeParameter)
        {
            var type = Substitute.For<INamedTypeSymbol>();
            type.IsGenericType.Returns(true);
            type.IsAnonymousType.Returns(false);
            type.IsAbstract.Returns(false);
            type.SpecialType.Returns(SpecialType.System_Nullable_T);
            type.TypeArguments.Returns(ImmutableArray.Create<ITypeSymbol>(typeParameter));
            return type;
        }
    }
}

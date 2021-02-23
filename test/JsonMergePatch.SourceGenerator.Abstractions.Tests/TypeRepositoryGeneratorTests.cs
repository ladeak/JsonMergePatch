using System;
using System.Reflection;
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.SourceGenerator;
using LaDeak.JsonMergePatch.SourceGenerator.Tests;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LaDeak.JsonMergePatch.AspNetCore.Tests
{
    public class TypeRepositoryGeneratorTests
    {
        [Fact]
        public void EmptyInput_CreateRepository_GeneratesEmptyConstructor()
        {
            var sut = new TypeRepositoryGenerator();
            var result = sut.CreateTypeRepository(null);
            Assert.Equal(@"
namespace LaDeak.JsonMergePatch.Generated
{
    public class TypeRepository : LaDeak.JsonMergePatch.Abstractions.ITypeRepository
    {       
        private System.Collections.Generic.Dictionary<System.Type, System.Type> _repository = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        private TypeRepository()
        {

        }

        public void Add<TSource, TWrapper>() where TWrapper : LaDeak.JsonMergePatch.Abstractions.Patch<TSource>
        {
            _repository.Add(typeof(TSource), typeof(TWrapper));
        }

        public bool TryGet(System.Type source, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out System.Type wrapper)
        {
            return _repository.TryGetValue(source, out wrapper);
        }

        public static LaDeak.JsonMergePatch.Abstractions.ITypeRepository Instance { get; } = new TypeRepository();
    }
}", result);
        }

        [Fact]
        public void WithTypes_CreateRepository_GeneratesEmptyConstructor()
        {
            var sut = new TypeRepositoryGenerator();
            var result = sut.CreateTypeRepository(new[] { ("TestDto0", "TestDto0Wrapped"), ("TestDto1", "TestDto1Wrapped") });
            Assert.Equal(@"
namespace LaDeak.JsonMergePatch.Generated
{
    public class TypeRepository : LaDeak.JsonMergePatch.Abstractions.ITypeRepository
    {       
        private System.Collections.Generic.Dictionary<System.Type, System.Type> _repository = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

        private TypeRepository()
        {
            Add<TestDto0, TestDto0Wrapped>();
            Add<TestDto1, TestDto1Wrapped>();

        }

        public void Add<TSource, TWrapper>() where TWrapper : LaDeak.JsonMergePatch.Abstractions.Patch<TSource>
        {
            _repository.Add(typeof(TSource), typeof(TWrapper));
        }

        public bool TryGet(System.Type source, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out System.Type wrapper)
        {
            return _repository.TryGetValue(source, out wrapper);
        }

        public static LaDeak.JsonMergePatch.Abstractions.ITypeRepository Instance { get; } = new TypeRepository();
    }
}", result);
        }

        [Fact]
        public void EmptyInput_CreateRepository_Compiles()
        {
            var sut = new TypeRepositoryGenerator();
            _ = Compile(sut.CreateTypeRepository(null));
        }

        [Fact]
        public void Compiled_Repository_ReturnsInstanceSingleton()
        {
            var sut = new TypeRepositoryGenerator();
            var typeRepository = GetTypeRepository(Compile(sut.CreateTypeRepository(null)));
            Assert.NotNull(typeRepository);
        }

        [Fact]
        public void EmptyRepository_Add_DoesNotThrow()
        {
            var generator = new TypeRepositoryGenerator();
            var sut = GetTypeRepository(Compile(generator.CreateTypeRepository(null)));
            sut.Add<TestDto, TestDtoWrapped>();
        }

        [Fact]
        public void EmptyRepository_AddTwiceSameType_ThrowsException()
        {
            var generator = new TypeRepositoryGenerator();
            var sut = GetTypeRepository(Compile(generator.CreateTypeRepository(null)));
            sut.Add<TestDto, TestDtoWrapped>();
            Assert.Throws<ArgumentException>(() => sut.Add<TestDto, TestDtoWrapped>());
        }

        [Fact]
        public void EmptyRepository_TryGet_ReturnsFalse()
        {
            var generator = new TypeRepositoryGenerator();
            var sut = GetTypeRepository(Compile(generator.CreateTypeRepository(null)));
            Assert.False(sut.TryGet(typeof(TestDto), out _));
        }

        [Fact]
        public void RepositoryWitTestDto_TryGet_ReturnsTrue()
        {
            var generator = new TypeRepositoryGenerator();
            var sut = GetTypeRepository(Compile(generator.CreateTypeRepository(null)));
            sut.Add<TestDto, TestDtoWrapped>();
            Assert.True(sut.TryGet(typeof(TestDto), out _));
        }

        [Fact]
        public void RepositoryWitTestDto_TryGet_ReturnsRegisteredType()
        {
            var generator = new TypeRepositoryGenerator();
            var sut = GetTypeRepository(Compile(generator.CreateTypeRepository(null)));
            sut.Add<TestDto, TestDtoWrapped>();
            sut.TryGet(typeof(TestDto), out var result);
            Assert.Equal(typeof(TestDtoWrapped), result);
        }

        [Fact]
        public void RepositoryWitTestDto_TryGet_ReturnsPatchOfUserType()
        {
            var generator = new TypeRepositoryGenerator();
            var sut = GetTypeRepository(Compile(generator.CreateTypeRepository(null)));
            sut.Add<TestDto, TestDtoWrapped>();
            sut.TryGet(typeof(TestDto), out var result);
            Assert.True(typeof(Patch<TestDto>).IsAssignableFrom(result));
        }

        [Fact]
        public void GeneratedTypesInRepository_TryGet_ReturnsRegisteredType()
        {
            var generator = new TypeRepositoryGenerator();
            var sut = GetTypeRepository(Compile(generator.CreateTypeRepository(new[] { (typeof(TestDto).FullName, typeof(TestDtoWrapped).FullName) })));
            sut.TryGet(typeof(TestDto), out var result);
            Assert.Equal(typeof(TestDtoWrapped), result);
        }

        [Fact]
        public void SameTypeRegisteredTwice_InstanceProperty_Throws()
        {
            var generator = new TypeRepositoryGenerator();
            var input = new[] { (typeof(TestDto).FullName, typeof(TestDtoWrapped).FullName), (typeof(TestDto).FullName, typeof(TestDtoWrapped).FullName) };
            Assert.Throws<TargetInvocationException>(() => GetTypeRepository(Compile(generator.CreateTypeRepository(input))));
        }

        private Assembly Compile(string code)
        {
            return SourceBuilder.CompileToAssembly(code, new[] { MetadataReference.CreateFromFile(typeof(TestDto).Assembly.Location) });
        }

        private ITypeRepository GetTypeRepository(Assembly assembly)
        {
            return assembly.GetType("LaDeak.JsonMergePatch.Generated.TypeRepository").GetProperty("Instance").GetValue(null) as ITypeRepository;
        }
    }
}

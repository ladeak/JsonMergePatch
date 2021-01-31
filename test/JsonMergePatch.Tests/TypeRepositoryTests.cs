using System;
using LaDeak.JsonMergePatch;
using Xunit;

namespace JsonMergePatch.Tests
{
    public class TypeRepositoryTests
    {
        [Fact]
        public void EmptyRepository_Add_DoesNotThrow()
        {
            var sut = new TypeRepository();
            sut.Add<TestDto, WrappedTestDto>();
        }

        [Fact]
        public void EmptyRepository_AddTwiceSameType_ThrowsException()
        {
            var sut = new TypeRepository();
            sut.Add<TestDto, WrappedTestDto>();
            Assert.Throws<ArgumentException>(() => sut.Add<TestDto, WrappedTestDto>());
        }

        [Fact]
        public void EmptyRepository_TryGet_ReturnsFalse()
        {
            var sut = new TypeRepository();
            Assert.False(sut.TryGet(typeof(TestDto), out _));
        }

        [Fact]
        public void RepositoryWitTestDto_TryGet_ReturnsTrue()
        {
            var sut = new TypeRepository();
            sut.Add<TestDto, WrappedTestDto>();
            Assert.True(sut.TryGet(typeof(TestDto), out _));
        }

        [Fact]
        public void RepositoryWitTestDto_TryGet_ReturnsRegisteredType()
        {
            var sut = new TypeRepository();
            sut.Add<TestDto, WrappedTestDto>();
            sut.TryGet(typeof(TestDto), out var result);
            Assert.Equal(typeof(WrappedTestDto), result);
        }

        [Fact]
        public void RepositoryWitTestDto_TryGet_ReturnsPatchOfUserType()
        {
            var sut = new TypeRepository();
            sut.Add<TestDto, WrappedTestDto>();
            sut.TryGet(typeof(TestDto), out var result);
            Assert.True(typeof(Patch<TestDto>).IsAssignableFrom(result));
        }
    }
}

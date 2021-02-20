using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.Tests
{
    public class HttpContentExtensionsTests
    {
        [Fact]
        public async Task NullContent_ReadJsonPatchAsync_ThrowsArgumentNullException()
        {
            HttpContent content = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => content.ReadJsonPatchAsync<TestDto>(Substitute.For<ITypeRepository>())).ConfigureAwait(false);
        }

        [Fact]
        public async Task NullRepository_ReadJsonPatchAsync_ThrowsArgumentNullException()
        {
            var content = Substitute.For<HttpContent>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => content.ReadJsonPatchAsync<TestDto>(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task NoRegistrationInRepository_ReadJsonPatchAsync_ThrowsArgumentException()
        {
            var content = Substitute.For<HttpContent>();
            var typeRepository = Substitute.For<ITypeRepository>();
            typeRepository.TryGet(typeof(TestDto), out Arg.Any<Type>()).Returns(false);
            await Assert.ThrowsAsync<ArgumentException>(() => content.ReadJsonPatchAsync<TestDto>(typeRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task NonMergePatchJsonContentType_ReadJsonPatchAsync_ReturnsNull()
        {
            var content = new ByteArrayContent(new byte[0]);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/not-merge-patch+json");
            var typeRepository = Substitute.For<ITypeRepository>();
            typeRepository.TryGet(typeof(TestDto), out Arg.Any<Type>()).Returns(callInfo => { callInfo[1] = typeof(WrappedTestDto); return true; });
            var result = await content.ReadJsonPatchAsync<TestDto>(typeRepository).ConfigureAwait(false);
            Assert.Null(result);
        }

        [Fact]
        public async Task InvalidEncoding_ReadJsonPatchAsync_ThrowsInvalidOperationException()
        {
            var content = CreateContentBytes();
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/merge-patch+json; charset=Invalid");
            var typeRepository = Substitute.For<ITypeRepository>();
            typeRepository.TryGet(typeof(TestDto), out Arg.Any<Type>()).Returns(callInfo => { callInfo[1] = typeof(WrappedTestDto); return true; });
            await Assert.ThrowsAsync<ArgumentException>(() => content.ReadJsonPatchAsync<TestDto>(typeRepository)).ConfigureAwait(false);
        }

        [Fact]
        public async Task EmptyEncoding_ReadJsonPatchAsync_UsesUtf8Encoding()
        {
            var content = CreateContentBytes();
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/merge-patch+json");
            var typeRepository = Substitute.For<ITypeRepository>();
            typeRepository.TryGet(typeof(TestDto), out Arg.Any<Type>()).Returns(callInfo => { callInfo[1] = typeof(WrappedTestDto); return true; });
            var result = await content.ReadJsonPatchAsync<TestDto>(typeRepository).ConfigureAwait(false);
            Assert.NotNull(result);
            Assert.Equal(1, result.ApplyOnDefault().Prop1);
        }

        [Fact]
        public async Task Utf32Encoding_ReadJsonPatchAsync_UsesUtf32Encoding()
        {
            var content = CreateContentBytes(encoding: Encoding.UTF32);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/merge-patch+json; charset=utf-32");
            var typeRepository = Substitute.For<ITypeRepository>();
            typeRepository.TryGet(typeof(TestDto), out Arg.Any<Type>()).Returns(callInfo => { callInfo[1] = typeof(WrappedTestDto); return true; });
            var result = await content.ReadJsonPatchAsync<TestDto>(typeRepository).ConfigureAwait(false);
            Assert.NotNull(result);
            Assert.Equal(1, result.ApplyOnDefault().Prop1);
        }

        [Fact]
        public async Task DoubleQuotesInCharset_ReadJsonPatchAsync_IgnoredWhenParsingEncoding()
        {
            var content = CreateContentBytes(encoding: Encoding.UTF32);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/merge-patch+json; charset=\"utf-32\"");
            var typeRepository = Substitute.For<ITypeRepository>();
            typeRepository.TryGet(typeof(TestDto), out Arg.Any<Type>()).Returns(callInfo => { callInfo[1] = typeof(WrappedTestDto); return true; });
            var result = await content.ReadJsonPatchAsync<TestDto>(typeRepository).ConfigureAwait(false);
            Assert.NotNull(result);
            Assert.Equal(1, result.ApplyOnDefault().Prop1);
        }

        private HttpContent CreateContentBytes(TestDto content = null, Encoding encoding = null)
        {
            content ??= new TestDto() { Prop1 = 1 };
            return new StringContent(JsonSerializer.Serialize(content), encoding);
        }
    }
}

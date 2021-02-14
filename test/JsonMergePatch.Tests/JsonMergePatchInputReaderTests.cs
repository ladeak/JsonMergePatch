using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.Tests
{
    public class JsonMergePatchInputReaderTests
    {
        [Fact]
        public void Contruction_Sets_SupportedMediaType()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            Assert.Contains("application/merge-patch+json", sut.SupportedMediaTypes);
        }

        [Fact]
        public void Contruction_Sets_SupportedEncodings()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            Assert.Contains(Encoding.UTF8, sut.SupportedEncodings);
            Assert.Contains(Encoding.Unicode, sut.SupportedEncodings);
        }

        [Fact]
        public void Contruction_Sets_SerializerOptions()
        {
            var options = new JsonOptions();
            var sut = new JsonMergePatchInputReader(options);
            Assert.Same(options.SerializerOptions, sut.SerializerOptions);
        }

        [Fact]
        public async Task NullArguments_ReadRequestBodyAsync_ThrowsArgumentNullException()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ReadRequestBodyAsync(null, null));
        }

        [Fact]
        public async Task NoTypeRepository_ReadRequestBodyAsync_ThrowsInvalidOperationException()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = Substitute.For<IServiceProvider>();
            var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8));
        }

        [Fact]
        public async Task NoTypeRepositoryRegistration_ReadRequestBodyAsync_ReturnsFailure()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            var httpContext = new DefaultHttpContext();
            var servicesProvider = Substitute.For<IServiceProvider>();
            servicesProvider.GetService(typeof(ITypeRepository)).Returns(Substitute.For<ITypeRepository>());
            httpContext.RequestServices = servicesProvider;
            var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

            var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

            Assert.Equal(InputFormatterResult.Failure(), result);
        }

        [Fact]
        public async Task InvalidJson_ReadRequestBodyAsync_ReturnsFailure()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            DefaultHttpContext httpContext = await CreateHttpContextAsync("invalid").ConfigureAwait(false);
            var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

            var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

            Assert.Equal(InputFormatterResult.Failure(), result);
            Assert.Equal(1, inputContext.ModelState.ErrorCount);
        }

        [Fact]
        public async Task ValidJson_ReadRequestBodyAsync_ReturnsFailure()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }").ConfigureAwait(false);
            var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

            var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

            Assert.Equal(5, ((WrappedTestDto)result.Model).Prop1);
        }

        [Fact]
        public async Task OtherEncodingJson_ReadRequestBodyAsync_ReturnsFailure()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }", Encoding.Unicode).ConfigureAwait(false);
            var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

            var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.Unicode);

            Assert.Equal(5, ((WrappedTestDto)result.Model).Prop1);
        }

        [Fact]
        public async Task NoPatchInputType_ReadRequestBodyAsync_ReturnsFailure()
        {
            var sut = new JsonMergePatchInputReader(new JsonOptions());
            DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }").ConfigureAwait(false);
            var inputContext = CreateInputFormatterContext(typeof(TestDto), httpContext);

            var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

            Assert.Equal(InputFormatterResult.Failure(), result);
        }

        private async Task<DefaultHttpContext> CreateHttpContextAsync(string requestBody = null, Encoding encoding = null)
        {
            var httpContext = new DefaultHttpContext();
            var servicesProvider = Substitute.For<IServiceProvider>();
            var typeRepository = Substitute.For<ITypeRepository>();
            typeRepository.TryGet(Arg.Is<Type>(x => x == typeof(TestDto)), out Arg.Any<Type>()).Returns(callInfo => { callInfo[1] = typeof(WrappedTestDto); return true; });
            servicesProvider.GetService(typeof(ITypeRepository)).Returns(typeRepository);
            httpContext.RequestServices = servicesProvider;
            if (string.IsNullOrEmpty(requestBody))
                return httpContext;

            var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, encoding: encoding ?? Encoding.UTF8, leaveOpen: true);
            await writer.WriteAsync(requestBody).ConfigureAwait(false);
            await writer.FlushAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);
            httpContext.Request.Body = memoryStream;

            return httpContext;
        }

        private InputFormatterContext CreateInputFormatterContext(
                Type modelType,
                HttpContext httpContext,
                string modelName = null)
        {
            var provider = new EmptyModelMetadataProvider();
            var metadata = provider.GetMetadataForType(modelType);

            return new InputFormatterContext(
                httpContext,
                modelName: modelName ?? string.Empty,
                modelState: new ModelStateDictionary(),
                metadata: metadata,
                readerFactory: (s, e) => new HttpRequestStreamReader(s, e),
                treatEmptyInputAsDefaultValue: false);
        }
    }
}

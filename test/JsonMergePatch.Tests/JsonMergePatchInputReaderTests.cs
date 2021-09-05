using System.Text;
using LaDeak.JsonMergePatch.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.AspNetCore.Tests;

public class JsonMergePatchInputReaderTests
{
    [Fact]
    public void NoTypeRepository_Constructor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new JsonMergePatchInputReader(new JsonOptions(), null));
    }

    [Fact]
    public void Contruction_Sets_SupportedMediaType()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), Substitute.For<ITypeRepository>());
        Assert.Contains("application/merge-patch+json", sut.SupportedMediaTypes);
    }

    [Fact]
    public void Contruction_Sets_SupportedEncodings()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), Substitute.For<ITypeRepository>());
        Assert.Contains(Encoding.UTF8, sut.SupportedEncodings);
        Assert.Contains(Encoding.Unicode, sut.SupportedEncodings);
    }

    [Fact]
    public void Contruction_Sets_SerializerOptions()
    {
        var options = new JsonOptions();
        var sut = new JsonMergePatchInputReader(options, Substitute.For<ITypeRepository>());
        Assert.Same(options.SerializerOptions, sut.SerializerOptions);
    }

    [Fact]
    public async Task NullArguments_ReadRequestBodyAsync_ThrowsArgumentNullException()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), Substitute.For<ITypeRepository>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ReadRequestBodyAsync(null, Encoding.UTF8));
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ReadRequestBodyAsync(CreateInputFormatterContext(typeof(Patch<TestDto>), new DefaultHttpContext()), null));
    }

    [Fact]
    public async Task NoTypeRepositoryRegistration_ReadRequestBodyAsync_ReturnsFailure()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), Substitute.For<ITypeRepository>());
        var httpContext = new DefaultHttpContext();
        var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

        var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

        Assert.Equal(InputFormatterResult.Failure(), result);
    }

    [Fact]
    public async Task InvalidJson_ReadRequestBodyAsync_ReturnsFailure()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), CreateTypeRepository());
        DefaultHttpContext httpContext = await CreateHttpContextAsync("invalid").ConfigureAwait(false);
        var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

        var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

        Assert.Equal(InputFormatterResult.Failure(), result);
        Assert.Equal(1, inputContext.ModelState.ErrorCount);
    }

    [Fact]
    public async Task ValidJson_ReadRequestBodyAsync_ReturnsFailure()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), CreateTypeRepository());
        DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }").ConfigureAwait(false);
        var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

        var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

        Assert.Equal(5, ((TestDtoWrapped)result.Model).Prop1);
    }

    [Fact]
    public async Task OtherEncodingJson_ReadRequestBodyAsync_ReturnsFailure()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), CreateTypeRepository());
        DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }", Encoding.Unicode).ConfigureAwait(false);
        var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

        var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.Unicode);

        Assert.Equal(5, ((TestDtoWrapped)result.Model).Prop1);
    }

    [Fact]
    public async Task NoPatchInputType_ReadRequestBodyAsync_ReturnsFailure()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), CreateTypeRepository());
        DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }").ConfigureAwait(false);
        var inputContext = CreateInputFormatterContext(typeof(TestDto), httpContext);

        var result = await sut.ReadRequestBodyAsync(inputContext, Encoding.UTF8);

        Assert.Equal(InputFormatterResult.Failure(), result);
    }

    [Fact]
    public async Task OpenPatchType_CanRead_ReturnsTrue()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), CreateTypeRepository());
        DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }").ConfigureAwait(false);
        var inputContext = CreateInputFormatterContext(typeof(Patch<>), httpContext);

        Assert.True(sut.CanRead(inputContext));
    }

    [Fact]
    public async Task ClosedPatchType_CanRead_ReturnsTrue()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), CreateTypeRepository());
        DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }").ConfigureAwait(false);
        var inputContext = CreateInputFormatterContext(typeof(Patch<TestDto>), httpContext);

        Assert.True(sut.CanRead(inputContext));
    }

    [Fact]
    public async Task NonPatchType_CanRead_ReturnsFalse()
    {
        var sut = new JsonMergePatchInputReader(new JsonOptions(), CreateTypeRepository());
        DefaultHttpContext httpContext = await CreateHttpContextAsync("{ \"Prop1\" : 5 }").ConfigureAwait(false);
        var inputContext = CreateInputFormatterContext(typeof(TestDto), httpContext);

        Assert.False(sut.CanRead(inputContext));
    }

    private ITypeRepository CreateTypeRepository()
    {
        var typeRepository = Substitute.For<ITypeRepository>();
        typeRepository.TryGet(Arg.Is<Type>(x => x == typeof(TestDto)), out Arg.Any<Type>()).Returns(callInfo => { callInfo[1] = typeof(TestDtoWrapped); return true; });
        return typeRepository;
    }

    private async Task<DefaultHttpContext> CreateHttpContextAsync(string requestBody = null, Encoding encoding = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.ContentType = "application/merge-patch+json";
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
            string modelName = null,
            bool emptyInputIsDefault = false)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForType(modelType);

        return new InputFormatterContext(
            httpContext,
            modelName: modelName ?? string.Empty,
            modelState: new ModelStateDictionary(),
            metadata: metadata,
            readerFactory: (s, e) => new HttpRequestStreamReader(s, e),
            treatEmptyInputAsDefaultValue: emptyInputIsDefault);
    }
}

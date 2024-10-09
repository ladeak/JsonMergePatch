﻿using System.Text;
using System.Text.Json;
using LaDeak.JsonMergePatch.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace LaDeak.JsonMergePatch.AspNetCore;

public class JsonMergePatchInputReader : TextInputFormatter
{
    private readonly ITypeRepository _typeRepository;

    public JsonSerializerOptions SerializerOptions { get; }

    public JsonMergePatchInputReader(JsonOptions? options = null, ITypeRepository? typeRepository = null)
    {
        options ??= new JsonOptions();
        SerializerOptions = options.SerializerOptions;
        _typeRepository = typeRepository ?? JsonMergePatchOptions.Repository ?? throw new ArgumentNullException(nameof(typeRepository));

        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/merge-patch+json"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanReadType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Patch<>);

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(encoding);

        object? model;
        var httpContext = context.HttpContext;
        var registeredType = context.ModelType.GenericTypeArguments.FirstOrDefault();
        if (registeredType == null || !_typeRepository.TryGet(registeredType, out var targetType))
        {
            return InputFormatterResult.Failure();
        }
        var (inputStream, usesTranscodingStream) = GetInputStream(httpContext, encoding);
        try
        {
            model = await JsonSerializer.DeserializeAsync(inputStream, targetType, SerializerOptions);
        }
        catch (JsonException jsonException)
        {
            var path = jsonException.Path ?? string.Empty;
            var formatterException = new InputFormatterException(jsonException.Message, jsonException);
            context.ModelState.TryAddModelError(path, formatterException, context.Metadata);
            return InputFormatterResult.Failure();
        }
        finally
        {
            if (usesTranscodingStream)
                await inputStream.DisposeAsync();
        }

        if (model == null && !context.TreatEmptyInputAsDefaultValue)
            return InputFormatterResult.NoValue();

        return InputFormatterResult.Success(model);
    }

    private (Stream inputStream, bool usesTranscodingStream) GetInputStream(HttpContext httpContext, Encoding encoding)
    {
        if (encoding.CodePage == Encoding.UTF8.CodePage)
        {
            return (httpContext.Request.Body, false);
        }

        var inputStream = Encoding.CreateTranscodingStream(httpContext.Request.Body, encoding, Encoding.UTF8, leaveOpen: true);
        return (inputStream, true);
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace LaDeak.JsonMergePatch
{
    public class JsonMergePatchInputReader : TextInputFormatter
    {
        public JsonSerializerOptions SerializerOptions { get; }

        public JsonMergePatchInputReader(JsonOptions options)
        {
            SerializerOptions = options.SerializerOptions;

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/merge-patch+json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanReadType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Patch<>);

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            object? model;
            var httpContext = context.HttpContext;
            var typeRepository = httpContext.RequestServices.GetRequiredService<ITypeRepository>();
            var registeredType = context.ModelType.GenericTypeArguments.FirstOrDefault();
            if (registeredType == null || !typeRepository.TryGet(registeredType, out var targetType))
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
}

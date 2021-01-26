using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonMergePatch.Shared;
using LaDeak.JsonMergePatch.Shared;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Http.Json;

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

        protected override bool CanReadType(Type type) => type.GetGenericTypeDefinition() == typeof(Patch<>);

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            var httpContext = context.HttpContext;
            var (inputStream, usesTranscodingStream) = GetInputStream(httpContext, encoding);

            var typeRepository = context.HttpContext.RequestServices.GetRequiredService<ITypeRepository>();
            if (!typeRepository.TryGet(context.ModelType.GenericTypeArguments.First(), out var targatType))
            {
                return InputFormatterResult.Failure();
            }
            object model = null;
            try
            {
                model = await JsonSerializer.DeserializeAsync(inputStream, targatType, SerializerOptions);
            }
            catch (JsonException jsonException)
            {
                var path = jsonException.Path;
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

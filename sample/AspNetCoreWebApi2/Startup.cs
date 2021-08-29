using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AspNetCoreWebApi2.Entities;
using LaDeak.JsonMergePatch.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace AspNetCoreWebApi2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddControllers();
            AddJsonMergePatch(mvcBuilder);
            services.AddHttpClient();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AspNetCoreWebApi2", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCoreWebApi2 v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public IMvcBuilder AddJsonMergePatch(IMvcBuilder builder)
        {
            LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeAspNetCoreWebApi2.Entities.TypeRepository.Instance;
            builder.Services.AddSingleton<LaDeak.JsonMergePatch.Abstractions.ITypeRepository>(LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository);
            var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();
            jsonOptions.SerializerOptions.AddContext<SampleJsonContext>();
            return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new LaDeak.JsonMergePatch.AspNetCore.JsonMergePatchInputReader(jsonOptions)));
        }
    }

    [JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreWebApi2.Entities.WeatherForecastWrapped))]
    [JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreWebApi2.Entities.CitiesDataWrapped))]
    [JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreWebApi2.Entities.DeviceDataWrapped))]
    public partial class SampleJsonContext : JsonSerializerContext
    {
    }
}

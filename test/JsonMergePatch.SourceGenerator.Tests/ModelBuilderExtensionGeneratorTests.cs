using System;
using System.Collections.Generic;
using System.Linq;
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests
{
    public class ModelBuilderExtensionGeneratorTests
    {
        [Fact]
        public void GeneratedCode_Compiles()
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.Abstractions.TypeRepository()");
            GetMethodDelegate(code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void NoTypeRepository_Compiles(string input)
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder(input);
            GetMethodDelegate(code);
        }

        [Fact]
        public void Adds_ITypeRepository_MvcOptions_ServiceRegistrations()
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.Abstractions.TypeRepository()");
            var mvcBuilder = Substitute.For<IMvcBuilder>();
            var services = Substitute.For<IServiceCollection>();
            mvcBuilder.Services.Returns(services);

            GetMethodDelegate(code)(mvcBuilder, null);

            services.Received().Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ITypeRepository)));
            services.Received().Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>)));
        }

        [Fact]
        public void AddsJsonMergePatchInputReader_ToMvcOptions()
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.Abstractions.TypeRepository()");
            var mvcBuilder = Substitute.For<IMvcBuilder>();
            var services = Substitute.For<IServiceCollection>();
            mvcBuilder.Services.Returns(services);

            IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions> optionsSetup = null;
            services.When(y => y.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>))))
                .Do(callInfo => optionsSetup = (callInfo[0] as ServiceDescriptor).ImplementationInstance as IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>);
            var options = new Microsoft.AspNetCore.Mvc.MvcOptions();

            GetMethodDelegate(code)(mvcBuilder, null);
            optionsSetup.Configure(options);

            Assert.Contains(options.InputFormatters, x => x is JsonMergePatchInputReader);
        }

        [Fact]
        public void PassesCustom_JsonOptions_JsonMergePatchInputReader()
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.Abstractions.TypeRepository()");
            var mvcBuilder = Substitute.For<IMvcBuilder>();
            var services = Substitute.For<IServiceCollection>();
            mvcBuilder.Services.Returns(services);

            IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions> optionsSetup = null;
            services.When(y => y.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>))))
                .Do(callInfo => optionsSetup = (callInfo[0] as ServiceDescriptor).ImplementationInstance as IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>);
            var options = new Microsoft.AspNetCore.Mvc.MvcOptions();

            var jsonOption = new JsonOptions();
            GetMethodDelegate(code)(mvcBuilder, jsonOption);
            optionsSetup.Configure(options);

            Assert.Same(jsonOption.SerializerOptions, options.InputFormatters.OfType<JsonMergePatchInputReader>().First().SerializerOptions);
        }

        private Func<IMvcBuilder, JsonOptions, IMvcBuilder> GetMethodDelegate(string code, IEnumerable<MetadataReference> metadataReferences = null)
        {
            return AspNetCoreSourceBuilder.GetMethod(AspNetCoreSourceBuilder.CompileMvcToAssembly(code, metadataReferences), "LaDeak.JsonMergePatch.Generated.MvcBuilderExtensions", "AddJsonMergePatch");
        }


    }
}

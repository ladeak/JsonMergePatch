using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests.TypeRepositoryHook()");
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
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests.TypeRepositoryHook()");
            var mvcBuilder = Substitute.For<IMvcBuilder>();
            var services = Substitute.For<IServiceCollection>();
            mvcBuilder.Services.Returns(services);

            GetMethodDelegate(code)(mvcBuilder, null, null);

            services.Received().Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ITypeRepository)));
            services.Received().Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>)));
        }

        [Fact]
        public void AddsJsonMergePatchInputReader_ToMvcOptions()
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests.TypeRepositoryHook()");
            var mvcBuilder = Substitute.For<IMvcBuilder>();
            var services = Substitute.For<IServiceCollection>();
            mvcBuilder.Services.Returns(services);

            IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions> optionsSetup = null;
            services.When(y => y.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>))))
                .Do(callInfo => optionsSetup = (callInfo[0] as ServiceDescriptor).ImplementationInstance as IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>);
            var options = new Microsoft.AspNetCore.Mvc.MvcOptions();

            GetMethodDelegate(code)(mvcBuilder, null, null);
            optionsSetup.Configure(options);

            Assert.Contains(options.InputFormatters, x => x is JsonMergePatchInputReader);
        }

        [Fact]
        public void PassesCustom_JsonOptions_JsonMergePatchInputReader()
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests.TypeRepositoryHook()");
            var mvcBuilder = Substitute.For<IMvcBuilder>();
            var services = Substitute.For<IServiceCollection>();
            mvcBuilder.Services.Returns(services);

            IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions> optionsSetup = null;
            services.When(y => y.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>))))
                .Do(callInfo => optionsSetup = (callInfo[0] as ServiceDescriptor).ImplementationInstance as IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>);
            var options = new Microsoft.AspNetCore.Mvc.MvcOptions();

            var jsonOption = new JsonOptions();
            GetMethodDelegate(code)(mvcBuilder, jsonOption, null);
            optionsSetup.Configure(options);

            Assert.Same(jsonOption.SerializerOptions, options.InputFormatters.OfType<JsonMergePatchInputReader>().First().SerializerOptions);
        }

        [Fact]
        public void PassesCustom_TypeRepository_RegistersToServices()
        {
            var sut = new ModelBuilderExtensionGenerator();
            var code = sut.CreateModelBuilder("new LaDeak.JsonMergePatch.SourceGenerator.AspNetCore.Tests.TypeRepositoryHook()");
            var mvcBuilder = Substitute.For<IMvcBuilder>();
            var services = Substitute.For<IServiceCollection>();
            mvcBuilder.Services.Returns(services);

            ITypeRepository registeredTypeRepository = null;
            services.When(y => y.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ITypeRepository))))
                .Do(callInfo => registeredTypeRepository = (callInfo[0] as ServiceDescriptor).ImplementationInstance as ITypeRepository);
            var options = new Microsoft.AspNetCore.Mvc.MvcOptions();

            var typeRepository = new TypeRepositoryHook();
            GetMethodDelegate(code)(mvcBuilder, null, typeRepository);

            Assert.Same(typeRepository, registeredTypeRepository);
        }

        private Func<IMvcBuilder, JsonOptions, ITypeRepository, IMvcBuilder> GetMethodDelegate(string code)
        {
            IEnumerable<MetadataReference> metadataReferences = new[] { MetadataReference.CreateFromFile(typeof(TypeRepositoryHook).Assembly.Location) };
            return AspNetCoreSourceBuilder.GetMethod(AspNetCoreSourceBuilder.CompileMvcToAssembly(code, metadataReferences), "LaDeak.JsonMergePatch.Generated.MvcBuilderExtensions", "AddJsonMergePatch");
        }
    }

    public class TypeRepositoryHook : ITypeRepository
    {
        public void Add<TSource, TWrapper>() where TWrapper : Patch<TSource>
        {
        }

        public void Add(Type source, Type wrapper)
        {
        }

        public IEnumerable<KeyValuePair<Type, Type>> GetAll() => Enumerable.Empty<KeyValuePair<Type, Type>>();

        public bool TryGet(Type source, [NotNullWhen(true)] out Type wrapper)
        {
            wrapper = null;
            return false;
        }
    }
}

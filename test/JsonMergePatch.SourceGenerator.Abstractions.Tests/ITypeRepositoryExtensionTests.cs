using System;
using System.Collections.Generic;
using System.Linq;
using LaDeak.JsonMergePatch.Abstractions;
using NSubstitute;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class ITypeRepositoryExtensionTests
    {
        [Fact]
        public void WithTypesInOther_ExtendAddsAllTo_TargetTypeRepository()
        {
            var testData = new[] { new KeyValuePair<Type, Type>(typeof(object), typeof(string)), new KeyValuePair<Type, Type>(typeof(object), typeof(int)) };
            var targetRespository = Substitute.For<ITypeRepository>();
            var sourceRepository = Substitute.For<ITypeRepository>();
            sourceRepository.GetAll().Returns(testData);

            var result = targetRespository.Extend(sourceRepository);

            Assert.Same(targetRespository, result);
            targetRespository.Received().Add(testData[0].Key, testData[0].Value);
            targetRespository.Received().Add(testData[1].Key, testData[1].Value);
        }

        [Fact]
        public void WithNoTypesInOther_ExtendAddsAllTo_TargetTypeRepository()
        {
            var testData = Enumerable.Empty<KeyValuePair<Type, Type>>();
            var targetRespository = Substitute.For<ITypeRepository>();
            var sourceRepository = Substitute.For<ITypeRepository>();
            sourceRepository.GetAll().Returns(testData);

            var result = targetRespository.Extend(sourceRepository);

            Assert.Same(targetRespository, result);
            targetRespository.DidNotReceiveWithAnyArgs().Add(null, null);
        }

        [Fact]
        public void WithNullTypesInOther_ExtendAddsAllTo_TargetTypeRepository()
        {
            var targetRespository = Substitute.For<ITypeRepository>();
            var sourceRepository = Substitute.For<ITypeRepository>();
            sourceRepository.GetAll().Returns((IEnumerable<KeyValuePair<Type, Type>>)null);

            var result = targetRespository.Extend(sourceRepository);

            Assert.Same(targetRespository, result);
            targetRespository.DidNotReceiveWithAnyArgs().Add(null, null);
        }

        [Fact]
        public void NullTarget_ThrowsArgumetNullExcepion()
        {
            ITypeRepository targetRespository = null;
            Assert.Throws<ArgumentNullException>(() => targetRespository.Extend(Substitute.For<ITypeRepository>()));
        }

        [Fact]
        public void NullSource_ThrowsArgumetNullExcepion()
        {
            ITypeRepository targetRespository = Substitute.For<ITypeRepository>();
            Assert.Throws<ArgumentNullException>(() => targetRespository.Extend(null));
        }
    }
}

using Collie.ServiceLookup;
using Collie.ServiceLookup.Expressions;
using Collie.Test.SampleServices;
using Moq;
using System;
using Xunit;

namespace Collie.Test
{

    public class ExpressionServiceFactoryTest
    {
        [Fact]
        public void TestBasicFactoryGeneration() {
            var factory = new ExpressionServiceFactory();
            var factoryMethod = factory.CreateFactory(typeof(CompositeServiceD));
            var serviceContainerMock = new Mock<ServiceContainer>();

            var expectedStack = new Type[] { typeof(CompositeServiceD) };
            var serviceAType = typeof(IServiceA);
            serviceContainerMock.Setup(sc => sc.GetServiceInternal(serviceAType, expectedStack))
                .Returns(() => new DefaultServiceA());
            //serviceContainerMock.Setup(sc => sc.GetServiceInternal(typeof(IServiceB), expectedStack))
            //    .Returns(() => new DefaultServiceB());


            factoryMethod(serviceContainerMock.Object, Array.Empty<Type>());
            //serviceContainerMock.Verify(sc => sc.GetImplementation(It.IsAny<ServiceIdentifier>(), It.IsAny<Type[]>()), Times.Exactly(3));
        }
    }
}

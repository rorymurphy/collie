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
            var serviceContainerMock = new Mock<IServiceContainerExtended>();

            var expectedStack = new Type[] { typeof(CompositeServiceD) };
            var serviceAType = typeof(IServiceA);
            serviceContainerMock.Setup(sc => sc.GetServiceInternal(typeof(IServiceA), expectedStack))
                .Returns(() => new DefaultServiceA())
                .Verifiable();
            serviceContainerMock.Setup(sc => sc.GetServiceInternal(typeof(IServiceB), expectedStack))
                .Returns(() => new DefaultServiceB())
                .Verifiable();
            serviceContainerMock.Setup(sc => sc.GetServiceInternal(typeof(IServiceC), expectedStack))
                .Returns(() => new DefaultServiceC())
                .Verifiable();

            factoryMethod(serviceContainerMock.Object, Array.Empty<Type>());

            serviceContainerMock.Verify();
        }
    }
}

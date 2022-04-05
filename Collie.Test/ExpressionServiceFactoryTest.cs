using Collie.Abstractions;
using Collie.ServiceLookup;
using Collie.ServiceLookup.Expressions;
using Collie.Test.SampleServices;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var instance = factoryMethod(serviceContainerMock.Object, Array.Empty<Type>());
            Assert.NotNull(instance);
            serviceContainerMock.Verify();
        }

        [Fact]
        public void TestMissingDependency()
        {
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

            Assert.Throws<MissingDependencyException>(() =>
            {
                var instance = factoryMethod(serviceContainerMock.Object, Array.Empty<Type>());
            });
        }

        [Fact]
        public void TestEnumerableFactoryGeneration()
        {
            var serviceType = typeof(IEnumerable<IServiceA>);

            var factory = new ExpressionServiceFactory();
            var factoryMethod = factory.CreateFactory(serviceType);
            var serviceContainerMock = new Mock<IServiceContainerExtended>();

            var serviceAIdentifier = new ServiceIdentifier(typeof(IServiceA), new Abstractions.ServiceDefinition(typeof(IServiceA), Abstractions.ServiceLifetime.Singleton, typeof(DefaultServiceA)));
            var serviceBIdentifier = new ServiceIdentifier(typeof(IServiceA), new Abstractions.ServiceDefinition(typeof(IServiceB), Abstractions.ServiceLifetime.Singleton, typeof(DefaultServiceB)));
            var serviceCIdentifier = new ServiceIdentifier(typeof(IServiceA), new Abstractions.ServiceDefinition(typeof(IServiceA), Abstractions.ServiceLifetime.Singleton, typeof(DefaultServiceC)));


            var expectedStack = new Type[] { serviceType };

            serviceContainerMock.Setup(sc => sc.GetImplementationTypes(typeof(IServiceA)))
                .Returns(() => new ServiceIdentifier[] {
                    serviceAIdentifier, serviceCIdentifier
                })
                .Verifiable();

            serviceContainerMock.Setup(sc => sc.GetImplementation(serviceAIdentifier, It.IsAny<Type[]>()))
                .Returns(() => new DefaultServiceA())
                .Verifiable();

            serviceContainerMock.Setup(sc => sc.GetImplementation(serviceCIdentifier, It.IsAny<Type[]>()))
                .Returns(() => new DefaultServiceC())
                .Verifiable();

            var instance = (IEnumerable<IServiceA>)factoryMethod(serviceContainerMock.Object, Array.Empty<Type>());
            Assert.NotNull(instance);
            Assert.Equal(2, instance.Count());
            Assert.Equal(1, instance.Count(i => i.GetType() == typeof(DefaultServiceA)));
            Assert.Equal(1, instance.Count(i => i.GetType() == typeof(DefaultServiceC)));
            serviceContainerMock.Verify();
        }

        [Fact]
        public void TestFunctionFactoryGeneration()
        {
            var factory = new ExpressionServiceFactory();
            var factoryMethod = factory.CreateFactory(typeof(CompositeServiceD), (IServiceContainer c) => new CompositeServiceD(c.GetService<IServiceA>(), c.GetService<IServiceB>(), c.GetService<IServiceC>()));
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

            var instance = factoryMethod(serviceContainerMock.Object, Array.Empty<Type>());
            Assert.NotNull(instance);
            serviceContainerMock.Verify();
        }
    }
}

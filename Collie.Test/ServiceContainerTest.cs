using Collie.Abstractions;
using Collie.ServiceLookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Collie.Test.SampleServices;

namespace Collie.Test
{
    public class ServiceContainerTest
    {
        [Fact]
        public void TestSingleTenantServiceResolution()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddScoped<IServiceB, DefaultServiceB>();
            catalog.AddTransient<IServiceC, DefaultServiceC>();

            IServiceContainer container = new ServiceContainer(catalog);

            var scopeBuilder = container.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(null);

            var scope1SvcA = scope1.GetService<IServiceA>();
            var scope1SvcB = scope1.GetService<IServiceB>();
            var scope1SvcC = scope1.GetService<IServiceC>();

            Assert.NotNull(scope1SvcA);
            Assert.NotNull(scope1SvcB);
            Assert.NotNull(scope1SvcC);
            Assert.Equal(scope1SvcA, scope1.GetService<IServiceA>());
            Assert.Equal(scope1SvcB, scope1.GetService<IServiceB>());
            Assert.NotEqual(scope1SvcC, scope1.GetService<IServiceC>());

        }

        [Fact]
        public void TestSingleTenantIntegralTypeResolution()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddScoped<IServiceB, DefaultServiceB>();
            catalog.AddTransient<int>(123);

            IServiceContainer container = new ServiceContainer(catalog);

            var scopeBuilder = container.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(null);

            var scope1SvcA = scope1.GetService<IServiceA>();
            var scope1SvcB = scope1.GetService<IServiceB>();

            Assert.NotNull(scope1SvcA);
            Assert.NotNull(scope1SvcB);
            Assert.Equal(123, scope1.GetService<int>());
            Assert.Equal(scope1SvcA, scope1.GetService<IServiceA>());
            Assert.Equal(scope1SvcB, scope1.GetService<IServiceB>());

        }

        [Fact]
        public void TestSingleTenantEnumerableResolution()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddScoped<IServiceB, DefaultServiceB>();
            catalog.AddTransient<IServiceA, DefaultServiceC>();

            IServiceContainer container = new ServiceContainer(catalog);

            var scopeBuilder = container.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(null);

            var scope1SvcA = scope1.GetService<IServiceA>();
            var scope1SvcAEnumerable = scope1.GetService<IEnumerable<IServiceA>>();

            Assert.Equal(typeof(DefaultServiceC), scope1SvcA.GetType());
            Assert.Equal(2, scope1SvcAEnumerable.Count());

        }

        [Fact]
        public void TestSingleTenantGenericResolution()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddScoped<IServiceB, DefaultServiceB>();
            catalog.AddTransient<IServiceA, DefaultServiceC>();
            catalog.Add(new ServiceDefinition(typeof(IServiceG<>), ServiceLifetime.Transient, typeof(DefaultServiceD<>)));

            IServiceContainer container = new ServiceContainer(catalog);

            var scopeBuilder = container.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(null);

            var scope1SvcD = scope1.GetService<IServiceG<int>>();

            Assert.NotNull(scope1SvcD);
        }

        [Fact]
        public void TestExplicitGenericDeclarationResolution()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddScoped<IServiceB, DefaultServiceB>();
            catalog.AddTransient<IServiceA, DefaultServiceC>();
            catalog.Add(new ServiceDefinition(typeof(IServiceG<>), ServiceLifetime.Scoped, typeof(DefaultServiceD<>)));
            catalog.AddScoped<IServiceG<int>, AlternateServiceD<int>>();

            IServiceContainer container = new ServiceContainer(catalog);

            var scopeBuilder = container.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(null);

            var scope1SvcD = scope1.GetService<IServiceG<int>>();

            Assert.NotNull(scope1SvcD);
            Assert.Equal(typeof(AlternateServiceD<int>), scope1SvcD.GetType());
        }

        [Fact]
        public void TestMultipleConstructorInstantiation()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddScoped<IServiceB, DefaultServiceB>();

            catalog.AddScoped<CompositeServiceE, CompositeServiceE>();

            IServiceContainer container = new ServiceContainer(catalog);

            var scopeBuilder = container.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(null);

            var svcE = scope1.GetService<CompositeServiceE>();

            Assert.NotNull(svcE);
            Assert.NotNull(svcE.ServiceA);
            Assert.NotNull(svcE.ServiceB);
            Assert.Null(svcE.ServiceC);

            var extraCatalog = new ServiceCatalog();
            extraCatalog.AddScoped<IServiceC, DefaultServiceC>();
            var scope2 = scopeBuilder.Create(extraCatalog);

            svcE = scope2.GetService<CompositeServiceE>();

            Assert.NotNull(svcE);
            Assert.NotNull(svcE.ServiceA);
            Assert.NotNull(svcE.ServiceB);
            Assert.NotNull(svcE.ServiceC);
        }

        [Fact]
        public void TestMultitenancyBasic()
        {
            int tenantId = 0;
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddTenantSingleton<IServiceB, DefaultServiceB>();
            catalog.AddScoped<Tuple<int>>(container => new Tuple<int>(tenantId++));

            IServiceContainer rootContainer = new ServiceContainer(catalog, container => container.GetService<Tuple<int>>(), typeof(Tuple<int>));

            var scopeBuilder = rootContainer.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(new ServiceCatalog());
            var scope1SvcB = scope1.GetService<IServiceB>();

            Assert.NotNull(scope1SvcB);
            Assert.Equal(scope1SvcB, scope1.GetService<IServiceB>());
            Assert.Equal(0, scope1.GetService<Tuple<int>>().Item1);

            var scope2 = scopeBuilder.Create(new ServiceCatalog());
            var scope2SvcB = scope2.GetService<IServiceB>();

            Assert.NotNull(scope2SvcB);
            Assert.Equal(scope2SvcB, scope2.GetService<IServiceB>());
            Assert.NotEqual(scope1SvcB, scope2SvcB);
            Assert.Equal(1, scope2.GetService<Tuple<int>>().Item1);

        }

        [Fact]
        public void TestMultitenancyEnumerable()
        {
            int tenantId = 0;
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddTenantSingleton<IServiceB, DefaultServiceB>();
            catalog.AddScoped<IServiceA, DefaultServiceC>();
            catalog.AddScoped<Tuple<int>>(container => new Tuple<int>(tenantId++));

            IServiceContainer container = new ServiceContainer(catalog, container => container.GetService<Tuple<int>>(), typeof(Tuple<int>));

            var globalSvcA = container.GetService<IEnumerable<IServiceA>>();
            Assert.NotNull(globalSvcA);
            Assert.Single(globalSvcA);

            var scopeBuilder = container.GetService<IScopeBuilder>();
            var scope1 = scopeBuilder.Create(new ServiceCatalog());
            var scope1SvcA = scope1.GetService<IEnumerable<IServiceA>>();

            Assert.NotNull(scope1SvcA);
            Assert.Equal(2, scope1SvcA.Count());
            Assert.Equal(0, scope1.GetService<Tuple<int>>().Item1);
        }
    }
}

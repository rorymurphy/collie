using Collie.Abstractions;
using Collie.Test.SampleServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Collie.Test
{
    public class DynamicRegistrationTest
    {
        [Fact]
        public void TestSingleTenantResolution()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            catalog.AddScoped<IServiceB, DefaultServiceB>();
            catalog.AddSingleton<IDynamicServiceRegistrar>(new DynamicRegistrar(services => services.AddTransient<IServiceC, DefaultServiceC>()));

            IServiceContainer container = new ServiceContainer(catalog, new ServiceContainerOptions());

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
        public void TestMultiTenantKeyModification()
        {
            var catalog = new ServiceCatalog();
            catalog.AddScoped<IServiceA, DefaultServiceA>();
            catalog.AddTenantSingleton<IServiceB>(sc =>
            {
                var id = sc.GetService<int>();
                return new DefaultServiceB();
            });

            catalog.AddSingleton<IDynamicServiceRegistrar>(new DynamicRegistrar(services =>
            {
                services.AddScoped<(int, string)>((1, "test"));
                services.AddScoped<IServiceA, ValueTypeDependentServiceA>();
            }));

            IServiceContainer rootContainer = new ServiceContainer(catalog, container => container.GetService<IServiceA>(), typeof(IServiceA), new ServiceContainerOptions());
            var scopeBuilder = rootContainer.GetService<IScopeBuilder>();
            Assert.Throws<DynamicRegistrationDependencyException>(() => scopeBuilder.Create(null));
        }

        class DynamicRegistrar : IDynamicServiceRegistrar
        {
            Action<IServiceCatalog> registrarAction;
            public DynamicRegistrar(Action<IServiceCatalog> registrarAction)
            {
                this.registrarAction = registrarAction;
            }

            public void RegisterServices(IServiceCatalog catalog)
            {
                registrarAction(catalog);
            }
        }
    }
}

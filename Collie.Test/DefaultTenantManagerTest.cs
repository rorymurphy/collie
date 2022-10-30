using Collie.Abstractions;
using Collie.ServiceLookup;
using Collie.Test.SampleServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Collie.Test
{
    public class DefaultTenantManagerTest
    {
        [Fact]
        public void TestReturnsSameInstanceWhileCaptured()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            int key = 0;
            catalog.AddScoped<int>(sc => key++);

            var serviceContainerOptions = new ServiceContainerOptions();
            var container = new ServiceContainer(catalog, sc => sc.GetService<int>(), typeof(int), serviceContainerOptions);

            var manager = new DefaultTenantManager(catalog, container, sc => sc.GetService<int>(), typeof(int), serviceContainerOptions.TenantCacheSize);

            var tenants = new ServiceContainer[100];
            tenants[0] = manager.CaptureTenant(0, serviceContainerOptions);
            tenants[1] = manager.CaptureTenant(1, serviceContainerOptions);

            for(int i=2; i < 100; i++)
            {
                tenants[i] = manager.CaptureTenant(i, serviceContainerOptions);
                manager.ReleaseTenant(i);
            }

            Assert.Equal(tenants[0], manager.CaptureTenant(0, serviceContainerOptions));
            Assert.Equal(tenants[1], manager.CaptureTenant(1, serviceContainerOptions));

            manager.ReleaseTenant(0);
            manager.ReleaseTenant(0);
            manager.ReleaseTenant(1);
            manager.ReleaseTenant(1);

            Assert.NotEqual(tenants[0], manager.CaptureTenant(0, serviceContainerOptions));
            Assert.NotEqual(tenants[1], manager.CaptureTenant(1, serviceContainerOptions));

            manager.ReleaseTenant(0);
            manager.ReleaseTenant(1);
        }

        [Fact]
        public void ThrowsExceptionWhenMaxLimitExceeded()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            int key = 0;
            catalog.AddScoped<int>(sc => key++);

            var serviceContainerOptions = new ServiceContainerOptions(0, 1);
            var container = new ServiceContainer(catalog, sc => sc.GetService<int>(), typeof(int), serviceContainerOptions);

            var manager = new DefaultTenantManager(catalog, container, sc => sc.GetService<int>(), typeof(int), serviceContainerOptions.TenantCacheSize, serviceContainerOptions.MaxTenantSize);

            var tenants = new ServiceContainer[100];

            Assert.Throws<TenantLimitExceededException>(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    tenants[i] = manager.CaptureTenant(i, serviceContainerOptions);
                }
             });
        }

        [Fact]
        public void RetainsInstancesUpToCacheLimit()
        {
            var catalog = new ServiceCatalog();
            catalog.AddSingleton<IServiceA, DefaultServiceA>();
            int key = 0;
            catalog.AddScoped<int>(sc => key++);

            var serviceContainerOptions = new ServiceContainerOptions(2);

            var container = new ServiceContainer(catalog, sc => sc.GetService<int>(), typeof(int), serviceContainerOptions);

            var manager = new DefaultTenantManager(catalog, container, sc => sc.GetService<int>(), typeof(int), serviceContainerOptions.TenantCacheSize);

            var tenants = new ServiceContainer[100];
            tenants[0] = manager.CaptureTenant(0, serviceContainerOptions);
            tenants[1] = manager.CaptureTenant(1, serviceContainerOptions);

            for (int i = 2; i < 100; i++)
            {
                tenants[i] = manager.CaptureTenant(i, serviceContainerOptions);
                manager.ReleaseTenant(i);
            }

            Assert.Equal(tenants[0], manager.CaptureTenant(0, serviceContainerOptions));
            Assert.Equal(tenants[1], manager.CaptureTenant(1, serviceContainerOptions));

            for (int i = 2; i < 100; i++)
            {
                Assert.NotEqual(tenants[i], manager.CaptureTenant(i, serviceContainerOptions));
                manager.ReleaseTenant(i);
            }
            manager.ReleaseTenant(0);
            manager.ReleaseTenant(1);
        }
    }
}

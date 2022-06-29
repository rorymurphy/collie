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

            var container = new ServiceContainer(catalog, sc => sc.GetService<int>(), typeof(int));

            var manager = new DefaultTenantManager(catalog, container, sc => sc.GetService<int>(), typeof(int), 0);

            var tenants = new ServiceContainer[100];
            tenants[0] = manager.CaptureTenant(0);
            tenants[1] = manager.CaptureTenant(1);

            for(int i=2; i < 100; i++)
            {
                tenants[i] = manager.CaptureTenant(i);
                manager.ReleaseTenant(i);
            }

            Assert.Equal(tenants[0], manager.CaptureTenant(0));
            Assert.Equal(tenants[1], manager.CaptureTenant(1));

            manager.ReleaseTenant(0);
            manager.ReleaseTenant(0);
            manager.ReleaseTenant(1);
            manager.ReleaseTenant(1);

            Assert.NotEqual(tenants[0], manager.CaptureTenant(0));
            Assert.NotEqual(tenants[1], manager.CaptureTenant(1));

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

            var container = new ServiceContainer(catalog, sc => sc.GetService<int>(), typeof(int));

            var manager = new DefaultTenantManager(catalog, container, sc => sc.GetService<int>(), typeof(int), 0, 1);

            var tenants = new ServiceContainer[100];

            Assert.Throws<TenantLimitExceededException>(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    tenants[i] = manager.CaptureTenant(i);
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

            var container = new ServiceContainer(catalog, sc => sc.GetService<int>(), typeof(int));

            var manager = new DefaultTenantManager(catalog, container, sc => sc.GetService<int>(), typeof(int), 2);

            var tenants = new ServiceContainer[100];
            tenants[0] = manager.CaptureTenant(0);
            tenants[1] = manager.CaptureTenant(1);

            for (int i = 2; i < 100; i++)
            {
                tenants[i] = manager.CaptureTenant(i);
                manager.ReleaseTenant(i);
            }

            Assert.Equal(tenants[0], manager.CaptureTenant(0));
            Assert.Equal(tenants[1], manager.CaptureTenant(1));

            for (int i = 2; i < 100; i++)
            {
                Assert.NotEqual(tenants[i], manager.CaptureTenant(i));
                manager.ReleaseTenant(i);
            }
            manager.ReleaseTenant(0);
            manager.ReleaseTenant(1);
        }
    }
}

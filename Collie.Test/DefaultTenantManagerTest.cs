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

            var container = new ServiceContainer(catalog, sc => sc.GetService<int>(), typeof(int))

            var manager = new DefaultTenantManager(catalog, container, sc => sc.GetService<int>(), typeof(int), 2);
        }

        [Fact]
        public void TestCreatesNewInstanceWhenOverLimit()
        {

        }

        [Fact]
        public void RetainsInstancesUpToCacheLimit()
        {

        }
    }
}

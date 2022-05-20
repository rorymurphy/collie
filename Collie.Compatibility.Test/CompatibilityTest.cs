using Collie.Test.SampleServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Collie.Compatibility.Test
{
    public class CompatibilityTest
    {

        protected (IServiceCollection, IServiceProvider) GetServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IServiceA, DefaultServiceA>();
            services.AddTenantSingleton<IServiceB, DefaultServiceB>();
            services.AddScoped<IServiceC, DefaultServiceC>();

            int key = 0;
            var provider = services.BuildCollieProvider(sc => key++, typeof(int));
            return (services, provider);
        }

        [Fact]
        public void TestBasicServiceResolution()
        {
            (_, var provider) = GetServices();

            var scope = provider.CreateScope().ServiceProvider;
            Assert.NotNull(scope.GetRequiredService<IServiceA>());
            Assert.NotNull(scope.GetRequiredService<IServiceB>());
            Assert.NotNull(scope.GetRequiredService<IServiceC>());
        }

        [Fact]
        public void TestScopes()
        {
            (_, var provider) = GetServices();

            var scope1 = provider.CreateScope().ServiceProvider;
            var scope2 = provider.CreateScope().ServiceProvider;

            var scope1SvcA = scope1.GetRequiredService<IServiceA>();
            var scope1SvcB = scope1.GetRequiredService<IServiceB>();
            var scope1SvcC = scope1.GetRequiredService<IServiceC>();

            var scope2SvcA = scope2.GetRequiredService<IServiceA>();
            var scope2SvcB = scope2.GetRequiredService<IServiceB>();
            var scope2SvcC = scope2.GetRequiredService<IServiceC>();

            Assert.NotEqual(scope1, scope2);
            Assert.NotNull(scope1SvcA);
            Assert.NotNull(scope1SvcB);
            Assert.NotNull(scope1SvcC);
            Assert.NotNull(scope2SvcA);
            Assert.NotNull(scope2SvcB);
            Assert.NotNull(scope2SvcC);

            Assert.Equal(scope1SvcA, scope2SvcA);
            Assert.NotEqual(scope1SvcB, scope2SvcB);
            Assert.NotEqual(scope1SvcC, scope2SvcC);
        }
    }
}

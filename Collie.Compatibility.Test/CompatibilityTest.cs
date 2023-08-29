using Collie.Abstractions;
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
            services.TenantedServiceCollection()
                .AddSingleton<IServiceB, DefaultServiceB>()
                .AddScoped<IServiceC, DefaultServiceC>();

            int key = 0;
            var providerFactory = new MultitenantServiceProviderFactory(sc => key++, typeof(int));
            var builder = providerFactory.CreateBuilder(services);
            var provider = providerFactory.CreateServiceProvider(builder);
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

        [Fact]
        public void TestTenantSingletonTenantFilters()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IServiceA, DefaultServiceA>();
            services.AddTenantSingleton<IServiceA, DefaultServiceC>(k => (int)k == 1);
            services.AddTenantSingleton<IServiceA>(sp => new DefaultServiceC(), k => (int)k == 2);
            services.TenantedServiceCollection()
                .AddSingleton<IServiceB, DefaultServiceB>()
                .AddScoped<IServiceC, DefaultServiceC>();

            int key = 0;
            var providerFactory = new MultitenantServiceProviderFactory(sc => key++, typeof(int));
            var builder = providerFactory.CreateBuilder(services);
            var provider = providerFactory.CreateServiceProvider(builder);

            var scope0 = provider.CreateScope();
            var scope1 = provider.CreateScope();
            var scope2 = provider.CreateScope();
            Assert.IsType<DefaultServiceA>(scope0.ServiceProvider.GetRequiredService<IServiceA>());
            Assert.IsType<DefaultServiceC>(scope1.ServiceProvider.GetRequiredService<IServiceA>());
            Assert.IsType<DefaultServiceC>(scope2.ServiceProvider.GetRequiredService<IServiceA>());
        }

        [Fact]
        public void TestScopedTenantFilters()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IServiceA, DefaultServiceA>();
            services.AddScoped<IServiceA, DefaultServiceC>(k => (int)k == 1);
            services.AddScoped<IServiceA>(sp => new DefaultServiceC(), k => (int)k == 2);
            services.TenantedServiceCollection()
                .AddSingleton<IServiceB, DefaultServiceB>()
                .AddScoped<IServiceC, DefaultServiceC>();

            int key = 0;
            var providerFactory = new MultitenantServiceProviderFactory(sc => key++, typeof(int));
            var builder = providerFactory.CreateBuilder(services);
            var provider = providerFactory.CreateServiceProvider(builder);

            var scope0 = provider.CreateScope();
            var scope1 = provider.CreateScope();
            var scope2 = provider.CreateScope();
            Assert.IsType<DefaultServiceA>(scope0.ServiceProvider.GetRequiredService<IServiceA>());
            Assert.IsType<DefaultServiceC>(scope1.ServiceProvider.GetRequiredService<IServiceA>());
            Assert.IsType<DefaultServiceC>(scope2.ServiceProvider.GetRequiredService<IServiceA>());
        }

        [Fact]
        public void TestDynamicRegistation()
        {
            var services = new ServiceCollection();
            services.AddScoped<IServiceA, DefaultServiceC>()
                .AddSingleton<IServiceB, DefaultServiceB>()
                .AddScoped<IServiceC, DefaultServiceC>()
                .AddSingleton<IDynamicServiceConfigurer>(new DynamicConfigurer(services => services.AddScoped<CompositeServiceD, CompositeServiceD>()));

            int key = 0;
            var providerFactory = new MultitenantServiceProviderFactory(sc => key++, typeof(int));
            var builder = providerFactory.CreateBuilder(services);
            var provider = providerFactory.CreateServiceProvider(builder);
            var scopeProvider = provider.CreateScope().ServiceProvider;
            Assert.NotNull(scopeProvider.GetService<CompositeServiceD>());
        }

        class DynamicConfigurer : IDynamicServiceConfigurer
        {
            Action<IServiceCollection> configureAction;
            public DynamicConfigurer(Action<IServiceCollection> configureAction)
            {
                this.configureAction = configureAction;
            }

            public void ConfigureServices(IServiceCollection services)
            {
                configureAction(services);
            }
        }
    }
}
